using RoR2;
using RoR2.UI;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Text;
using TMPro;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using MSU;
using RoR2.ContentManagement;
using System.Collections;

namespace SS2
{
    public class ExtraEquipmentManager
    {
        [SystemInitializer]
        public static void Init()
        {
            IL.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.RoR2.CharacterModel.Awake += CharacterModel_Awake;
            IL.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;
        }

        //vanilla only adds passivebuffdef from active equipment slot
        //if body has composite injector, we want them from all equipment slots
        //if body has multielite, use all elite equipments
        // hook runs after OnEquipmentLost and OnEquipmentGained, and before adding itembehaviors from elite buffs
        private static void CharacterBody_OnInventoryChanged(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            bool b = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdcI4(1),
                x => x.MatchStfld<CharacterBody>(nameof(CharacterBody.statsDirty))); // statsDirty = true;
            if (b)
            {
                c.Emit(OpCodes.Ldarg_0); //body
                c.EmitDelegate<Action<CharacterBody>>((body) =>
                {
                    bool hasInjector = body.inventory.GetItemCount(SS2Content.Items.CompositeInjector) > 0;
                    bool hasMultiElite = body.inventory.GetItemCount(SS2Content.Items.MultiElite) > 0;
                    if (!hasInjector && !hasMultiElite) return;

                    for (int i = 0; i < body.inventory.GetEquipmentSlotCount(); i++)
                    {
                        BuffDef buffDef = body.inventory.GetEquipment((uint)i).equipmentDef?.passiveBuffDef;
                        // no multielite means its always ok
                        // no injector means we only want elite
                        bool multiElite = !hasMultiElite || (!hasInjector && buffDef && buffDef.isElite);
                        if (buffDef && !body.HasBuff(buffDef) && multiElite)
                        {
                            body.AddBuff(buffDef);
                        }
                    }
                });
            }
            else
            {
                SS2Log.Warning("ExtraEquipmentManager.CharacterBody_OnEquipmentLost: ILHook failed.");
            }
        }

        #region Item Displays
        // make charactermodel use the first elite equipment for overlays instead of only in active slot
        private static void CharacterModel_UpdateOverlays(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            bool b = c.TryGotoNext(MoveType.After,
                x => x.MatchStfld<CharacterModel>(nameof(CharacterModel.activeOverlayCount)),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CharacterModel>(nameof(CharacterModel.inventoryEquipmentIndex))); // bad
            if (b)
            {
                c.Index++; // so bad
                           // EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(inventoryEquipmentIndex);
                c.Emit(OpCodes.Ldarg_0); // charactermodel
                c.EmitDelegate<Func<EquipmentDef, CharacterModel, EquipmentDef>>((ed, model) =>
                {
                    CharacterBody body = model.body;
                    // return TRUE if we want to remove the buff
                    // return FALSE if we want to skip removing the buff
                    if (body && body.inventory)
                    {
                        bool hasInjector = body.inventory.GetItemCount(SS2Content.Items.CompositeInjector) > 0;
                        bool hasMultiElite = body.inventory.GetItemCount(SS2Content.Items.MultiElite) > 0;
                        if(hasInjector || hasMultiElite)
                        {
                            for (int i = 0; i < body.inventory.GetEquipmentSlotCount(); i++)
                            {
                                EquipmentState state = body.inventory.GetEquipment((uint)i);
                                if (state.equipmentDef?.passiveBuffDef?.eliteDef?.eliteIndex != null)
                                {
                                    return state.equipmentDef;
                                }
                            }
                        }                        
                    }
                    return ed;
                });
            }
            else
            {
                SS2Log.Error("ExtraEquipmentManager.CharacterModel_UpdateOverlays: ILHook failed.");
            }
        }
        //add a component to CharacterModel that handles extra equipment displays
        private static void CharacterModel_Awake(On.RoR2.CharacterModel.orig_Awake orig, CharacterModel self)
        {
            orig(self);
            self.gameObject.AddComponent<ExtraEquipmentDisplays>();
        }

        // keep itemdisplays of all equipment in inventory, if body has composite injector.
        // mostly copypasted functionality of charactermodel, except it keeps more than one equipment display active
        private class ExtraEquipmentDisplays : MonoBehaviour
        {
            private CharacterModel model;
            private ChildLocator childLocator;
            private List<CharacterModel.ParentedPrefabDisplay> parentedPrefabDisplays = new List<CharacterModel.ParentedPrefabDisplay>();
            private List<CharacterModel.LimbMaskDisplay> limbMaskDisplays = new List<CharacterModel.LimbMaskDisplay>();
            private List<EquipmentIndex> enabledEquipmentDisplays = new List<EquipmentIndex>();

