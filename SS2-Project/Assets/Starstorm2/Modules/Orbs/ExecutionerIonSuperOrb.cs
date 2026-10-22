using MSU;
using RoR2;
using RoR2.Orbs;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Orbs
{
    public class ExecutionerIonSuperOrb : Orb
    {
        public float buffDuration = -1f;
        private static NetworkSoundEventDef sound;
        private static GameObject orbEffect;
        private HurtBox hurtBox;
        private const float speed = 50f;

        [AsyncAssetLoad]
        private static IEnumerator LoadAssetsAsync()
        {
            ParallelCoroutine helper = ParallelCoroutine.RequestFromPool();

            using var executionerIonSuperOrbEffect = SS2Assets.LoadAssetAsync<GameObject>("ExecutionerIonSuperOrbEffect", SS2Bundle.Executioner2);
            using var executionerGainCharge = SS2Assets.LoadAssetAsync<NetworkSoundEventDef>("ExecutionerGainCharge", SS2Bundle.Executioner2);

            helper.Add(executionerIonSuperOrbEffect);
            helper.Add(executionerGainCharge);

            while (!helper.isDone)
                yield return null;

            orbEffect = executionerIonSuperOrbEffect.Asset;
            sound = executionerGainCharge.Asset;

            ParallelCoroutine.ReturnToPool(helper);
        }
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