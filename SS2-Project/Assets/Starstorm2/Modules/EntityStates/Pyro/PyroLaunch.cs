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
        public static AnimationCurve speedCoefficientCurve;
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
        private float charge = 0.3f;

        public static float baseVerticalSpeed;
        public static float walkSpeedCoefficient;
        private Vector3 flyVector = Vector3.zero;
        private bool hasLaunched = false;
        private float verticalSpeed;
        private float launchTimer;
        private bool hasMax = false; 

        public static float damageCoefficient;
        public static float heatPerTick;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;
            maxDuration = baseMaxDuration / attackSpeedStat;
            verticalSpeed = baseVerticalSpeed; // movespeed scaling?
            flyVector = Vector3.up;
            characterBody.hideCrosshair = true;

            characterMotor.walkSpeedPenaltyCoefficient = walkSpeedCoefficient;

            pc = GetComponent<PyroController>();
            if (pc == null)
            {
                outer.SetNextStateToMain();
                SS2Log.Error("PyroLaunch.OnEnter : Failed to find PyroController on body! Is this a Pyro?");
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.SetAimTimer(1f);

            if (inputBank.skill3.down && !hasLaunched)
            {
                if (characterMotor.isGrounded && pc.heat >= 0)
                {
                    // recalc each time in case of attack speed changes
                    tickRate = baseDurationBetweenTicks / attackSpeedStat;
                    tickTimer += GetDeltaTime();
                    if (tickTimer >= tickRate)
                    {
                        tickRate -= tickTimer;
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
                HandleMovement();
            }
        }

        public void MaxCharge()
        {
            hasMax = true;
            SS2Log.Info("PyroLaunch.MaxCharge : Max Charge");
        }

        public void HandleMovement()
        {
            if (characterMotor.isGrounded)
            {
                characterMotor.Motor.ForceUnground();
            }

            SS2Log.Debug("handlemovement");

            launchTimer += GetDeltaTime();

            float speed = verticalSpeed * charge * speedCoefficientCurve.Evaluate(launchTimer / duration);
            characterMotor.rootMotion += flyVector * speed * GetDeltaTime();
            characterMotor.velocity.y = 0f;

            characterMotor.moveDirection = inputBank.moveVector;
            characterDirection.forward = inputBank.aimDirection;
            characterDirection.targetVector = inputBank.aimDirection;
            characterDirection.moveVector = Vector3.zero;

            if (launchTimer >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public void FireBlast()
        {

        }

        public void LaunchBlast()
        {
            if (!hasLaunched)
            {
                pc.AddHeat(Mathf.Max(-heatPerTick * 3f));

                if (NetworkServer.active)
                {
                    characterBody.AddBuff(SS2.Survivors.Pyro._bdPyroJet);
                }
            }

            hasLaunched = true;
        }

        public override void OnExit()
        {
            base.OnExit();

            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            characterBody.hideCrosshair = false;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
