/*using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace EntityStates.Nucleator
{
    [RequireComponent(typeof(HudElement))]
    public class ChargeController : MonoBehaviour
    {
        private GameObject sourceGameObject;
        private HudElement hudElement;

        public Image image;

        private void Awake()
        {
            hudElement = GetComponent<HudElement>();
        }

        private void FixedUpdate()
        {
            float fillAmount = 0f;
            if (hudElement.targetCharacterBody)
            {
                SkillLocator component = hudElement.targetCharacterBody.GetComponent<SkillLocator>();
                if (component && component.primary && component.secondary && component.utility)
                {
                    EntityStateMachine stateMachine = component.secondary.stateMachine;
                    if (stateMachine)
                    {
                        NucleatorSkillStateBase nucleatorChargeState = stateMachine.state as NucleatorSkillStateBase;
                        if (nucleatorChargeState != null)
                        {
                            fillAmount = nucleatorChargeState.charge;
                        }
                    }
                }
            }
            if (image)
            {
                image.fillAmount = fillAmount;
            }
        }
    }
}*/