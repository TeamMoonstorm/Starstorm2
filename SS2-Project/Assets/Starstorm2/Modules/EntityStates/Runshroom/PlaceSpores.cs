using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Runshroom
{
    public class PlaceSpores : BaseSkillState
    {
        public static GameObject projectilePrefab;
        public static GameObject effectPrefab;

        private static float blastDamageCoefficient = 1.5f * 0.4f;
        private static float procCoefficient = 1f;
        private static float blastRadius = 6.5f;
        private static float force = 14f;

        private static float fireTime = 0.75f;
        private static string enterSoundString = "Play_runshroom_charge"; 

        private bool hasFired;
        private bool hasWaited;
        private static float baseDuration = 1.3f;
        private static float damageCoefficient = 1.5f;
        public static GameObject chargeEffectPrefab;
        private float duration;
        private Animator animator;
        private ChildLocator childLocator;
        private GameObject chargeEffectInstance;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            childLocator = GetModelChildLocator();
            //characterBody.SetAimTimer(duration * 1.5f);
            hasFired = false;
            hasWaited = false;
            animator = GetModelAnimator();

            if (chargeEffectPrefab)
            {
                chargeEffectInstance = Object.Instantiate(chargeEffectPrefab, characterBody.corePosition, Quaternion.identity);
                
            }
            PlayCrossfade("Body", "Attack", "Primary.playbackRate", duration, 0.1f);
            Util.PlaySound(enterSoundString, gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireTime * duration && !hasFired)
            {
                hasFired = true;
                
                FireProjectile();

            }

            if (fixedAge >= duration && hasFired)
                outer.SetNextStateToMain();
        }
        public override void Update()
        {
            base.Update();
            if (chargeEffectInstance)
            {
                chargeEffectInstance.transform.position = characterBody.corePosition;
            }
        }
        public void FireProjectile()
        {
            if (chargeEffectInstance)
            {
                Destroy(chargeEffectInstance);
            }
            EffectManager.SimpleEffect(effectPrefab, characterBody.corePosition, Quaternion.identity, false);

            if (isAuthority)
            {
                bool crit = RollCrit();

                BlastAttack blastAttack = new BlastAttack();
                blastAttack.position = characterBody.corePosition;
                blastAttack.baseDamage = characterBody.damage * blastDamageCoefficient;
                blastAttack.baseForce = force;
                blastAttack.radius = blastRadius;
                blastAttack.attacker = gameObject;
                blastAttack.teamIndex = teamComponent.teamIndex;
                blastAttack.crit = crit;
                blastAttack.procCoefficient = procCoefficient;
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                blastAttack.damageType = DamageType.Generic;
                blastAttack.damageType.damageSource = DamageSource.Primary;
                blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                blastAttack.Fire();

                float damage = damageCoefficient * damageStat;
                Ray aimRay = GetAimRay();
                DamageTypeCombo damageType = DamageType.Generic;
                damageType.damageSource = DamageSource.Primary;
                ProjectileManager.instance.FireProjectile(
                    projectilePrefab,
                    characterBody.corePosition,
                    Quaternion.identity,
                    gameObject,
                    damage,
                    0f,
                    crit,
                    DamageColorIndex.Default,
                    null,
                    damageType);
            }
            
        }

        public override void OnExit()
        {
            base.OnExit();
            if (chargeEffectInstance)
            {
                Destroy(chargeEffectInstance);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}