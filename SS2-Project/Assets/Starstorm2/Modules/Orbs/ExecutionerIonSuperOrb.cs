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
            ParallelMultiStartAssetLoadCoroutine helper = new ParallelMultiStartAssetLoadCoroutine();

            helper.AddAssetToLoad<GameObject>("ExecutionerIonSuperOrbEffect", SS2Bundle.Executioner2);
            helper.AddAssetToLoad<NetworkSoundEventDef>("ExecutionerGainCharge", SS2Bundle.Executioner2);

            helper.Start();
            while (!helper.IsDone)
                yield return null;

            orbEffect = helper.GetLoadedAsset<GameObject>("ExecutionerIonSuperOrbEffect");
            sound = helper.GetLoadedAsset<NetworkSoundEventDef>("ExecutionerGainCharge");
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