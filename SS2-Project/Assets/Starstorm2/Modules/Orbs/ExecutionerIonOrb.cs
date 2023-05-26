using Moonstorm.Starstorm2.Components;
using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace Moonstorm.Starstorm2.Orbs
{
    public class ExecutionerIonOrb : Orb
    {
        public ExecutionerController execController;

        //private NetworkSoundEventDef sound = SS2Assets.LoadAsset<NetworkSoundEventDef>("SoundEventExecutionerGainCharge", SS2Bundle.Executioner);
        private GameObject orbEffect = SS2Assets.LoadAsset<GameObject>("ExecutionerIonOrbEffect", SS2Bundle.Executioner);
        private GameObject orbEffectMastery = SS2Assets.LoadAsset<GameObject>("ExecutionerIonOrbEffectMastery", SS2Bundle.Executioner);// referenced just for u <3 -b
        private const float speed = 50f;
        private string skinNameToken;

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

            HurtBox hurtBox = target.GetComponent<HurtBox>();
            if (hurtBox)
                execController = hurtBox.healthComponent.GetComponent<ExecutionerController>();
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