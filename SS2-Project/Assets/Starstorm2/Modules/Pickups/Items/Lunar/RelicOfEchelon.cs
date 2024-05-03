using R2API;
using RoR2;
using RoR2.Items;
using System;
using UnityEngine;
using System.Collections.Generic;
using RoR2.UI;
using System.Collections;

using MSU;
using MSU.Config;
using RoR2.ContentManagement;

//Had disabled content, to the nether realm with you. -N

#if DEBUG
namespace SS2.Items
{
    public sealed class RelicOfEchelon : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_RELICOFECHELON_DESC";
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        private BuffDef _buffEchelon ;// { get; } = SS2Assets.LoadAsset<BuffDef>("BuffEchelon", SS2Bundle.Indev);

        public static Material _overlay;// => SS2Assets.LoadAsset<Material>("matTerminationOverlay");
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => throw new NotImplementedException();

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Equipment cooldown increase per use, per stack.")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float cooldownIncrease = .15f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Base amount of additional base damage added.")]
        [FormatToken(token,   1)]
        public static float damageBonus = 150;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Base amount of additional base health added.")]
        [FormatToken(token,   2)]
        public static float healthBonus = 5000;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Duration of the Echelon buff. Does not scale with stacks. (1 = 1 second)")]
        [FormatToken(token,   3)]
        public static float buffDuration = 8;

        public static Color echelonColor;

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false; //disabled for now
        }

        public override IEnumerator LoadContentAsync()
        {
            //ItemDef - "RelicOfEchelon" - Items
            //BuffDef - "BuffEchelon" - Indev
            yield break;
        }
        public override void Initialize()
        {
            BuffOverlays.AddBuffOverlay(_buffEchelon, _overlay);

            On.RoR2.Inventory.CalculateEquipmentCooldownScale += Inventory_CalculateEquipmentCooldownScale;

            echelonColor = new Color(0.4235f, 0.6706f, 0.6588f);
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.buffDefs.AddSingle(_buffEchelon);
        }

        private float Inventory_CalculateEquipmentCooldownScale(On.RoR2.Inventory.orig_CalculateEquipmentCooldownScale orig, Inventory self)
        {
            float num = orig.Invoke(self);
            EquipmentIndex eqp = self.currentEquipmentIndex;
            var token = self.gameObject.GetComponent<EchelonToken>();
            var def = EquipmentCatalog.GetEquipmentDef(eqp);

            int useCount;
            int itemCount = self.GetItemCount(SS2Content.Items.RelicOfEchelon);
            if (itemCount > 0)
            {
                if (token)
                {
                    if(!token.usedEquipment.ContainsKey(eqp))
                    {
                        token.usedEquipment.Add(eqp, itemCount);
                    }
                }
                else
                {
                    token = self.gameObject.AddComponent<EchelonToken>();
                    token.usedEquipment = new Dictionary<EquipmentIndex, int>();
                    token.usedEquipment.Add(eqp, itemCount);
                    token.body = null;

                }

                useCount = token.usedEquipment[eqp];
                float mod = Mathf.Pow(1 + cooldownIncrease, useCount);
                float cooldownAdjustment = Math.Abs(((.7f + def.cooldown) / def.cooldown) - 1);
                if (def.cooldown < 5)
                {
                    cooldownAdjustment -= Mathf.Max(0, 6 - useCount);
                }
                float adjusted = def.cooldown + cooldownAdjustment;
                float adj2 = adjusted * mod;
                float mod2 = adj2 / def.cooldown;
                num *= mod2;
            }
            return num;
        }

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfEchelon;

            private void OnEnable()
            {
                EquipmentSlot.onServerEquipmentActivated += GrantEchelonBuff;
                On.RoR2.UI.HealthBar.UpdateBarInfos += Echelon2;
            }

            private void OnDisable()
            {
                EquipmentSlot.onServerEquipmentActivated -= GrantEchelonBuff;
                On.RoR2.UI.HealthBar.UpdateBarInfos -= Echelon2;
            }

            private void GrantEchelonBuff(EquipmentSlot arg1, EquipmentIndex arg2)
            {
                var token = arg1.characterBody.inventory.gameObject.GetComponent<EchelonToken>();
                var inv = arg1.characterBody.inventory;
                if (inv)
                {
                    int count = inv.GetItemCount(SS2Content.Items.RelicOfEchelon);
                    if (count > 0)
                    {
                        if (!token)
                        {
                            SS2Log.Debug("Token added in OnEquipmentActivated - should probably never happen, but may be fine");
                            token = arg1.characterBody.inventory.gameObject.AddComponent<EchelonToken>();
                            token.usedEquipment = new Dictionary<EquipmentIndex, int>();
                        }
                        if(token.body == null)
                        {
                            token.body = arg1.characterBody;
                        }
                        if (token.usedEquipment.ContainsKey(arg2))
                        {
                            token.usedEquipment[arg2] += count;

                        }
                        else
                        {
                            token.usedEquipment.Add(arg2, count);

                        }
                        arg1.characterBody.AddTimedBuffAuthority(SS2Content.Buffs.BuffEchelon.buffIndex, buffDuration);
                        StartCoroutine(token.callRecalcOnBuffEnd());
                    }
                }
            }

            private void Echelon2(On.RoR2.UI.HealthBar.orig_UpdateBarInfos orig, HealthBar self)
            {
                orig(self);
                var healthComponent = self._source;
                if (healthComponent)
                {
                    if (healthComponent.body.HasBuff(SS2Content.Buffs.BuffEchelon))
                    {
                        self.barInfoCollection.trailingOverHealthbarInfo.color = echelonColor;
                    }
                }
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (body.HasBuff(SS2Content.Buffs.BuffEchelon))
                {
                    args.baseDamageAdd += (damageBonus * (stack - 1));
                    args.baseHealthAdd += (healthBonus * (stack - 1));
                }
            }
        }

        public class EchelonToken : MonoBehaviour
        {
            public IDictionary<EquipmentIndex, int> usedEquipment;
            public CharacterBody body;
            public bool hadBuff;

            public IEnumerator callRecalcOnBuffEnd()
            {
                yield return new WaitForSeconds(buffDuration + .01f);
                body.RecalculateStats();
            }
        }

        public sealed class EchelonBuffBehavior : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffEchelon;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseDamageAdd += RelicOfEchelon.damageBonus;
                args.baseHealthAdd += RelicOfEchelon.healthBonus;
            }
        }

    }
}
#endif