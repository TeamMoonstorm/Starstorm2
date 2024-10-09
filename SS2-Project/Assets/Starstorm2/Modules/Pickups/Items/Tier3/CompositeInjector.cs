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

namespace SS2.Items
{
    // welcome to hell
    //N: This is fucking insanity, LOL
    public sealed class CompositeInjector : SS2Item
    {
        private const string token = "SS2_ITEM_COMPOSITEINJECTOR_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acCompositeInjector", SS2Bundle.Items);

        private static GameObject _useEffect;

        public static int funnyNumber = 16;
        public static float funnyNumber2 = 60f; // fuck unity ui. its going off the screen. i dont fcare
        public static Vector3 funnyVector = new Vector3(-72f, 0f, 0f);

        public override void Initialize()
        {
            _useEffect = AssetCollection.FindAsset<GameObject>("CompositeInjectorEffect");
            // 10 hooks lol
            var hook = new Hook(typeof(EquipmentIcon).GetMethod(nameof(EquipmentIcon.GenerateDisplayData), (System.Reflection.BindingFlags)(-1)), typeof(CompositeInjector).GetMethod(nameof(EquipmentIcon_GenerateDisplayData), (System.Reflection.BindingFlags)(-1)));
            On.RoR2.UI.HUD.Awake += AddIcons;
            On.RoR2.EquipmentDef.AttemptGrant += EquipmentDef_AttemptGrant;
            EquipmentSlot.onServerEquipmentActivated += ActivateAllEquipment;
            IL.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            IL.RoR2.Inventory.UpdateEquipment += Inventory_UpdateEquipment;
            On.RoR2.CharacterModel.Awake += CharacterModel_Awake;
            IL.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;
            On.RoR2.EquipmentSlot.Update += EquipmentSlot_Update;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        #region Gameplay Mechanics
        private void ActivateAllEquipment(EquipmentSlot self, EquipmentIndex equipmentIndex)
        {
            Inventory inventory = self.characterBody.inventory;
            if (inventory && inventory.GetItemCount(SS2Content.Items.CompositeInjector) <= 0) return;

            EquipmentDef first = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
            float initialCooldown = first.cooldown;
            float highestCooldown = initialCooldown;
            for (int i = 0; i < inventory.GetEquipmentSlotCount(); i++)
            {
                EquipmentState state = inventory.GetEquipment((uint)i);
                if (i != inventory.activeEquipmentSlot && state.equipmentIndex != EquipmentIndex.None)
                {
                    EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(state.equipmentIndex);
                    if (self.PerformEquipmentAction(equipmentDef) && equipmentDef.cooldown > highestCooldown)
                    {
                        highestCooldown = equipmentDef.cooldown;
                    }
                }
            }

            EffectData effectData = new EffectData();
            effectData.origin = self.characterBody.corePosition;
            effectData.SetNetworkedObjectReference(self.gameObject);
            EffectManager.SpawnEffect(_useEffect, effectData, true);
        }

        // set all equipment cooldowns to the highest cooldown equipment, if inventory has composite injector
        private void Inventory_UpdateEquipment(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            bool b = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(3),
                x => x.MatchLdfld<EquipmentState>(nameof(EquipmentState.equipmentDef)),
                x => x.MatchLdfld<EquipmentDef>(nameof(EquipmentDef.cooldown))); // float num2 = equipmentState.equipmentDef.cooldown(here) * CalculateEquipmentCooldownScale();
            if (b)
            {
                c.Emit(OpCodes.Ldarg_0); //inventory
                c.EmitDelegate<Func<float, Inventory, float>>((cd, inv) =>
                {
                    if (inv.GetItemCount(SS2Content.Items.CompositeInjector) <= 0) return cd;

                    float highestCooldown = cd;
                    for (int i = 0; i < inv.GetEquipmentSlotCount(); i++)
                    {
                        float cooldown = inv.GetEquipment((uint)i).equipmentDef?.cooldown ?? 0;
                        if (cooldown > highestCooldown)
                        {
                            highestCooldown = cooldown;
                        }
                    }
                    return highestCooldown;
                });
            }
            else
            {
                SS2Log.Warning("CompositeInjector.CharacterBody_OnEquipmentLost: ILHook failed.");
            }
        }

