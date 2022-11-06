using Moonstorm;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Nemmando
{
    public class FireSwordBeam : BaseSkillState
    {
        public float charge;

        public static float maxEmission;
        public static float minEmission;
        [TokenModifier("SS2_NEMMANDO_SECONDARY_CONCUSSION_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float maxDamageCoefficient;
        public static float minDamageCoeffficient;
        public static float procCoefficient;
        public static float maxRecoil;
        public static float minRecoil;
        public static float maxProjectileSpeed;
        public static float minProjectileSpeed;
        public static float baseDuration;
        public static GameObject projectilePrefab;

        private float emission;
        private Material swordMat;
        private float damageCoefficient;
        private float recoil;
        private float projectileSpeed;
        private float duration;
        private float fireDuration;
        private bool hasFired;
        private Animator animator;
        private string muzzleString;
        //private NemmandoController nemmandoController;
        //private float minimumEmission;
        //private float maximumEmission;

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.SetAimTimer(2f);
            animator = GetModelAnimator();
            muzzleString = "Muzzle";
            hasFired = false;
            duration = baseDuration / attackSpeedStat;
            damageCoefficient = Util.Remap(charge, 0f, 1f, minDamageCoeffficient, maxDamageCoefficient);
            recoil = Util.Remap(charge, 0f, 1f, minRecoil, maxRecoil);
            projectileSpeed = Util.Remap(charge, 0f, 1f, minProjectileSpeed, maxProjectileSpeed);
            emission = Util.Remap(charge, 0f, 1f, minEmission, maxEmission);
            fireDuration = 0.1f * duration;
            //nemmandoController = GetComponent<NemmandoController>();

            //minimumEmission = effectComponent.defaultSwordEmission;
            //maximumEmission = minimumEmission + 150f;

            string fireAnim = charge > 0.6f ? "Secondary3(Strong)" : "Secondary3(Weak)";

            bool moving = animator.GetBool("isMoving");
            bool grounded = animator.GetBool("isGrounded");

            if (!moving && grounded/* && !nemmandoController.chargingDecisiveStrike && !nemmandoController.rolling*/)
            {
                PlayCrossfade("FullBody, Override", fireAnim, "Secondary.playbackRate", duration, 0.05f);
            }

            PlayCrossfade("Gesture, Override", fireAnim, "Secondary.playbackRate", duration, 0.05f);

            swordMat = GetModelTransform().GetComponent<CharacterModel>().baseRendererInfos[1].defaultMaterial;

            Util.PlaySound("NemmandoFireBeam2", gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();

        }

        public virtual void FireBeam()
        {
            if (!hasFired)
            {
                hasFired = true;

                //bool isCrit = RollCrit();

                //Util.PlayAttackSpeedSound("NemmandoSwing1", base.gameObject, attackSpeedStat);

                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FireBarrage.effectPrefab, gameObject, muzzleString, false);

                if (isAuthority)
                {
                    float damage = damageCoefficient * damageStat;
                    AddRecoil(-2f * recoil, -3f * recoil, -1f * recoil, 1f * recoil);
                    characterBody.AddSpreadBloom(0.33f * recoil);
                    Ray aimRay = GetAimRay();

                    ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), gameObject, damage, 0f, RollCrit(), DamageColorIndex.Default, null, projectileSpeed);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireDuration)
            {
                FireBeam();
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}