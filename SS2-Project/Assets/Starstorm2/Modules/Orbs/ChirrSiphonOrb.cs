using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace Moonstorm.Starstorm2.Orbs
{
    public class ChirrSiphonOrb : Orb
    {
        public float siphonPercent;
        public float overrideDuration = 0.2f;
        public GameObject orbEffect;
        public CharacterBody siphonBody;
        public CharacterBody playerBody;

        public override void Begin()
        {
            base.duration = overrideDuration;
            EffectData effectData = new EffectData
            {
                scale = 1f,
                origin = siphonBody.corePosition,
                genericFloat = duration
            };
            effectData.SetHurtBoxReference(playerBody.mainHurtBox);
            if (orbEffect) EffectManager.SpawnEffect(orbEffect, effectData, true);

            if (siphonBody && playerBody)
            {
                HealthComponent hc = siphonBody.healthComponent;
                if (hc)
                {
                    DamageInfo damageInfo = new DamageInfo();
                    damageInfo.damage = hc.fullHealth * siphonPercent;
                    damageInfo.position = siphonBody.corePosition;
                    damageInfo.procCoefficient = 0f;
                    damageInfo.damageType = DamageType.BypassArmor;
                    damageInfo.damageColorIndex = DamageColorIndex.Poison;
                    hc.TakeDamage(damageInfo);
                }
            }
        }

        public override void OnArrival()
        {
            if (playerBody)
            {
                HealthComponent hc = playerBody.healthComponent;
                if (hc)
                {
                    hc.HealFraction(siphonPercent, default(ProcChainMask));
                }
            }
        }
    }
}
