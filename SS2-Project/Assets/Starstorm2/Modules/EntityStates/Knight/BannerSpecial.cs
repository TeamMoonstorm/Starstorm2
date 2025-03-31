using UnityEngine;
using RoR2;
using RoR2.Skills;
using UnityEngine.Networking;
using System;

namespace EntityStates.Knight
{
    public class BannerSpecial : BaseState
    {
        public static GameObject knightBannerWard;
        public static GameObject slowBuffWard;

        private GameObject bannerObject;
        private GameObject slowBuffWardInstance;

        [SerializeField]
        public float impactRadius = 5f;
        [SerializeField]
        public float impactDamage = 3f;

        public float minimumY = 0.05f;
        public float airControl = 0.15f;
        public float aimVelocity = 4f;
        public float upwardVelocity = 7f;
        public float forwardVelocity = 3f;

        private bool detonateNextFrame;
        private float previousAirControl;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("FullBody, Override", "SpecialLeapStart");

            base.OnEnter();
            previousAirControl = base.characterMotor.airControl;
            base.characterMotor.airControl = airControl;
            Vector3 direction = GetAimRay().direction;
            if (base.isAuthority)
            {
                base.characterBody.isSprinting = true;
                direction.y = Mathf.Max(direction.y, minimumY);
                Vector3 vector = direction.normalized * aimVelocity * moveSpeedStat;
                Vector3 vector2 = Vector3.up * upwardVelocity;
                Vector3 vector3 = new Vector3(direction.x, 0f, direction.z).normalized * forwardVelocity;
                base.characterMotor.Motor.ForceUnground();
                base.characterMotor.velocity = vector + vector2 + vector3;
            }
            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            GetModelTransform().GetComponent<AimAnimator>().enabled = true;
            base.characterDirection.moveVector = direction;

            if (base.isAuthority)
            {
                base.characterMotor.onMovementHit += OnMovementHit;
            }
        }

        private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            detonateNextFrame = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority && base.characterMotor)
            {
                base.characterMotor.moveDirection = base.inputBank.moveVector;
                if (detonateNextFrame || (base.characterMotor.Motor.GroundingStatus.IsStableOnGround && !base.characterMotor.Motor.LastGroundingStatus.IsStableOnGround))
                {
                    outer.SetNextStateToMain();
                }
            }
        }

        public override void OnExit()
        {
            if (base.isAuthority)
            {
                base.characterMotor.onMovementHit -= OnMovementHit;
            }

            FireImpact();

            base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            base.characterMotor.airControl = previousAirControl;
            base.characterBody.isSprinting = false;

            characterMotor.velocity *= 0.1f;

            base.OnExit();
        }

        protected virtual void FireImpact()
        {
            PlayAnimation("FullBody, Override", "SpecialLeapEnd", "Special.playbackRate", detonateNextFrame ? 1f : 0.2f);

            if (base.isAuthority)
            {
                var blastAttack = new BlastAttack
                {
                    attacker = base.gameObject,
                    baseDamage = damageStat * impactDamage,
                    baseForce = 0f,
                    bonusForce = Vector3.up,
                    crit = false,
                    damageType = DamageTypeCombo.GenericSpecial,
                    falloffModel = BlastAttack.FalloffModel.None,
                    procCoefficient = 1f,
                    radius = impactRadius,
                    position = base.characterBody.footPosition,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    //impactEffect = EffectCatalog.FindEffectIndexFromPrefab(SS2.Survivors.Knight.KnightImpactEffect),
                    teamIndex = base.teamComponent.teamIndex,
                };

                ModifyBlastAttack(blastAttack);

                blastAttack.Fire();
            }

            if (NetworkServer.active)
            {
                Vector3 position = inputBank.aimOrigin - (inputBank.aimDirection);
                bannerObject = UnityEngine.Object.Instantiate(knightBannerWard, position, Quaternion.identity);

                bannerObject.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                NetworkServer.Spawn(bannerObject);

                slowBuffWardInstance = UnityEngine.Object.Instantiate(slowBuffWard, position, Quaternion.identity);
                slowBuffWardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                slowBuffWardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(bannerObject);
            }
        }

        protected virtual void ModifyBlastAttack(BlastAttack blastAttack) { }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }

}