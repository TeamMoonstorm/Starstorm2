using RoR2;
using UnityEngine;
namespace SS2.Components
{
    [RequireComponent(typeof(ChildLocator))]
    public class ModifiedFootstepHandler : MonoBehaviour
    {
        public string baseFootstepString;
        public string sprintFootstepOverrideString;
        public bool enableFootstepDust;
        public bool rotateToMoveDirection;
        public GameObject footstepDustPrefab;
        public GameObject footstepEffect;
        private ChildLocator childLocator;
        private Animator animator;
        private Transform footstepDustInstanceTransform;
        private ParticleSystem footstepDustInstanceParticleSystem;
        private ShakeEmitter footstepDustInstanceShakeEmitter;
        private CharacterBody body;

        private void Start()
        {
            childLocator = GetComponent<ChildLocator>();
            body = GetComponent<CharacterModel>()?.body;
            animator = GetComponent<Animator>();
            if (enableFootstepDust && footstepDustPrefab)
            {
                footstepDustInstanceTransform = Instantiate(footstepDustPrefab, transform).transform;
                footstepDustInstanceParticleSystem = footstepDustInstanceTransform.GetComponent<ParticleSystem>();
                footstepDustInstanceShakeEmitter = footstepDustInstanceTransform.GetComponent<ShakeEmitter>();
            }
        }

        public void Footstep(AnimationEvent animationEvent)
        {
            if ((double)animationEvent.animatorClipInfo.weight > 0.5)
            {
                this.Footstep(animationEvent.stringParameter);
            }
        }

        public void Footstep(string childName)
        {
            if (!body || !enableFootstepDust)
                return;
            Transform transform = childLocator.FindChild(childName);
            int childIndex = childLocator.FindChildIndex(childName);
            if (transform)
            {
                Color color = Color.gray;
                RaycastHit raycastHit = default(RaycastHit);
                Vector3 position = transform.position;
                position.y += 1.5f;
                if (Physics.Raycast(new Ray(position, Vector3.down), out raycastHit, 4f, LayerIndex.world.mask | LayerIndex.water.mask, QueryTriggerInteraction.Collide))
                {
                    //TODO: make this shit work with the normal
                    if (footstepEffect)
                    {
                        EffectData effectData = new EffectData();
                        effectData.origin = raycastHit.point;
                        
                        if (rotateToMoveDirection)
                        {
                            Vector3 forward = body.characterDirection ? body.characterDirection.forward : body.coreTransform.forward;
                            effectData.rotation = Util.QuaternionSafeLookRotation(forward, raycastHit.normal);
                        }
                        else effectData.rotation = Util.QuaternionSafeLookRotation(raycastHit.normal);
                        effectData.SetChildLocatorTransformReference(this.body.gameObject, childIndex);
                        EffectManager.SpawnEffect(footstepEffect, effectData, false);
                        //EffectManager.SimpleImpactEffect(footstepEffect, raycastHit.point, raycastHit.normal, false);
                    }
                        
                    if (footstepDustInstanceTransform)
                    {
                        footstepDustInstanceTransform.position = raycastHit.point;
                        var main = footstepDustInstanceParticleSystem.main;
                        main.startColor = color;
                        footstepDustInstanceParticleSystem.Play();
                        if (footstepDustInstanceShakeEmitter)
                            footstepDustInstanceShakeEmitter.StartShake();
                    }
                }
                Util.PlaySound((!string.IsNullOrEmpty(sprintFootstepOverrideString) && body.isSprinting) ? sprintFootstepOverrideString : baseFootstepString, body.gameObject);
                return;
            }
            Debug.LogWarningFormat("Object {0} lacks ChildLocator entry \"{1}\" to handle Footstep event!", new object[]
            {
                gameObject.name,
                childName
            });
        }


        public void OnDestroy()
        {
            if (footstepDustInstanceTransform)
                Destroy(footstepDustInstanceTransform.gameObject);
        }

    }
}
