using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
using Moonstorm.Starstorm2;
using RoR2.Skills;
namespace EntityStates.Cyborg2
{
    public class Unmaker : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        public static float baseDuration = 0.45f;
        public static float bulletMaxDistance = 256;
        public static float bulletRadius = 1;

        public static float damageCoefficient = 2.5f;
        public static float procCoefficient = 1f;
        public static float force = 150f;
        public static GameObject tracerPrefab;
        public static GameObject hitEffectPrefab;
        public static GameObject muzzleFlashPrefab;
        public static string fireSoundString = "Play_MULT_m1_snipe_shoot"; //"Play_MULT_m1_snipe_shoot"
        public static float recoil = 1;
        public static float bloom = .4f;
        public static float fireSoundPitch = 1;

        private float duration;
        private bool hasFired;
        private string muzzleString;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = Unmaker.baseDuration / base.attackSpeedStat;
            base.StartAimMode();
            Fire();

        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            if (!this.outer.destroying && !this.hasFired)
            {
                Fire();
            }

            base.OnExit();
        }
        private void Fire()
        {

            EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, base.gameObject, muzzleString, false);
            //anim
            base.characterBody.AddSpreadBloom(bloom);
            base.AddRecoil(-1f * recoil, -1.5f * recoil, -1f * recoil, 1f * recoil);

            this.hasFired = true;

            Util.PlayAttackSpeedSound(fireSoundString, base.gameObject, fireSoundPitch);
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                new BulletAttack
                {
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    owner = base.gameObject,
                    damage = damageStat * damageCoefficient,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    force = force,
                    HitEffectNormal = false,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    maxDistance = Unmaker.bulletMaxDistance,
                    radius = Unmaker.bulletRadius,
                    isCrit = base.RollCrit(),
                    muzzleName = muzzleString,
                    minSpread = 0,
                    maxSpread = 0,
                    hitEffectPrefab = hitEffectPrefab,
                    smartCollision = true,
                    tracerEffectPrefab = tracerPrefab
                }.Fire();
            }

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public void SetStep(int i)
        {
            this.muzzleString = i == 0 ? "CannonR" : "CannonL";
        }
    }
}
