using RoR2;
using RoR2.Projectile;
using SS2;
using SS2.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Pyro
{
    public class PyroLaunch : BaseSkillState
    {
        public static float baseDuration;
        public static float baseMaxDuration;
        private float duration;
        private float maxDuration;

        private PyroController pyroController;

        public static float baseDurationBetweenTicks;
        private float tickRate;
        private float timer = 0f;
        private float tickTimer = 0f;
        private float heatTimer = 0f;
        private float charge = 0.35f;

        public static float baseVerticalSpeed;
        public static float walkSpeedCoefficient;
        private bool hasLaunched = false;
        private bool hasMax = false;

        public static float aimVelocity;
        public static float upwardVelocity;
        public static float forwardVelocity;
        public static float minimumY;

        public static float damageCoefficient;
        public static float heatPerTick;

        private BlastAttack blast;

        public static float miniRadius;
        public static float miniDmgCoef;
        public static float miniProcCoef;
        public static float miniProcForce;

        public static float launchHeat;
        public static float launchRadius;
        public static float launchDmgCoef;
        public static float launchProcCoef;
        public static float launchProcForce;

        private ParticleSystem leftExhaust;
        private ParticleSystem rightExhaust;

        public static GameObject fullChargePrefab;
        public static GameObject fullChargeFirePrefab;

        private bool setGravity;
        private float originalGravScale;
        public static float gravModifier = 0.4f;
        public static float shorthopVelocity;

        public override void OnEnter()
        {
            base.OnEnter();

            pyroController = GetComponent<PyroController>();
            if (pyroController == null)
            {
                outer.SetNextStateToMain();
                SS2Log.Error("PyroLaunch.OnEnter : Failed to find PyroController on body! Is this a Pyro?");
            }

            duration = baseDuration / attackSpeedStat;
            maxDuration = baseMaxDuration / attackSpeedStat;
            characterBody.hideCrosshair = true;
            
            characterMotor.walkSpeedPenaltyCoefficient = walkSpeedCoefficient;

            ChildLocator bunsDoesntLikeWhenIHaveFunCodingAnywayThisIsAChildLocatorThatWeDontReferToAgainAfterThisIfStatement = GetModelChildLocator();
            if (bunsDoesntLikeWhenIHaveFunCodingAnywayThisIsAChildLocatorThatWeDontReferToAgainAfterThisIfStatement != null)
            {
                bunsDoesntLikeWhenIHaveFunCodingAnywayThisIsAChildLocatorThatWeDontReferToAgainAfterThisIfStatement.FindChild("ExhaustLParticles").TryGetComponent(out ParticleSystem left);
                {
                    leftExhaust = left;
                }
                bunsDoesntLikeWhenIHaveFunCodingAnywayThisIsAChildLocatorThatWeDontReferToAgainAfterThisIfStatement.FindChild("ExhaustRParticles").TryGetComponent(out ParticleSystem right);
                {
                    rightExhaust = right;
                }
            }

            blast = new BlastAttack()
            {
                radius = miniRadius,
                procCoefficient = miniProcCoef,
                baseDamage = characterBody.damage * miniDmgCoef,
                damageColorIndex = DamageColorIndex.Default,
                falloffModel = BlastAttack.FalloffModel.Linear,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                teamIndex = teamComponent.teamIndex,
                position = transform.position,
                attacker = gameObject,
                baseForce = miniProcForce,
            };

            if (isAuthority && !characterMotor.isGrounded)
            {
                SmallHop(characterMotor, shorthopVelocity);
            }

            PlayCrossfade("Gesture, Override", "StartLaunch", 0.1f);
        }

        public void SetGravityOverride(bool set)
        {
            setGravity = set;

            if (setGravity)
            {
                characterMotor.gravityScale *= gravModifier;
            }
            else
            {
                characterMotor.gravityScale = originalGravScale;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.SetAimTimer(1f);

            if (isAuthority)
            {
                if (inputBank.skill3.down && !hasLaunched)
                {
                    if (pyroController.heat > 0)
                    {
                        // recalc each time in case of attack speed changes
                        tickRate = baseDurationBetweenTicks / attackSpeedStat;

                        charge += GetDeltaTime() / maxDuration;
                        charge = Mathf.Min(charge, 1f);
                        if (charge == 1f && !hasMax)
                        {
                            MaxCharge();
                            FireBlast();
                        }

                        if (hasMax)
                        {
                            tickTimer += GetDeltaTime();
                            if (tickTimer >= tickRate)
                            {
                                tickTimer -= tickRate;
                                FireBlast();
                            }
                        }

                        // counted separate so attack speed isnt a nerf
                        heatTimer += GetDeltaTime();
                        if (heatTimer >= baseDurationBetweenTicks)
                        {
                            heatTimer -= heatTimer;
                            pyroController.AddHeat(-heatPerTick);
                        }
                    }
                    else
                    {
                        LaunchBlast();
                    }
                }

                else
                {
                    LaunchBlast();
                    // HandleMovement();
                }
            }
        }

        public void MaxCharge()
        {
            hasMax = true;
            SS2Log.Info("PyroLaunch.MaxCharge : Max Charge");

            ChildLocator cl = GetModelChildLocator();
            if (cl != null && cl.TryFindChild("JetpackBase", out Transform jetpackBase))
            {
                EffectManager.SimpleEffect(fullChargePrefab, jetpackBase.position, jetpackBase.rotation, false);
            }
        }

        public void FireBlast()
        {
            blast.crit = RollCrit();
            blast.position = transform.position;
            blast.Fire();

            if (leftExhaust != null && rightExhaust != null)
            {
                leftExhaust.Play();
                rightExhaust.Play();
            }
        }

        public void LaunchBlast()
        {
            if (!hasLaunched)
            {
                pyroController.AddHeat(Mathf.Max(-launchHeat));

                if (leftExhaust != null && rightExhaust != null)
                {
                    // >

                    leftExhaust.Play();
                    leftExhaust.Play();
                    leftExhaust.Play();
                    rightExhaust.Play();
                    rightExhaust.Play();
                    rightExhaust.Play();
                }

                Vector3 aimVector = GetAimRay().direction;
                Vector3 moveVector = inputBank.moveVector;

                if (isAuthority)
                {
                    BlastAttack launchBlast = new BlastAttack()
                    {
                        radius = launchRadius,
                        procCoefficient = launchProcCoef,
                        baseDamage = characterBody.damage * launchDmgCoef,
                        damageColorIndex = DamageColorIndex.Default,
                        falloffModel = BlastAttack.FalloffModel.None,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        teamIndex = teamComponent.teamIndex,
                        attacker = gameObject,
                        baseForce = launchProcForce,
                        position = transform.position,
                        crit = RollCrit()
                    };

                    launchBlast.Fire();

                    if (hasMax)
                    {
                        ProjectileManager.instance.FireProjectile(
                            fullChargeFirePrefab,
                            characterBody.footPosition,
                            transform.rotation,
                            gameObject,
                            damageCoefficient * damageStat,
                            0f,
                            false,
                            DamageColorIndex.Default,
                            null,
                            -1);

                        EntityStateMachine bodyEsm = EntityStateMachine.FindByCustomName(gameObject, "Body");
                        if (bodyEsm != null)
                        {
                            PyroLiftoff nextState = new PyroLiftoff();
                            bodyEsm.SetNextState(nextState);
                            outer.SetNextStateToMain();
                        }
                    }
                    else
                    {
                        characterBody.isSprinting = true;
                        // aimVector.y = Mathf.Max(aimVector.y, minimumY);
                        // Vector3 aimVelocityVector = aimVector.normalized * aimVelocity * (characterBody.moveSpeed + ((moveSpeedStat - characterBody.moveSpeed) * 0.5f)); // dampened movespeed scaling
                        Vector3 upwardVelocityVector = Vector3.up * upwardVelocity;
                        Vector3 forwardVelocityVector = new Vector3(moveVector.x, 0f, moveVector.z).normalized * forwardVelocity * characterBody.moveSpeed;
                        characterMotor.Motor.ForceUnground();
                        characterMotor.velocity = (upwardVelocityVector + forwardVelocityVector);
                        characterMotor.airControl = 0;
                        pyroController.cachedAirControl = 0.25f; // low just for sake of always emulating high-charge launch

                        outer.SetNextStateToMain();
                    }
                }

                characterDirection.moveVector = aimVector;

                hasLaunched = true;
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            characterBody.hideCrosshair = false;

            PlayCrossfade("Gesture, Override", "Launch", 0.1f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
