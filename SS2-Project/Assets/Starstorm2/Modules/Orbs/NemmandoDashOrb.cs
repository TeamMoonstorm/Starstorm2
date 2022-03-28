using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace Moonstorm.Starstorm2.Orbs
{
    public class NemmandoDashOrb : Orb
    {
        private const float speed = 80f;

        public override void Begin()
        {
            duration = distanceToTarget / speed;

            EffectData effectData = new EffectData
            {
                origin = origin,
                genericFloat = duration
            };

            effectData.SetHurtBoxReference(target);

            //EffectManager.SpawnEffect(Assets.nemDashEffect, effectData, true);
        }

        public override void OnArrival()
        {
            EffectData effectData = new EffectData
            {
                origin = origin,
                genericFloat = duration
            };

            effectData.SetHurtBoxReference(target);

            //EffectManager.SpawnEffect(Assets.nemPreImpactFX, effectData, true);

            HurtBox hurtBox = target.GetComponent<HurtBox>();
            if (hurtBox)
            {
                GameObject bodyObject = hurtBox.healthComponent.gameObject;
                if (bodyObject)
                {
                    SetStateOnHurt setState = bodyObject.GetComponent<SetStateOnHurt>();
                    if (setState)
                    {
                        setState.CallRpcSetStun(5f);
                    }
                }
            }
        }
    }
}