        // when picking up new equipment, move it to first slot, move old equipment to third slot (2 is toolbot), move evreything else upwards, pop out last equipment
        private void EquipmentDef_AttemptGrant(On.RoR2.EquipmentDef.orig_AttemptGrant orig, ref PickupDef.GrantContext context)
        {
            Inventory inventory = context.body.inventory;
            EquipmentState oldEquipmentState = inventory.currentEquipmentState;

            int stack = inventory.GetItemCount(SS2Content.Items.CompositeInjector);
            EquipmentState lastEquipmentState = inventory.GetEquipment(2 + (uint)stack - 1);

            bool shouldDestroy = false;
            if (stack > 0 && oldEquipmentState.equipmentIndex != EquipmentIndex.None)
            {
                //move equipment upwards
                for (int i = 2; i < 2 + stack; i++)
                {
                    EquipmentState newEquipmentState = oldEquipmentState;
                    oldEquipmentState = inventory.GetEquipment((uint)i);
                    // SetEquipmentInternal doesnt call onInventoryChanged each time like SetEquipment does, so its ideal.
                    // unfortunatenyl im too fucking raetearred
                    inventory.SetEquipment(newEquipmentState, (uint)i);

                    if (oldEquipmentState.Equals(EquipmentState.empty))
                    {
                        shouldDestroy = true; // destroy pickup since its in our inventory now		
                        break;
                    }
                }
            }

            orig(ref context); // orig calls OnInventoryChanged
            context.shouldDestroy |= shouldDestroy;

            if (stack <= 0 || oldEquipmentState.equipmentIndex == EquipmentIndex.None) return;

            //pop out last equipment		
            PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(lastEquipmentState.equipmentIndex);
            context.controller.NetworkpickupIndex = pickupIndex;
            if (pickupIndex == PickupIndex.none)
            {
                context.shouldDestroy = true;
            }
            else
            {
                // AAAAAAAAAAAAAAAAAAAAAAAAHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH
                context.body.OnEquipmentLost(lastEquipmentState.equipmentDef);
            }
        }

        //vanilla only adds passivebuffdef from active equipment slot
        //if body has composite injector, we want them from all equipment slots
        // hook runs after OnEquipmentLost and OnEquipmentGained, and before adding itembehaviors from elite buffs
        private void CharacterBody_OnInventoryChanged(ILContext il)
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
                    if (body.inventory.GetItemCount(SS2Content.Items.CompositeInjector) <= 0) return;

