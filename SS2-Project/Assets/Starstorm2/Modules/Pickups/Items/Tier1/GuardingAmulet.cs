using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;
using System.Collections.Generic;
using System;
using EntityStates;

using Moonstorm;
namespace SS2.Items
{
    public sealed class GuardingAmulet : ItemBase
    {
        public const string token = "SS2_ITEM_GUARDINGAMULET_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("GuardingAmulet", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Damage reduction per stack. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float damageReduction = 0.3f;

        public static GameObject shieldEffect = SS2Assets.LoadAsset<GameObject>("AmuletShieldEffect", SS2Bundle.Items);

        public sealed class Behavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.GuardingAmulet;

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (!damageInfo.attacker) return;
                
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                Vector3 attackerPosition = attackerBody ? attackerBody.corePosition : damageInfo.attacker.transform.position;
                Vector3 between = damageInfo.position - attackerPosition;
                if (BackstabManager.IsBackstab(between, this.body))
                {
                    float reduction = Util.ConvertAmplificationPercentageIntoReductionPercentage(damageReduction * this.stack * 100f) / 100f;
                    damageInfo.damage *= 1 - reduction;
                    
                    EffectData effectData = new EffectData
                    {
                        origin = this.body.corePosition,
                        rotation = Util.QuaternionSafeLookRotation(attackerPosition - damageInfo.position),
                        scale = this.body.radius,
                    };
                    effectData.SetNetworkedObjectReference(this.body.gameObject);
                    EffectManager.SpawnEffect(shieldEffect, effectData, true);
                }

            }
        }
    }
}
