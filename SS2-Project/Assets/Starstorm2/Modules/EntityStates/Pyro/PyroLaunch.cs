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
        public static float baseMaxDuration;
        private float maxDuration;

        private PyroController pyroController;

        private static float minDuration = 0.3f;
        private static float heatInterval = 0.2f;

        public static float walkSpeedCoefficient;
        private bool hasLaunched = false;
        private bool hasMax = false;

        public static float heatPerTick;

        public static float launchHeat;

        private ParticleSystem leftExhaust;
        private ParticleSystem rightExhaust;

        public static GameObject fullChargePrefab;

        public static float shorthopVelocity;

        private float heatTimer = 0f;
        private float charge = 0f;
        public override void OnEnter()
        {
            base.OnEnter();

            pyroController = GetComponent<PyroController>();
            if (pyroController == null)
            {
                outer.SetNextStateToMain();
                SS2Log.Error("PyroLaunch.OnEnter : Failed to find PyroController on body! Is this a Pyro?");
            }

            maxDuration = baseMaxDuration / attackSpeedStat;            
            characterMotor.walkSpeedPenaltyCoefficient = walkSpeedCoefficient;

            ChildLocator bunsDoesntLikeWhenIHaveFunCodingAnywayThisIsAChildLocatorThatWeDontReferToAgainAfterThisIfStatement = GetModelChildLocator();
            if (bunsDoesntLikeWhenIHaveFunCodingAnywayThisIsAChildLocatorThatWeDontReferToAgainAfterThisIfStatement != null)
            {
                if (bunsDoesntLikeWhenIHaveFunCodingAnywayThisIsAChildLocatorThatWeDontReferToAgainAfterThisIfStatement.FindChild("ExhaustLParticles").TryGetComponent(out ParticleSystem left))
                {
                    leftExhaust = left;
                }
                if (bunsDoesntLikeWhenIHaveFunCodingAnywayThisIsAChildLocatorThatWeDontReferToAgainAfterThisIfStatement.FindChild("ExhaustRParticles").TryGetComponent(out ParticleSystem right))
                {
                    rightExhaust = right;
                }
            }

            if (isAuthority && !characterMotor.isGrounded)
            {
                SmallHop(characterMotor, shorthopVelocity);
            }

            PlayCrossfade("Gesture, Override", "StartLaunch", 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.SetAimTimer(1f);

            if (isAuthority)
            {
                if (!hasLaunched)
                {
                    if (pyroController.heat > 0)
                    {
                        charge += GetDeltaTime() / maxDuration;
                        charge = Mathf.Min(charge, 1f);
                        if (charge == 1f && !hasMax)
                        {
                            MaxCharge();
                        }

                        // counted separate so attack speed isnt a nerf
                        if (!hasMax)
                        {
                            heatTimer += GetDeltaTime();
                            if (heatTimer >= heatInterval)
                            {
                                heatTimer -= heatInterval;
                                pyroController.AddHeat(-heatPerTick);
                            }
                        }
                    }
                }

                if (fixedAge >= minDuration && (!inputBank.skill3.down || pyroController.heat <= 0))
                {
                    LaunchBlast();
                }
            }
        }

        public void MaxCharge()
        {
            hasMax = true;

            ChildLocator cl = GetModelChildLocator();
            if (cl != null && cl.TryFindChild("JetpackBase", out Transform jetpackBase))
            {
                EffectManager.SimpleEffect(fullChargePrefab, jetpackBase.position, jetpackBase.rotation, false);
            }
        }


        public void LaunchBlast()
        {
            if (!hasLaunched)
            {
                hasLaunched = true;
                

                Vector3 aimVector = GetAimRay().direction;
                Vector3 moveVector = inputBank.moveVector;

                if (isAuthority)
                {
                    pyroController.AddHeat(Mathf.Max(-launchHeat));

                    EntityStateMachine jetpack = EntityStateMachine.FindByCustomName(gameObject, "Jetpack");
                    if (jetpack != null)
                    {
                        PyroLiftoff nextState = new PyroLiftoff();
                        nextState.charge = charge;
                        jetpack.SetNextState(nextState);
                        outer.SetNextStateToMain();
                    }
                    
                }

                characterDirection.moveVector = aimVector;

            }
        }

        public override void OnExit()
        {
            base.OnExit();

            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            PlayCrossfade("Gesture, Override", "Launch", 0.1f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
