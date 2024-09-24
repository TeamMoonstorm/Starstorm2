using SS2.Components;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using System.Collections;
using MSU;

namespace SS2.Orbs
{
    public class ExecutionerIonOrb : Orb
    {
        public ExecutionerController execController;

        //private NetworkSoundEventDef sound = SS2Assets.LoadAsset<NetworkSoundEventDef>("SoundEventExecutionerGainCharge", SS2Bundle.Executioner);
        private const float speed = 50f;
        private string skinNameToken;

        private static GameObject _orbEffect;
        private static GameObject _masteryOrbEffect;

        [AsyncAssetLoad]
        private static IEnumerator LoadAssets()
        {
            var helper = new ParallelMultiStartAssetLoadCoroutine();
            helper.AddAssetToLoad<GameObject>("ExecutionerIonOrbEffect", SS2Bundle.Executioner2);
            helper.AddAssetToLoad<GameObject>("ExecutionerIonOrbEffectMastery", SS2Bundle.Executioner2);

            helper.Start();
            while (!helper.IsDone)
                yield return null;

            _orbEffect = helper.GetLoadedAsset<GameObject>("ExecutionerIonOrbEffect");
            _masteryOrbEffect = helper.GetLoadedAsset<GameObject>("ExecutionerIonOrbEffectMastery");
            yield break;
        }

        public override void Begin()
        {
            duration = distanceToTarget / speed;
            if (duration > 4f)
                duration = 4f;
            EffectData effectData = new EffectData
            {
                origin = origin,
                genericFloat = duration
            };
            effectData.SetHurtBoxReference(target);

            

            HurtBox hurtBox = target.GetComponent<HurtBox>();
            if (hurtBox)
            {
                execController = hurtBox.healthComponent.GetComponent<ExecutionerController>();
                skinNameToken = hurtBox.gameObject.transform.root.GetComponentInChildren<ModelSkinController>().skins[hurtBox.healthComponent.body.skinIndex].nameToken;


                if (skinNameToken == "SS2_SKIN_EXECUTIONER2_MASTERY")
                {
                    EffectManager.SpawnEffect(_masteryOrbEffect, effectData, true);
                }
                else
                {
                    EffectManager.SpawnEffect(_orbEffect, effectData, true);
                }
            }
        }

        public override void OnArrival()
        {
            if (execController)
            {
                execController.RpcAddIonCharge();
                //if (sound)
                //    EffectManager.SimpleSoundEffect(sound.index, execController.transform.position, true);
            }
        }
    }
}