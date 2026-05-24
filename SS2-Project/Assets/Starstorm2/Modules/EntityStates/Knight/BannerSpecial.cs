using UnityEngine;
using RoR2;
using RoR2.Skills;
using UnityEngine.Networking;
using System;

namespace EntityStates.Knight
{
    public class EnterBannerSpecial : BaseState
    {
        private static float duration = 0.125f;
        private static float velocityDamping = 0.33f;
        private static InterruptPriority minimumInterruptPriority = InterruptPriority.PrioritySkill;
        private static string enterSoundString = "Play_item_proc_warbanner";
        public override void OnEnter()
        {
            base.OnEnter();

            if (isAuthority)
            {
                characterMotor.velocity = Vector3.zero;
            }

            PlayAnimation("FullBody, Override", "SpecialLeapStart");

            Util.PlaySound(enterSoundString, gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
            {
                characterMotor.velocity *= velocityDamping;
            }
            if (fixedAge >= duration && isAuthority)
            {
                SetNextState();
                return;
            }
        }
        public virtual void SetNextState()
        {
            outer.SetNextState(new BannerSpecial());
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return minimumInterruptPriority;
        }
    }
    public class BannerSpecial : BaseState
    {
        public static GameObject knightBannerWard;
        public static GameObject slowBuffWard;
        
        internal static float impactRadius = 4f;
        private static float impactDamage = 7.5f;

        private static float minimumY = 0.05f;
        private static float airControl = 0.15f;
        private static float aimVelocity = 3f;
        private static float upwardVelocity = 21f;
        private static float moveVelocity = 3f;
        private static float forwardVelocity = 3f;
        private static float gravityCoefficient = 1.8f;
        private static Vector3 bannerOffset = new Vector3(0.15f, 0f, 1.4f);
        private static float gracePeriod = 0.2f;

        private static string collisionTransformString = "BannerSlamHitbox";

        private bool detonateNextFrame;
        private float previousAirControl;
        private Vector3 initialDirection;
        private Transform collisionTransform;
        public override void OnEnter()
        {
            base.OnEnter();


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
                Vector3 vector4 = inputBank.moveVector * moveVelocity;
                base.characterMotor.Motor.ForceUnground(0.2f);
                base.characterMotor.velocity = vector + vector2 + vector3 + vector4;
                initialDirection = direction;
            }
            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            base.characterDirection.moveVector = direction;

            if (base.isAuthority)
            {
                collisionTransform = FindModelChild(collisionTransformString);
                base.characterMotor.onMovementHit += OnMovementHit;
            }
        }

        private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            detonateNextFrame = true;
        }

        private void CheckCollisions()
        {
            if (collisionTransform)
            {
                Collider[] hits = Physics.OverlapBox(collisionTransform.position, collisionTransform.lossyScale * 0.5f, collisionTransform.rotation, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i])
                    {
                        HurtBox hurtBox = hits[i].GetComponent<HurtBox>();

                        if (hurtBox)
                        {
                            if (hurtBox.healthComponent.gameObject != gameObject)
                            {
                                detonateNextFrame = true;
                                break;
                            }
                        }
                        else
                        {
                            // we hit da world
                            detonateNextFrame = true;
                            break;
                        }
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority && base.characterMotor)
            {
                base.characterMotor.moveDirection = base.inputBank.moveVector;
                characterDirection.moveVector = initialDirection;

                characterMotor.velocity += Physics.gravity * gravityCoefficient * Time.fixedDeltaTime;

                if (fixedAge > gracePeriod)
                {
                    CheckCollisions();
                }

                if (detonateNextFrame || (base.characterMotor.Motor.GroundingStatus.IsStableOnGround && !base.characterMotor.Motor.LastGroundingStatus.IsStableOnGround))
                {
                    outer.SetNextState(new BannerSpecialWindDown());
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

            // TODO: Bro needs sound
            //Util.PlaySound("Play_huntress_R_snipe_shoot", gameObject);

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
                Vector3 position = inputBank.aimOrigin + Util.QuaternionSafeLookRotation(characterDirection.forward) * bannerOffset;
                var bannerObject = UnityEngine.Object.Instantiate(knightBannerWard, position, Util.QuaternionSafeLookRotation(-characterDirection.forward));

                bannerObject.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                NetworkServer.Spawn(bannerObject);
            }
        }

        protected virtual void ModifyBlastAttack(BlastAttack blastAttack) { }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }

    public class BannerSpecialWindDown : BaseState
    {
        private static float baseDuration = 0.4f;
        private static InterruptPriority minimumInterruptPriority = InterruptPriority.Skill;
        protected float duration;
        private Vector3 initialDirection;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            initialDirection = characterDirection.forward;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
            {
                characterMotor.moveDirection = Vector3.zero;
                characterDirection.moveVector = initialDirection;
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;

            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return minimumInterruptPriority;
        }
    }

}