                    for (int i = 0; i < body.inventory.GetEquipmentSlotCount(); i++)
                    {
                        BuffDef buffDef = body.inventory.GetEquipment((uint)i).equipmentDef?.passiveBuffDef;
                        if (buffDef && !body.HasBuff(buffDef))
                        {
                            body.AddBuff(buffDef);
                        }
                    }
                });
            }
            else
            {
                SS2Log.Warning("CompositeInjector.CharacterBody_OnEquipmentLost: ILHook failed.");
            }

        }

        // display indicator of the first equipment that uses one
        // would be cool to get all the indicators but it would be very annoying to do
        private void EquipmentSlot_Update(On.RoR2.EquipmentSlot.orig_Update orig, EquipmentSlot self)
        {
            orig(self);
            if (self.inventory?.GetItemCount(SS2Content.Items.CompositeInjector) > 0)
            {
                self.targetIndicator.active = false;
                for (int i = 0; i < self.inventory.GetEquipmentSlotCount(); i++)
                {
                    EquipmentState state = self.inventory.GetEquipment((uint)i);
                    if (state.equipmentIndex != self.equipmentIndex)
                    {
                        self.UpdateTargets(state.equipmentIndex, self.stock > 0);
                    }

                    if (self.targetIndicator.active) break; // use the indicator of the first equipment that uses one			
                }
            }
        }
        #endregion

        #region Item Displays
        // make charactermodel use the first elite equipment for overlays instead of only in active slot
        private void CharacterModel_UpdateOverlays(ILContext il)
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
                    if (body?.inventory?.GetItemCount(SS2Content.Items.CompositeInjector) > 0)
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
                    return ed;
                });
            }
            else
            {
                SS2Log.Error("CompositeInjector.CharacterModel_UpdateOverlays: ILHook failed.");
            }
        }

        //add a component to CharacterModel that handles extra equipment displays
        private void CharacterModel_Awake(On.RoR2.CharacterModel.orig_Awake orig, CharacterModel self)
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
                    if (HasEquipment(equipmentIndex) && model.body.inventory.GetItemCount(SS2Content.Items.CompositeInjector) > 0)
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

        #region	HUD stuff
        // dont display mul-t's alt equipment slot if we just have composite injector
        private static EquipmentIcon.DisplayData EquipmentIcon_GenerateDisplayData(Func<EquipmentIcon, EquipmentIcon.DisplayData> orig, EquipmentIcon self)
        {
            EquipmentIcon.DisplayData result = orig(self);
            if (self.displayAlternateEquipment && self.targetInventory)
            {
                int stacks = self.targetInventory.GetItemCount(SS2Content.Items.CompositeInjector);
                bool shouldHide;
                if (stacks <= 0) shouldHide = (self.targetInventory.GetEquipmentSlotCount() <= 1); // vanilla condition. this could just be replaced with our condition but im being safe.
                else shouldHide = self.targetInventory.GetEquipment(1).Equals(EquipmentState.empty); // otherwise only hide it if empty

                result.hideEntireDisplay = shouldHide;
            }
            return result;
        }



        //its fuckeed is so fucked
        private void AddIcons(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);

            
            Transform scaler = self.transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomRightCluster/Scaler");
            Transform slot = scaler.Find("AltEquipmentSlot");
            if (!scaler || !slot) return;
            IconHolder epic = self.gameObject.AddComponent<IconHolder>();
            epic.hud = self;
            epic.icons = new EquipmentIconButEpic[funnyNumber];
            for (int i = 1; i <= funnyNumber; i++)
            {
                Transform newIcon = GameObject.Instantiate(slot.gameObject, slot.parent).transform;
                newIcon.SetParent(slot.parent, false);
                newIcon.transform.position += funnyVector + (Vector3.up * funnyNumber2 * i);

                EquipmentIcon oldIcon = newIcon.GetComponent<EquipmentIcon>();
                EquipmentIconButEpic icon = newIcon.gameObject.AddComponent<EquipmentIconButEpic>();
                icon.targetSlotIndex = (uint)i + 1;
                epic.icons[i - 1] = icon;

                GameObject.Destroy(oldIcon.cooldownText);
                GameObject.Destroy(oldIcon.stockText);
                icon.displayRoot = oldIcon.displayRoot;
                icon.iconImage = oldIcon.iconImage;
                icon.isAutoCastPanelObject = oldIcon.isAutoCastPanelObject;
                icon.tooltipProvider = oldIcon.tooltipProvider;
                icon.displayAlternateEquipment = true;
                GameObject.Destroy(oldIcon);
            }
        }


        public class IconHolder : MonoBehaviour
        {
            public HUD hud;
            public EquipmentIconButEpic[] icons;

            private void Update()
            {
                if(!hud)
                {
                    Destroy(this);
                    return;
                }
                foreach (EquipmentIconButEpic epic in icons)
                    if(epic) 
                        epic.targetInventory = hud.targetMaster != null ? hud.targetMaster.inventory : null;
            }
        }

        // copypaste of ror2.ui.equipmenticon but we get to choose the equipmentslot
        // keeping the hex tokens and redudant stuff to remind me of my failure
        // N: I removed the hex tokens, oops.

        public class EquipmentIconButEpic : MonoBehaviour
        {
            public uint targetSlotIndex;

            public Inventory targetInventory;

            public EquipmentSlot targetEquipmentSlot;

            public GameObject displayRoot;

            public PlayerCharacterMasterController playerCharacterMasterController;

            public RawImage iconImage;

            public TextMeshProUGUI cooldownText;

            public TextMeshProUGUI stockText;

            public GameObject stockFlashPanelObject;

            public GameObject reminderFlashPanelObject;

            public GameObject isReadyPanelObject;

            public GameObject isAutoCastPanelObject;

            public TooltipProvider tooltipProvider;

            public bool displayAlternateEquipment;

            private int previousStockCount;

            private float equipmentReminderTimer;

            private EquipmentIconButEpic.DisplayData currentDisplayData;

            public bool hasEquipment
            {
                get
                {
                    return this.currentDisplayData.hasEquipment;
                }
            }

            // Token: 0x06004BC3 RID: 19395 RVA: 0x00137FB8 File Offset: 0x001361B8
            private void SetDisplayData(EquipmentIconButEpic.DisplayData newDisplayData)
            {
                if (!this.currentDisplayData.isReady && newDisplayData.isReady)
                {
                    this.DoStockFlash();
                }
                if (this.displayRoot)
                {
                    this.displayRoot.SetActive(!newDisplayData.hideEntireDisplay);
                }
                if (newDisplayData.stock > this.currentDisplayData.stock)
                {
                    Util.PlaySound("Play_item_proc_equipMag", RoR2Application.instance.gameObject);
                    this.DoStockFlash();
                }
                if (this.isReadyPanelObject)
                {
                    this.isReadyPanelObject.SetActive(newDisplayData.isReady);
                }
                if (this.isAutoCastPanelObject)
                {
                    if (this.targetInventory)
                    {
                        this.isAutoCastPanelObject.SetActive(this.targetInventory.GetItemCount(RoR2Content.Items.AutoCastEquipment) > 0);
                    }
                    else
                    {
                        this.isAutoCastPanelObject.SetActive(false);
                    }
                }
                if (this.iconImage)
                {
                    Texture texture = null;
                    Color color = Color.clear;
                    if (newDisplayData.equipmentDef != null)
                    {
                        color = ((newDisplayData.stock > 0) ? Color.white : Color.gray);
                        texture = newDisplayData.equipmentDef.pickupIconTexture;
                    }
                    this.iconImage.texture = texture;
                    this.iconImage.color = color;
                }
                if (this.cooldownText)
                {
                    this.cooldownText.gameObject.SetActive(newDisplayData.showCooldown);
                    if (newDisplayData.cooldownValue != this.currentDisplayData.cooldownValue)
                    {
                        StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
                        stringBuilder.AppendInt(newDisplayData.cooldownValue, 1U, uint.MaxValue);
                        this.cooldownText.SetText(stringBuilder);
                        HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
                    }
                }
                if (this.stockText)
                {
                    if (newDisplayData.hasEquipment && (newDisplayData.maxStock > 1 || newDisplayData.stock > 1))
                    {
                        this.stockText.gameObject.SetActive(true);
                        StringBuilder stringBuilder2 = HG.StringBuilderPool.RentStringBuilder();
                        stringBuilder2.AppendInt(newDisplayData.stock, 1U, uint.MaxValue);
                        this.stockText.SetText(stringBuilder2);
                        HG.StringBuilderPool.ReturnStringBuilder(stringBuilder2);
                    }
                    else
                    {
                        this.stockText.gameObject.SetActive(false);
                    }
                }
                string titleToken = null;
                string bodyToken = null;
                Color titleColor = Color.white;
                Color gray = Color.gray;
                if (newDisplayData.equipmentDef != null)
                {
                    titleToken = newDisplayData.equipmentDef.nameToken;
                    bodyToken = newDisplayData.equipmentDef.pickupToken;
                    titleColor = ColorCatalog.GetColor(newDisplayData.equipmentDef.colorIndex);
                }
                if (this.tooltipProvider)
                {
                    this.tooltipProvider.titleToken = titleToken;
                    this.tooltipProvider.titleColor = titleColor;
                    this.tooltipProvider.bodyToken = bodyToken;
                    this.tooltipProvider.bodyColor = gray;
                }
                this.currentDisplayData = newDisplayData;
            }

            // Token: 0x06004BC4 RID: 19396 RVA: 0x0013825C File Offset: 0x0013645C
            private void DoReminderFlash()
            {
                if (this.reminderFlashPanelObject)
                {
                    AnimateUIAlpha component = this.reminderFlashPanelObject.GetComponent<AnimateUIAlpha>();
                    if (component)
                    {
                        component.time = 0f;
                    }
                    this.reminderFlashPanelObject.SetActive(true);
                }
                this.equipmentReminderTimer = 5f;
            }

            // Token: 0x06004BC5 RID: 19397 RVA: 0x001382AC File Offset: 0x001364AC
            private void DoStockFlash()
            {
                this.DoReminderFlash();
                if (this.stockFlashPanelObject)
                {
                    AnimateUIAlpha component = this.stockFlashPanelObject.GetComponent<AnimateUIAlpha>();
                    if (component)
                    {
                        component.time = 0f;
                    }
                    this.stockFlashPanelObject.SetActive(true);
                }
            }

            // Token: 0x06004BC6 RID: 19398 RVA: 0x001382F8 File Offset: 0x001364F8
            private EquipmentIconButEpic.DisplayData GenerateDisplayData()
            {
                EquipmentIconButEpic.DisplayData result = default(EquipmentIconButEpic.DisplayData);
                EquipmentIndex equipmentIndex = EquipmentIndex.None;
                if (this.targetInventory)
                {
                    EquipmentState equipmentState;
                    if (this.displayAlternateEquipment)
                    {

                        //equipmentState = this.targetInventory.alternateEquipmentState; // VANILLA CODE /////////////////////////////////////////////////////////
                        //result.hideEntireDisplay = (this.targetInventory.GetEquipmentSlotCount() <= 1);

                        // NEW /////////////////////////////////////////////////////////
                        // DISPLAY NUMBER OF INJECTOR SLOTS EQUAL TO NUMBER OF COMPOSITE INJECTORS INVENTORY HAS
                        equipmentState = this.targetInventory.GetEquipment(this.targetSlotIndex);
                        int stack = this.targetInventory.GetItemCount(SS2Content.Items.CompositeInjector);
                        result.hideEntireDisplay = (stack + 1 < this.targetSlotIndex); // first injector icon is slotindex 2
                    }
                    else
                    {
                        equipmentState = this.targetInventory.currentEquipmentState;
                        result.hideEntireDisplay = false;
                    }
                    Run.FixedTimeStamp now = Run.FixedTimeStamp.now;
                    Run.FixedTimeStamp chargeFinishTime = equipmentState.chargeFinishTime;
                    equipmentIndex = equipmentState.equipmentIndex;
                    result.cooldownValue = (chargeFinishTime.isInfinity ? 0 : Mathf.CeilToInt(chargeFinishTime.timeUntilClamped));
                    result.stock = (int)equipmentState.charges;
                    result.maxStock = (this.targetEquipmentSlot ? this.targetEquipmentSlot.maxStock : 1);
                }
                else if (this.displayAlternateEquipment)
                {
                    result.hideEntireDisplay = true;
                }
                result.equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
                return result;
            }

            // Token: 0x06004BC7 RID: 19399 RVA: 0x001383E4 File Offset: 0x001365E4
            private void Update()
            {
                this.SetDisplayData(this.GenerateDisplayData());
                this.equipmentReminderTimer -= Time.deltaTime;
                if (this.currentDisplayData.isReady && this.equipmentReminderTimer < 0f && this.currentDisplayData.equipmentDef != null)
                {
                    this.DoReminderFlash();
                }
            }

            private struct DisplayData
            {

                public EquipmentDef equipmentDef;

                public int cooldownValue;

                public int stock;

                public int maxStock;

                public bool hideEntireDisplay;
                public bool isReady
                {
                    get
                    {
                        return this.stock > 0;
                    }
                }

                public bool hasEquipment
                {
                    get
                    {
                        return this.equipmentDef != null;
                    }
                }

                public bool showCooldown
                {
                    get
                    {
                        return !this.isReady && this.hasEquipment;
                    }
                }
            }
        }
        #endregion
    }
}