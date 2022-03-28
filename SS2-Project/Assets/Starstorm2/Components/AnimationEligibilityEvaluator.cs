using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    public class AnimationEligibilityEvaluator : MonoBehaviour
    {
        private Animator modelAnimator;
        private CharacterBody body;
        private HealthComponent healthComponent;

        void Start()
        {
            modelAnimator = gameObject.GetComponent<ModelLocator>().modelTransform.GetComponent<Animator>();
            body = gameObject.GetComponent<CharacterBody>();
            healthComponent = gameObject.GetComponent<HealthComponent>();
            if (!(modelAnimator && body))
            {
                Debug.Log("Animator and/or CharacterBody not provided to Animation Eligibility Evaluator. Destroying.");
                Destroy(this);
            }
        }

        void Update()
        {
            if (!(modelAnimator && body))
            {
                Destroy(this);
                return;
            }
            modelAnimator.SetBool("outOfCombat", body.outOfCombat);
            modelAnimator.SetBool("outOfDanger", body.outOfDanger);
            modelAnimator.SetFloat("healthFraction", healthComponent.fullCombinedHealth);
        }
    }
}
