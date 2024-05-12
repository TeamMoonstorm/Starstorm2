using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Nemmando
{
    public class BossAttackEntry : BaseSkillState
    {
        public float charge;

        public static float maxRecoil;
        public static float minRecoil;
        public static float initialMaxSpeedCoefficient;
        public static float initialMinSpeedCoefficient;
        public static float minDuration;
        public static float maxDuration;
        public static GameObject dashEffect;

        private float speedCoefficient;
        private float recoil;
        private float duration;

        private float dashSpeed;
        private Vector3 forwardDirection;
        private Vector3 previousPosition;
        private ChildLocator childLocator;

        //private ParticleSystem dashEffect;
        private GameObject dashEffectInstance;

        public CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private CharacterCameraParamsData decisiveCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 70f,
            minPitch = -70f,
            pivotVerticalOffset = 2f, //how far up should the camera go?
            idealLocalCameraPos = zoomCameraPosition,
            wallCushion = 0.1f
        };
        public static Vector3 zoomCameraPosition = new Vector3(0f, 0f, -14f); // how far back should the camera go?
        public Material matInstance;
        public Material swordMat;

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.isSprinting = true;
            //if (cameraTargetParams)
            //    cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);

            cameraTargetParams.RemoveParamsOverride(camOverrideHandle, .25f);
            CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
            {
                cameraParamsData = decisiveCameraParams,
                priority = 0f
            };
            camOverrideHandle = cameraTargetParams.AddParamsOverride(request, duration);

            duration = Util.Remap(charge, 0f, 1f, minDuration, maxDuration);
            speedCoefficient = Util.Remap(charge, 0f, 1f, initialMinSpeedCoefficient, initialMaxSpeedCoefficient);
            recoil = Util.Remap(charge, 0f, 1f, minRecoil, maxRecoil);

            if (GetTeam() == TeamIndex.Monster) speedCoefficient = 0f;

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

            if ((bool)dashEffect)
            {
                Transform dashTransform = childLocator.FindChild("DashEffect");
                dashEffectInstance = Object.Instantiate(dashEffect, dashTransform);
                if ((bool)dashEffectInstance)
                {
                    foreach (var particle in dashEffectInstance.GetComponentsInChildren<ParticleSystem>())
                    {
                        particle.Play();
                    }
                }
            }

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

            if (dashEffectInstance)
            {
                Destroy(dashEffectInstance);
            }

            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.isSprinting = true;
            if (fixedAge >= duration && isAuthority)
            {
                BossAttack nextState = new BossAttack();
                nextState.charge = charge;
                nextState.camOverrideHandle = camOverrideHandle;
                nextState.matInstance = matInstance;
                nextState.swordMat = swordMat;
                outer.SetNextState(nextState);
                return;
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
}