            private void Awake()
            {
                this.model = base.GetComponent<CharacterModel>();
                this.childLocator = base.GetComponent<ChildLocator>();
            }
            private void OnEnable()
            {
                if (model.body) model.body.onInventoryChanged += OnInventoryChanged;
            }
            private void OnDisable()
            {
                if (model.body) model.body.onInventoryChanged -= OnInventoryChanged;
            }

            private void OnInventoryChanged()
            {
                EquipmentIndex equipmentIndex = 0;
                EquipmentIndex equipmentCount = (EquipmentIndex)EquipmentCatalog.equipmentCount;
                while (equipmentIndex < equipmentCount)
                {
                    // only show multiple equipments with composite injector, to not interfere with toolbot
                    // also dont want to re enable CharacterModel's equipment display
                    bool hasInjector = model.body.inventory.GetItemCount(SS2Content.Items.CompositeInjector) > 0;
                    EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
                    bool hasMultiElite = model.body.inventory.GetItemCount(SS2Content.Items.MultiElite) > 0 && equipmentDef && equipmentDef.passiveBuffDef && equipmentDef.passiveBuffDef.isElite;
                    if (HasEquipment(equipmentIndex) && (hasInjector || hasMultiElite))
                    {
                        this.EnableEquipmentDisplay(equipmentIndex);
                    }
                    else
                    {
                        this.DisableEquipmentDisplay(equipmentIndex);
                    }
                    equipmentIndex++;
                }
            }
            private void EnableEquipmentDisplay(EquipmentIndex index)
            {
                if (this.enabledEquipmentDisplays.Contains(index))
                {
                    return;
                }
                this.enabledEquipmentDisplays.Add(index);
                if (model.itemDisplayRuleSet)
                {
                    DisplayRuleGroup itemDisplayRuleGroup = model.itemDisplayRuleSet.GetEquipmentDisplayRuleGroup(index);
                    this.InstantiateDisplayRuleGroup(itemDisplayRuleGroup, index);
                }
            }
            private void DisableEquipmentDisplay(EquipmentIndex index)
            {
                if (!this.enabledEquipmentDisplays.Contains(index))
                {
                    return;
                }
                this.enabledEquipmentDisplays.Remove(index);
                for (int i = this.parentedPrefabDisplays.Count - 1; i >= 0; i--)
                {
                    if (this.parentedPrefabDisplays[i].equipmentIndex == index)
                    {
                        this.parentedPrefabDisplays[i].Undo();
                        this.parentedPrefabDisplays.RemoveAt(i);
                    }
                }
                for (int j = this.limbMaskDisplays.Count - 1; j >= 0; j--)
                {
                    if (this.limbMaskDisplays[j].equipmentIndex == index)
                    {
                        this.limbMaskDisplays[j].Undo(model);
                        this.limbMaskDisplays.RemoveAt(j);
                    }
                }
            }
            private bool HasEquipment(EquipmentIndex index)
            {
                Inventory inventory = model.body.inventory;
                //dont use first equipment slot because charactermodel already uses it
                EquipmentIndex first = inventory.GetEquipment(0).equipmentIndex;
                for (int i = 1; i < inventory.GetEquipmentSlotCount(); i++)
                {
                    EquipmentIndex equipmentIndex = inventory.GetEquipment((uint)i).equipmentIndex;
                    if (equipmentIndex == index && equipmentIndex != first) return true;
                }
                return false;
            }

            private void InstantiateDisplayRuleGroup(DisplayRuleGroup displayRuleGroup, EquipmentIndex equipmentIndex)
            {
                if (displayRuleGroup.rules != null)
                {
                    for (int i = 0; i < displayRuleGroup.rules.Length; i++)
                    {
                        ItemDisplayRule itemDisplayRule = displayRuleGroup.rules[i];
                        ItemDisplayRuleType ruleType = itemDisplayRule.ruleType;
                        if (ruleType != ItemDisplayRuleType.ParentedPrefab)
                        {
                            if (ruleType == ItemDisplayRuleType.LimbMask)
                            {
                                CharacterModel.LimbMaskDisplay item = new CharacterModel.LimbMaskDisplay
                                {
                                    equipmentIndex = equipmentIndex
                                };
                                item.Apply(model, itemDisplayRule.limbMask);
                                this.limbMaskDisplays.Add(item);
                            }
                        }
                        else if (this.childLocator)
                        {
                            Transform transform = this.childLocator.FindChild(itemDisplayRule.childName);
                            if (transform)
                            {
                                CharacterModel.ParentedPrefabDisplay item2 = new CharacterModel.ParentedPrefabDisplay
                                {
                                    equipmentIndex = equipmentIndex
                                };
                                item2.Apply(model, itemDisplayRule.followerPrefab, transform, itemDisplayRule.localPos, Quaternion.Euler(itemDisplayRule.localAngles), itemDisplayRule.localScale);
                                this.parentedPrefabDisplays.Add(item2);
                            }
                        }
                    }
                }
            }
        }
        #endregion

    }
}
