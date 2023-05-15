using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]
    public sealed class ErraticGadget : ItemBase
    {
        private const string token = "SS2_ITEM_ERRATICGADGET_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("ErraticGadget", SS2Bundle.Items);

        [ConfigurableField(SS2Config.IDItem, ConfigDesc = "Base crit chance granted by the first stack of Erratic Gadget.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float extraCrit = 10f;

        [ConfigurableField(SS2Config.IDItem, ConfigDesc = "Extra damage that crits deal per stack of Erratic Gadget. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 1, "100")]
        public static float extraDamage = 0.5f;

        /*
         * Update: i've fixed this with this new epic interface i made based off the broken one kevin made. cool!
         */

        // R2API Moment
        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier, IOnIncomingDamageOtherServerReciever
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ErraticGadget;

            private GameObject erraticGadgetPrefab;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.critAdd += 10f;
            }


            public void OnIncomingDamageOther(HealthComponent victimHealthComponent, DamageInfo damageInfo)
            {
                if (damageInfo.crit)
                {
                    if (erraticGadgetPrefab)
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = victimHealthComponent.transform.position,
                            start = body.corePosition,
                        };
                        EffectManager.SpawnEffect(erraticGadgetPrefab, effectData, true);
                    }
                    damageInfo.damage += (damageInfo.damage * 0.5f) * stack;
                }
            }
        }
    }
}
