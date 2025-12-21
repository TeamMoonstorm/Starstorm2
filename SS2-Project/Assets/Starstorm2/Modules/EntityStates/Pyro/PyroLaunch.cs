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

        private PyroController pc;

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

        public override void OnEnter()
        {
            base.OnEnter();

            pc = GetComponent<PyroController>();
            if (pc == null)
            {
                outer.SetNextStateToMain();
                SS2Log.Error("PyroLaunch.OnEnter : Failed to find PyroController on body! Is this a Pyro?");
            }

            duration = baseDuration / attackSpeedStat;
            maxDuration = baseMaxDuration / attackSpeedStat;
            characterBody.hideCrosshair = true;

            characterMotor.walkSpeedPenaltyCoefficient = walkSpeedCoefficient;

            ChildLocator maralago = GetModelChildLocator();
            if (maralago != null)
            {
                maralago.FindChild("ExhaustLParticles").TryGetComponent(out ParticleSystem left);
                {
                    leftExhaust = left;
                }
                maralago.FindChild("ExhaustRParticles").TryGetComponent(out ParticleSystem right);
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

            PlayCrossfade("Gesture, Override", "StartLaunch", 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.SetAimTimer(1f);

            if (isAuthority)
            {
                if (inputBank.skill3.down && !hasLaunched)
                {
                    if (characterMotor.isGrounded && pc.heat > 0)
                    {
                        // recalc each time in case of attack speed changes
                        tickRate = baseDurationBetweenTicks / attackSpeedStat;
                        tickTimer += GetDeltaTime();
                        if (tickTimer >= tickRate)
                        {
                            tickTimer -= tickRate;
                            FireBlast();
                        }

                        charge += GetDeltaTime() / maxDuration;
                        charge = Mathf.Min(charge, 1f);
                        if (charge == 1f && !hasMax)
                        {
                            MaxCharge();
                        }

                        // counted separate so attack speed isnt a nerf
                        heatTimer += GetDeltaTime();
                        if (heatTimer >= baseDurationBetweenTicks)
                        {
                            heatTimer -= heatTimer;
                            pc.AddHeat(-heatPerTick);
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
                pc.AddHeat(Mathf.Max(-launchHeat));

                if (NetworkServer.active)
                {
                    characterBody.AddBuff(SS2.Survivors.Pyro._bdPyroJet);
                }

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
                        position = transform.position,
                        attacker = gameObject,
                        baseForce = launchProcForce,
                        crit = RollCrit()
                    };

                    launchBlast.Fire();

                    if (hasMax)
                    {
                        ProjectileManager.instance.FireProjectile(
                            fullChargeFirePrefab,
                            characterBody.footPosition,
                            Util.QuaternionSafeLookRotation(GetAimRay().direction),
                            gameObject,
                            damageCoefficient * damageStat,
                            0f,
                            false,
                            DamageColorIndex.Default,
                            null,
                            -1);
                    }

                    characterBody.isSprinting = true;
                    aimVector.y = Mathf.Max(aimVector.y, minimumY);
                    Vector3 aimVelocityVector = aimVector.normalized * aimVelocity * (characterBody.moveSpeed + ((moveSpeedStat - characterBody.moveSpeed) * 0.5f)); // dampened movespeed scaling
                    Vector3 upwardVelocityVector = Vector3.up * upwardVelocity;
                    Vector3 forwardVelocityVector = new Vector3(aimVector.x, 0f, aimVector.z).normalized * forwardVelocity;
                    characterMotor.Motor.ForceUnground();
                    characterMotor.velocity = (aimVelocityVector + upwardVelocityVector + forwardVelocityVector) * (charge);

                    outer.SetNextStateToMain();
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
