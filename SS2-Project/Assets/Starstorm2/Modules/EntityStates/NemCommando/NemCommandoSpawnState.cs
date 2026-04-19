using EntityStates.Generic;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace EntityStates.NemCommando
{
    public class NemCommandoSpawnState : NemesisSpawnState
    {
        public static string portalMuz;
        public static SkillDef gunSecondary;
        public static SkillDef gunSpecial;
        private bool hasGun = true;
        public static float throwGunDelay;
        private float throwGunTime;
        private bool thrownGun = false;

        public override void OnEnter()
        {
            base.OnEnter();

            throwGunTime = throwGunDelay * duration;

            if (skillLocator.secondary.skillDef != gunSecondary && skillLocator.special.skillDef != gunSpecial)
            {
                hasGun = false;
                GetModelAnimator().SetBool("gunEquipped", hasGun);
                PlayAnimation("FullBody, Override", "SpawnNoGun");
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!hasGun && !thrownGun && fixedAge >= throwGunTime)
            {
                FindModelChild("gunParticle").GetComponent<ParticleSystem>().Emit(1);
                FindModelChild("GunModel").gameObject.SetActive(false);
                thrownGun = true;
            }
        }

        public override void SpawnEffect()
        {
            portalMuzzle = portalMuz;
            base.SpawnEffect();
        }
    }
}