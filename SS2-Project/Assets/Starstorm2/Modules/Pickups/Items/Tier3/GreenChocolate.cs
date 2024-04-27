using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS2.Items
{
    public sealed class GreenChocolate : SS2Item
    {
        private const string token = "SS2_ITEM_GREENCHOCOLATE_DESC";
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;
        private static GameObject _effect;
        private BuffDef _buffDef;


        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Percentage of max hp that must be lost for Green Chocolate's effect to proc. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float damageThreshold = 0.2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Percent damage reduction that the damage in excess of the above threshold (base value 20%) is reduced by. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float damageReduction = 0.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Base duration of the buff provided by Green Chocolate. (1 = 1 second)")]
        [FormatToken(token, 2)]
        public static float baseDuration = 12f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Duration of the buff gained per stack. (1 = 1 second)")]
        [FormatToken(token, 3)]
        public static float stackDuration = 6f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Percent damage increase from the buff. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 4)]
        public static float buffDamage = 0.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Crit chance increase from the buff. (1 = 1% crit chance)")]
        [FormatToken(token, 5)]
        public static float buffCrit = 20f;

        public override void Initialize()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            int buffStacks = sender.GetBuffCount(_buffDef);
            args.critAdd += buffCrit * buffStacks;
            args.damageMultAdd += buffDamage * buffStacks;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "GreenChocolate" - Items
             * GameObject - "ChocolateEffect" - Items
             * BuffDef - "BuffChocolate" - Items
             */
            yield break;
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.GreenChocolate;
            public void Start()
            {
                if (body.healthComponent)
                {
                    HG.ArrayUtils.ArrayAppend(ref body.healthComponent.onIncomingDamageReceivers, this);
                }
            }


            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo.damage >= body.healthComponent.fullCombinedHealth * damageThreshold)
                {
                    damageInfo.damage = damageInfo.damage * (1 - damageReduction) + (body.healthComponent.fullCombinedHealth * (damageThreshold * damageReduction));
                    body.AddTimedBuff(SS2Content.Buffs.BuffChocolate, baseDuration + (stackDuration * (stack - 1)));


                    // NO SOUND :(((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((
                    EffectData effectData = new EffectData
                    {
                        origin = this.body.corePosition,
                        scale = this.body.radius,
                    };
                    effectData.SetNetworkedObjectReference(this.body.gameObject);
                    EffectManager.SpawnEffect(_effect, effectData, true);
                }
            }
            private void OnDestroy()
            {
                //This SHOULDNT cause any errors because nothing should be fucking with the order of things in this list... I hope.
                if (body.healthComponent)
                {
                    int i = Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this);
                    if (i > -1)
                        HG.ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onIncomingDamageReceivers, body.healthComponent.onIncomingDamageReceivers.Length, i);
                }
            }
        }
    }
}
