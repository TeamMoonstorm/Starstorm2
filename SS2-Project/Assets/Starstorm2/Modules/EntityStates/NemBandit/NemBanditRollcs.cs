using System;
using System.Collections.Generic;
using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemBandit
{
    //Need to split this into two states
    public class NemBanditRoll : BaseSkillState
    {
        public static float duration = 5f;
        public static float rollDuration = 0.5f;
        public static float initialSpeedCoefficient = 5f;
        public static float finalSpeedCoefficient = 2.5f;

        public static string dodgeSoundString = "Play_commando_shift";
        public static float dodgeFOV = Commando.DodgeState.dodgeFOV;

        private float rollSpeed;
        private Vector3 forwardDirection;
        private Animator animator;
        private Vector3 previousPosition;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();

            if (isAuthority && inputBank && characterDirection)
            {
                forwardDirection = (inputBank.moveVector == Vector3.zero ? characterDirection.forward : inputBank.moveVector).normalized;
            }

            Vector3 rhs = characterDirection ? characterDirection.forward : forwardDirection;
            Vector3 rhs2 = Vector3.Cross(Vector3.up, rhs);

            float num = Vector3.Dot(forwardDirection, rhs);
            float num2 = Vector3.Dot(forwardDirection, rhs2);

            RecalculateRollSpeed();

            if (characterMotor && characterDirection)
            {
                characterMotor.velocity.y = 0f;
                characterMotor.velocity = forwardDirection * rollSpeed;
            }

            Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - b;

            if (characterBody)
            {
                if (NetworkServer.active)
                {
                    characterBody.AddBuff(RoR2Content.Buffs.Cloak);
                    characterBody.AddBuff(RoR2Content.Buffs.CloakSpeed);
                }
                characterBody.onSkillActivatedAuthority += CharacterBody_onSkillActivatedAuthority;
            }

            PlayAnimation("FullBody, Override", "Roll", "Roll.playbackRate", duration);
            Util.PlaySound(dodgeSoundString, gameObject);
        }

        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            base.OnExit();

            characterMotor.disableAirControlUntilCollision = false;

            if (characterBody)
            {
                characterBody.onSkillActivatedAuthority -= CharacterBody_onSkillActivatedAuthority;
                if (NetworkServer.active)
                {
                    characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
                    characterBody.RemoveBuff(RoR2Content.Buffs.CloakSpeed);
                }
            }
            base.OnExit();
        }

        private void CharacterBody_onSkillActivatedAuthority(GenericSkill obj)
        {
            if (fixedAge <= rollDuration && obj.skillDef.isCombatSkill)
            {
                outer.SetNextStateToMain();
            }
        }

        private void RecalculateRollSpeed()
        {
            rollSpeed = moveSpeedStat * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, fixedAge / rollDuration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge <= rollDuration)
            {
                RecalculateRollSpeed();

                if (characterDirection) characterDirection.forward = forwardDirection;
                if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(dodgeFOV, 60f, fixedAge / rollDuration);

                Vector3 normalized = (transform.position - previousPosition).normalized;
                if (characterMotor && characterDirection && normalized != Vector3.zero)
                {
                    Vector3 vector = normalized * rollSpeed;
                    float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                    vector = forwardDirection * d;
                    vector.y = 0f;

                    characterMotor.velocity = vector;
                }
                previousPosition = transform.position;
            }
            

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(forwardDirection);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            forwardDirection = reader.ReadVector3();
        }
    }
}