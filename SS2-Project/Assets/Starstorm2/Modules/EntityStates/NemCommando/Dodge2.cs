using Moonstorm.Starstorm2;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemCommmando
{
    public class Dodge2 : BaseSkillState
    {
        public float duration = 0.6f;
        public float initialSpeedCoefficient = 7.5f;
        public float finalSpeedCoefficient = 1f;
        public static float dodgeFOV = -1f;
        public static GameObject JetEffect;
        public static string DashJetL;
        public static string DashJetR;

        private float rollSpeed;
        private Vector3 forwardDirection;
        private Vector3 previousPosition;
        private Animator animator;
        private EntityStateMachine swordSM;
        private NetworkStateMachine nsm;
        private string skinNameToken;

        public override void OnEnter()
        {
            base.OnEnter();

            animator = GetModelAnimator();
            ChildLocator childLocator = animator.GetComponent<ChildLocator>();
            Util.PlaySound(Commando.DodgeState.dodgeSoundString, gameObject);

            //nsm = GetComponent<NetworkStateMachine>();
            //swordSM = nsm.stateMachines[1];

            //Debug.Log("swordSM: " + swordSM.customName);

            animator.SetBool("isRolling", true);

            PlayCrossfade("Body", "Utility", "Utility.rate", duration * 1.25f, 0.05f);
            PlayCrossfade("Gesture, Override", "BufferEmpty", 0.05f);
            PlayCrossfade("FullBody, Override", "BufferEmpty", 0.05f);
            PlayCrossfade("Gesture, Override, LeftArm", "BufferEmpty", 0.05f);
            PlayCrossfade("Gesture, Additive, LeftArm", "BufferEmpty", 0.05f);

            //don't add buffs unless you're the server ty
            if (NetworkServer.active) characterBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, 1.5f * duration);

            if (isAuthority && inputBank && characterDirection)
            {
                forwardDirection = ((inputBank.moveVector == Vector3.zero) ? characterDirection.forward : inputBank.moveVector).normalized;
            }

            RecalculateRollSpeed();
            if (characterMotor && characterDirection)
            {
                characterMotor.velocity.y = 0f;
                characterMotor.velocity = forwardDirection * rollSpeed;
            }

            skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

            if ((bool)JetEffect && skinNameToken != "SS2_SKIN_NEMCOMMANDO_GRANDMASTERY")
            {
                //red
                if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_DEFAULT")
                    JetEffect = SS2Assets.LoadAsset<GameObject>("NemCommandoDashJets", SS2Bundle.NemCommando);
                //yellow
                if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_MASTERY")
                    JetEffect = SS2Assets.LoadAsset<GameObject>("NemCommandoDashJetsYellow", SS2Bundle.NemCommando);
                //blue
                if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_COMMANDO")
                    JetEffect = SS2Assets.LoadAsset<GameObject>("NemCommandoDashJetsBlue", SS2Bundle.NemCommando);
                Transform transform = childLocator.FindChild(DashJetL);
                Transform transform2 = childLocator.FindChild(DashJetR);
                if ((bool)transform)
                {
                    Object.Instantiate(JetEffect, transform);
                }
                if ((bool)transform2)
                {
                    Object.Instantiate(JetEffect, transform2);
                }
            }

            Vector3 velocity = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - velocity;

        }

        private void RecalculateRollSpeed()
        {
            rollSpeed = moveSpeedStat * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, fixedAge / duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            RecalculateRollSpeed();

            if (isAuthority)
            {
                Vector3 normalized = (transform.position - previousPosition).normalized;
                if (characterMotor && characterDirection && normalized != Vector3.zero)
                {
                    Vector3 vector = normalized * rollSpeed;
                    float y = vector.y;
                    vector.y = 0f;
                    float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                    vector = forwardDirection * d;
                    vector.y += Mathf.Max(y, 0f);
                    characterMotor.velocity = vector;

                    Vector3 rhs = inputBank ? characterDirection.forward : forwardDirection;
                    Vector3 rhs2 = Vector3.Cross(Vector3.up, rhs);
                    float num = Vector3.Dot(forwardDirection, rhs);
                    float num2 = Vector3.Dot(forwardDirection, rhs2);
                    animator.SetFloat("forwardSpeed", num);
                    animator.SetFloat("rightSpeed", num2);
                }

                previousPosition = transform.position;
                if (fixedAge >= duration && isAuthority)
                {
                    outer.SetNextStateToMain();
                    return;
                }
            }
        }

        public override void OnExit()
        {
            animator.SetBool("isRolling", false);

            if (swordSM)
            {
                //swordSM.SetNextStateToMain();
                //Debug.Log("set sword to idle");
            }

            animator.SetBool("shouldExit", true);

            if (cameraTargetParams)
                cameraTargetParams.fovOverride = -1f;
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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
