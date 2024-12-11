using RoR2;
using UnityEngine;
using RoR2.Skills;
namespace EntityStates.Cyborg2
{
    public class Unmaker : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        private static float baseDuration = 0.625f;
        private static float bulletMaxDistance = 256;
        private static float bulletRadius = 1;

        private static float damageCoefficient = 2f;
        private static float procCoefficient = 1f;
        private static float force = 150f;
        public static GameObject tracerPrefab;
        public static GameObject hitEffectPrefab;
        public static GameObject muzzleFlashPrefab;
        private static string fireSoundString = "Play_MULT_m1_snipe_shoot"; //"Play_MULT_m1_snipe_shoot"
        private static float recoil = 2;
        private static float bloom = 2f;
        private static float fireSoundPitch = 1;

        private float duration;
        private bool hasFired;
        private string muzzleString;
        private int step;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = Unmaker.baseDuration / base.attackSpeedStat;
            base.StartAimMode();
            this.muzzleString = step == 0 ? "CannonR" : "CannonL";
            Fire();
            base.PlayAnimation("Gesture, Override", step == 0 ? "Primary1" : "Primary2");//, "Primary.playbackRate", this.duration);
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
                DamageTypeCombo damageType = DamageType.SlowOnHit;
                damageType.damageSource = DamageSource.Primary;
                new BulletAttack
                {
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    owner = base.gameObject,
                    damage = damageStat * damageCoefficient,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = damageType,
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
            step = i;
        }
    }
}
