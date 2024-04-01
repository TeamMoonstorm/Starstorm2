using SS2.Components;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
namespace SS2.Orbs
{
    public class FieldAcceleratorOrb : Orb
    {
        public ExecutionerController execController;

        private GameObject orbEffect = SS2Assets.LoadAsset<GameObject>("FieldAcceleratorOrbEffect", SS2Bundle.Items);
        private const float speed = 100f;

        public override void Begin()
        {
            duration = distanceToTarget / speed;
            EffectData effectData = new EffectData
            {
                origin = origin,
                genericFloat = duration
            };

            EffectManager.SpawnEffect(orbEffect, effectData, true);
        }

        public override void OnArrival()
        {
            var teleInstance = TeleporterInteraction.instance;
            if (teleInstance != null)
                teleInstance.holdoutZoneController.charge += Items.FieldAccelerator.chargePerKill;
            //Util.PlaySound("AcceleratorAddCharge", teleInstance.gameObject);
        }
    }
}