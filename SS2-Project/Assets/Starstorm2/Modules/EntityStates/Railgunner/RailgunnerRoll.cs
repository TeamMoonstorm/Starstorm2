using EntityStates.Railgunner.Backpack;
using EntityStates.Railgunner.Reload;
using RoR2;
using RoR2.Skills;
using SS2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Railgunner
{
    public class RailgunnerRoll : BaseSkillState
    {
        public static float duration = 0.7f;
        public static float initialSpeedCoefficient = 5.5f;
        public static float finalSpeedCoefficient = 3f;

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

            //PlayAnimation("FullBody, Override", "Roll", "Roll.playbackRate", duration);
           Util.PlaySound(dodgeSoundString, gameObject);

            if (isAuthority)
            {
                EntityStateMachine scopeStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Scope");
                EntityStateMachine reloadStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Reload");
                EntityStateMachine backpackStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Backpack");

                if (!scopeStateMachine || !reloadStateMachine || !backpackStateMachine || !skillLocator)
                {
                    SS2Log.Error("Tactical Roll requires Railgunner's Scope, Reload, and Backpack state machines and SkillLocator.");
                    return;
                }

                // Scope overrides must be removed before refilling the underlying skills
                if (!scopeStateMachine.IsInMainState() || scopeStateMachine.HasPendingState())
                {
                    scopeStateMachine.SetState(EntityStateCatalog.InstantiateState(ref scopeStateMachine.mainStateType));
                }

                if (backpackStateMachine.state is Offline || backpackStateMachine.nextState is Offline)
                {
                    return;
                }

                if (reloadStateMachine.nextState is BoostConfirm boostConfirm)
                {
                    // Preserve a successful manual reload that has not transitioned yet
                    reloadStateMachine.SetState(boostConfirm);
                }
                else if (reloadStateMachine.state is Waiting || reloadStateMachine.state is Reloading
                    || reloadStateMachine.nextState is Waiting)
                {
                    // Also clear a queued reload, restoring stock alone does not cancel it
                    reloadStateMachine.SetState(new Waiting());
                }

                for (int i = 0; i < skillLocator.skillSlotCount; i++)
                {
                    GenericSkill skill = skillLocator.GetSkillAtIndex(i);
                    if (skill && skill.skillDef is RailgunSkillDef railgunSkillDef && railgunSkillDef.restockOnReload)
                    {
                        skill.stock = skill.maxStock;
                    }
                }
            }
        }

        private void RecalculateRollSpeed()
        {
            rollSpeed = moveSpeedStat * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, fixedAge / duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            RecalculateRollSpeed();

            if (characterDirection) characterDirection.forward = forwardDirection;
            if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(dodgeFOV, 60f, fixedAge / duration);

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

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            characterMotor.disableAirControlUntilCollision = false;
            base.OnExit();
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