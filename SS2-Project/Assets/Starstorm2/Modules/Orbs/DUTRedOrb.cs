
using SS2.Components;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using System.Collections;
using MSU;

namespace SS2.Orbs
{
    public class DUTRedOrb : Orb
    {
        public DUTController dutController;

        private const float speed = 65f;
        private static GameObject _orbEffect = SS2Assets.LoadAsset<GameObject>("DUTOrbEffectRed", SS2Bundle.Indev);

        [AsyncAssetLoad]
        private static IEnumerator LoadAssets()
        {
            var request = SS2Assets.LoadAssetAsync<GameObject>("DUTOrbEffectRed", SS2Bundle.Indev);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _orbEffect = request.Asset;
        }

        public override void Begin()
        {
            //base.Begin();
            duration = distanceToTarget / speed;
            if (duration > 3f)
                duration = 3f;

            EffectData effectData = new EffectData
            {
                origin = origin,
                genericFloat = duration
            };
            effectData.SetHurtBoxReference(target);

            if (_orbEffect) EffectManager.SpawnEffect(_orbEffect, effectData, true);

            HurtBox hurtBox = target.GetComponent<HurtBox>();
            if (hurtBox)
                dutController = hurtBox.healthComponent.GetComponent<DUTController>();
        }

        public override void OnArrival()
        {
            if (dutController)
            {
                dutController.AddCharge(1f);
            }
        }
    }
}