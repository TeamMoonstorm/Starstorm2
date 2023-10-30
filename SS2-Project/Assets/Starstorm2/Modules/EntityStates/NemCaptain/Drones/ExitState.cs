using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using UnityEngine.AddressableAssets;

namespace EntityStates.NemCaptainDrone
{
    public class ExitState : BaseNemCaptainDroneState
    {
        private float duration;
        public static float baseDuration;
        private Transform modelTransform;
        private EffectData effectData;
        public override void OnEnter()
        {
            base.OnEnter();
            modelTransform = GetModelBaseTransform();
            modelTransform.gameObject.SetActive(false);
            effectData = new EffectData()
            {
                origin = modelTransform.position
            };
            EffectManager.SpawnEffect(EffectCatalog.FindEffectIndexFromPrefab(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainSuplyDropExplosion.prefab").WaitForCompletion()), effectData, true);
            duration = baseDuration;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration)
            {
                Destroy(gameObject); //bye bye!
            }
        }
    }
}
