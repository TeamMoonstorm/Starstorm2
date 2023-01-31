using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Orbs
{
    public class ExecutionerIonSuperOrb : Orb
    {
        public float buffDuration = -1f;
        private NetworkSoundEventDef sound = SS2Assets.LoadAsset<NetworkSoundEventDef>("SoundEventExecutionerGainCharge", SS2Bundle.Executioner);
        private GameObject orbEffect = SS2Assets.LoadAsset<GameObject>("ExecutionerIonSuperOrbEffect", SS2Bundle.Executioner);
        private HurtBox hurtBox;
        private const float speed = 50f;

        public override void Begin()
        {
            duration = distanceToTarget / speed;
            EffectData effectData = new EffectData
            {
                origin = origin,
                genericFloat = duration
            };
            effectData.SetHurtBoxReference(target);
            EffectManager.SpawnEffect(orbEffect, effectData, true);
            hurtBox = target.GetComponent<HurtBox>();
        }

        public override void OnArrival()
        {
            if (NetworkServer.active && hurtBox)
            {
                var body = hurtBox.healthComponent.body;
                if (buffDuration == -1f)
                    body.AddBuff(SS2Content.Buffs.BuffExecutionerSuperCharged);
                else
                    body.AddTimedBuff(SS2Content.Buffs.BuffExecutionerSuperCharged, buffDuration);
                EffectManager.SimpleSoundEffect(sound.index, body.corePosition, true);
            }
        }
    }
}