using RoR2;
using SS2;
using UnityEngine;

namespace EntityStates
{
    public class FearedState : BaseState
    {
        private static float turnSpeed = 360f;
        private static string enterSoundString = "Play_voidman_R_pop";

        [SerializeField] public GameObject effectPrefab;

        public float duration;
        public GameObject target;

        private GameObject effectInstance;
        private float cachedTurnSpeed;
        private Vector3 lastKnownTargetPosition;
        private Collider bodyCollider;

        public override void OnEnter()
        {
            base.OnEnter();

            if (characterDirection)
            {
                cachedTurnSpeed = characterDirection.turnSpeed;
                characterDirection.turnSpeed = turnSpeed;
            }

            bodyCollider = GetComponent<Collider>();
            if(effectPrefab)
            {
                effectInstance = GameObject.Instantiate(effectPrefab, characterBody.coreTransform.position, Quaternion.identity);
            }
            Util.PlaySound(enterSoundString, gameObject);
        }
        private void PlayStunAnimation()
        {
            Animator modelAnimator = base.GetModelAnimator();
            if (modelAnimator)
            {
                modelAnimator.CrossFadeInFixedTime((UnityEngine.Random.Range(0, 2) == 0) ? "Hurt1" : "Hurt2", 0.1f);
                modelAnimator.Update(0f);
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(isAuthority)
            {
                if (target != null)
                {
                    lastKnownTargetPosition = target.transform.position;
                }

                Vector3 direction = (transform.position - lastKnownTargetPosition).normalized;
                if (characterMotor)
                {
                    characterMotor.moveDirection = direction;
                }
                if (rigidbodyMotor)
                {
                    rigidbodyMotor.moveVector = direction * moveSpeedStat;
                }

                if(fixedAge >= duration)
                {
                    outer.SetNextStateToMain();
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if (effectInstance)
            {
                Vector3 a = transform.position;
                if (bodyCollider)
                {
                    a = bodyCollider.bounds.center + new Vector3(0f, bodyCollider.bounds.extents.y, 0f);
                }
                effectInstance.transform.position = a;
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (characterDirection)
            {
                characterDirection.turnSpeed = cachedTurnSpeed;
            }
        }
    }

    public class FearedStateRed : BaseState
    {

    }
}
