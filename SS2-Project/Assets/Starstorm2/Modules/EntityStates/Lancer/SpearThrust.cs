using EntityStates.Knight;
using R2API;
using RoR2;
using UnityEngine;

namespace EntityStates.Lancer
{
    public class SpearThrust : BaseKnightMeleeAttack
    {
        public static float tipperDamageCoefficient = 2f;
        public static string thrustAnimationState = "SpearThrust";

        // Static VFX fields assigned from Lancer.cs Initialize()
        // BaseKnightMeleeAttack's swingEffectPrefab/hitEffectPrefab are [SerializeField] instance fields,
        // so we bridge them via static fields set in OnEnter before base.OnEnter().
        public static GameObject spearSwingEffectPrefab;
        public static GameObject spearHitEffectPrefab;

        private OverlapAttack tipperAttack;

        public override void OnEnter()
        {
            hitboxGroupName = "SpearHitbox";
            muzzleString = "SpearMuzzle";
            swingEffectPrefab = spearSwingEffectPrefab;
            hitEffectPrefab = spearHitEffectPrefab;

            base.OnEnter();

            this.attack.damageType.damageSource = DamageSource.Primary;

            HitBoxGroup tipperHitboxGroup = FindHitBoxGroup("TipperHitbox");
            if (tipperHitboxGroup)
            {
                tipperAttack = new OverlapAttack();
                tipperAttack.damageType = attack.damageType;
                tipperAttack.attacker = gameObject;
                tipperAttack.inflictor = gameObject;
                tipperAttack.teamIndex = GetTeam();
                tipperAttack.damage = tipperDamageCoefficient * damageStat;
                tipperAttack.procCoefficient = procCoefficient;
                tipperAttack.hitEffectPrefab = hitEffectPrefab;
                tipperAttack.forceVector = bonusForce;
                tipperAttack.pushAwayForce = pushForce;
                tipperAttack.hitBoxGroup = tipperHitboxGroup;
                tipperAttack.isCrit = attack.isCrit;
                tipperAttack.maximumOverlapTargets = 1000;
                tipperAttack.AddModdedDamageType(SS2.Survivors.Lancer.TipperIonFieldDamageType);
                if (impactSound != null)
                {
                    tipperAttack.impactSound = impactSound.index;
                }
            }
            else
            {
                Debug.LogError("SpearThrust: TipperHitbox HitBoxGroup not found on model.");
            }
        }

        public override void PlayAttackAnimation()
        {
            if (isGrounded && !animator.GetBool("isMoving"))
            {
                PlayCrossfade("FullBody, Override", thrustAnimationState, playbackRateParam, duration, 0.08f);
            }
            PlayCrossfade("Gesture, Override", thrustAnimationState, playbackRateParam, duration, 0.08f);
        }

        protected override void FireAttack()
        {
            base.FireAttack();

            if (isAuthority && tipperAttack != null)
            {
                tipperAttack.Fire();
            }
        }
    }
}
