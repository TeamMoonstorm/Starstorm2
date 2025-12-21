using System;
using UnityEngine;
using RoR2;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace EntityStates.AcidBug
{
    public class Dash : BaseSkillState
    {
        private static float minDistance = 5f;
        private static float maxDistance = 12f;
        private static float baseDuration = 0.3f;

        private static int maxRepeats = 1;
        private static float repeatPercentChance = 40f;
        private static float repeatDistanceCoefficient = 0.5f;

        private static float spreadAngle = 45f;

        private static string enterSoundString;
        public static GameObject effectPrefab;

        private float duration;
        private float speed;
        private bool shouldRepeat;
        private Vector3 dashVector;

        public int repeatCount;
        private bool isRepeat => repeatCount > 0;
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
        public override void OnSerialize(NetworkWriter writer)
        {
            // these values are randomized and must be networked to play the correct animation
            writer.Write(duration);
            writer.Write(dashVector);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            duration = reader.ReadSingle();
            dashVector = reader.ReadVector3();
        }
        public override void OnEnter()
        {
            base.OnEnter();

            if (isAuthority)
            {
                duration = baseDuration; // no movespeed scaling
                float distance = UnityEngine.Random.Range(minDistance, maxDistance);
                bool right = UnityEngine.Random.Range(0f, 1f) > 0.5f;
                if (isRepeat)
                {
                    distance *= repeatDistanceCoefficient;
                }
                speed = distance / duration;
                Vector3 direction = transform.right;
                if (right == false) direction *= -1f;

                dashVector = Util.ApplySpread(direction, 0f, spreadAngle, 1f, 1f); // todo: not true random direction

                shouldRepeat = repeatCount < maxRepeats && Util.CheckRoll(repeatPercentChance);
            }

            var animator = GetModelAnimator();
            if (animator)
            {
                Vector3 forwardDirection = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
                Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection);
                float num = Vector3.Dot(dashVector, forwardDirection);
                float num2 = Vector3.Dot(dashVector, rightDirection);
                animator.SetFloat("dashForwardSpeed", num);
                animator.SetFloat("dashRightSpeed", num2);
            }

            PlayAnimation("FullBody, Override", "Dash", "Dash.playbackRate", duration);
            Util.PlaySound(enterSoundString, gameObject);
            if (effectPrefab)
            {
                var effectData = new EffectData
                {
                    origin = transform.position,
                    rotation = Util.QuaternionSafeLookRotation(dashVector),
                    start = transform.forward, // see EffectWorldRotationFromStartVector
                };
                EffectManager.SpawnEffect(effectPrefab, effectData, false);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
            {
                rigidbodyMotor.AddDisplacement(dashVector * speed * Time.fixedDeltaTime);

                if (fixedAge >= duration)
                {
                    if (shouldRepeat)
                    {
                        outer.SetNextState(new Dash { repeatCount = repeatCount++ });
                    }
                    else
                    {
                        outer.SetNextStateToMain();
                    }
                    
                }
            }
        }


    }
}
