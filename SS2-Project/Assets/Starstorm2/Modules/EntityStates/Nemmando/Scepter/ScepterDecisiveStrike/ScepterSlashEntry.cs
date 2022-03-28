/*using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Nemmando
{
    public class ScepterSlashEntry : BaseSkillState
    {
        public float charge;
        public static float maxRecoil = 5f;
        public static float minRecoil = 0.4f;
        public static float initialMaxSpeedCoefficient = 35f;
        public static float initialMinSpeedCoefficient = 2f;
        public static float minDuration = 0.2f;
        public static float maxDuration = 0.1f;

        private float speedCoefficient;
        private float recoil;
        private float duration;

        private float dashSpeed;
        private Vector3 forwardDirection;
        private Vector3 previousPosition;
        private ChildLocator childLocator;
        private ParticleSystem dashEffect;

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.isSprinting = true;
            if (cameraTargetParams) cameraTargetParams.aimMode = CameraTargetParams.AimType.Aura;

            duration = Util.Remap(charge, 0f, 1f, minDuration, maxDuration);
            speedCoefficient = Util.Remap(charge, 0f, 1f, initialMinSpeedCoefficient, initialMaxSpeedCoefficient);
            recoil = Util.Remap(charge, 0f, 1f, minRecoil, maxRecoil);

            childLocator = GetModelChildLocator();

            forwardDirection = GetAimRay().direction;

            RecalculateSpeed();

            if (characterMotor && characterDirection)
            {
                characterMotor.velocity.y *= 0.1f;
                characterMotor.velocity = forwardDirection * dashSpeed;
            }

            PlayAnimation("FullBody, Override", "DecisiveStrikeDash");

            characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - b;

            dashEffect = childLocator.FindChild("DashEffect").GetComponent<ParticleSystem>();
            if (dashEffect) dashEffect.Play();

            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 1.5f * duration;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = Resources.Load<Material>("Materials/matDoppelganger");
                temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
            }
        }

        private void RecalculateSpeed()
        {
            dashSpeed = (4 + 0.25f * moveSpeedStat) * speedCoefficient;
        }

        public override void OnExit()
        {
            if (characterMotor) characterMotor.disableAirControlUntilCollision = false;

            characterMotor.velocity = Vector3.zero;

            characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;

            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.isSprinting = true;

            if (fixedAge >= duration && isAuthority)
            {
                if (charge >= 1f)
                {
                    ScepterSlashAttack nextState = new ScepterSlashAttack();
                    nextState.charge = charge;
                    outer.SetNextState(nextState);
                    return;
                }
                else
                {
                    BossAttack nextState = new BossAttack();
                    nextState.charge = charge;
                    outer.SetNextState(nextState);
                    return;
                }
            }

            RecalculateSpeed();

            if (isAuthority)
            {
                Vector3 normalized = (transform.position - previousPosition).normalized;

                if (characterDirection)
                {
                    if (normalized != Vector3.zero)
                    {
                        Vector3 vector = normalized * dashSpeed;
                        float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                        vector = forwardDirection * d;
                        vector.y = characterMotor.velocity.y;
                        characterMotor.velocity = vector;
                    }

                    characterDirection.forward = forwardDirection;
                }

                previousPosition = transform.position;
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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}*/