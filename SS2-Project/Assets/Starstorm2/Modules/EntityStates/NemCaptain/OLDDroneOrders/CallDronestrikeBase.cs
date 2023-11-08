using Moonstorm.Starstorm2.Components;
using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.NemCaptain.Weapon
{
    public class CallDronestrikeBase : AimThrowableBase
    {
        [SerializeField]
        public float airstrikeRadius;

        [SerializeField]
        public float bloom;
        public static GameObject muzzleflashEffect;
        public static string muzzleString;
        public static string fireDronestrikeSoundString;

        private NemCaptainController ncc;

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.SetSpreadBloom(bloom, true);

            ncc = characterBody.GetComponent<NemCaptainController>();
            
            EntityStateMachine esm = EntityStateMachine.FindByCustomName(gameObject, "Skillswap");
            if (esm)
                esm.SetNextStateToMain();

            //skillLocator.primary.SetSkillOverride(gameObject, activatorSkillSlot.skillDef, GenericSkill.SkillOverridePriority.Replacement);

            activatorSkillSlot.UnsetSkillOverride(gameObject, activatorSkillSlot.skillDef, GenericSkill.SkillOverridePriority.Replacement);
            activatorSkillSlot.SetSkillOverride(gameObject, ncc.nullSkill, GenericSkill.SkillOverridePriority.Loadout);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.SetAimTimer(4f);
        }

        public override void OnExit()
        {
            Util.PlaySound(fireDronestrikeSoundString, gameObject);
            //skillLocator.primary.UnsetSkillOverride(gameObject, activatorSkillSlot.skillDef, GenericSkill.SkillOverridePriority.Replacement);
            base.OnExit();
        }

        public override void ModifyProjectile(ref FireProjectileInfo fireProjectileInfo)
        {
            base.ModifyProjectile(ref fireProjectileInfo);
            //characterBody.transform;
            fireProjectileInfo.position = currentTrajectoryInfo.hitPoint;
            fireProjectileInfo.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360f), 0f);
            fireProjectileInfo.speedOverride = 0f;
        }

        /*public override bool KeyIsDown()
        {
            return inputBank.skill1.down;
        }

        public override EntityState PickNextState()
        {
            return new Idle();
        }*/

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
