using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.UI;

namespace SS2.Components
{
    [RequireComponent(typeof(HudElement))]
    [RequireComponent(typeof(CrosshairController))]
    [RequireComponent(typeof(ImageFillController))]
    public class PyroCrosshairController : MonoBehaviour
    {
        private PyroController pyro;
        private HudElement hudElement;
        private ImageFillController ifc;
        private Animator animator;
        private static int HighHeatHash = Animator.StringToHash("highHeat");
        private static int LowHeatHash = Animator.StringToHash("lowHeat");
        private bool wasHighHeat;

        private void Awake()
        {
            hudElement = GetComponent<HudElement>();
            ifc = GetComponent<ImageFillController>();
            animator = GetComponent<Animator>();
            if (hudElement.targetCharacterBody != null && hudElement.targetCharacterBody.TryGetComponent(out PyroController pc))
            {
                pyro = pc;
            }
            UpdateHeat();
        }

        private void Start()
        {
            if (hudElement.targetCharacterBody != null && hudElement.targetCharacterBody.TryGetComponent(out PyroController pc))
            {
                pyro = pc;
            }
            else
            {
                SS2Log.Error("PyroCrosshairController.Start : Failed to find PyroController from HudElement's TargetCharacterBody!!");
                return;
            }
            UpdateHeat();
        }

        private void FixedUpdate()
        {
            UpdateHeat();
        }

        private void UpdateHeat()
        {
            if (pyro)
            {
                if (ifc)
                {
                    ifc.SetTValue(pyro.heat / pyro.heatMax);
                }

                if (animator)
                {
                    if (pyro.isHighHeat && !wasHighHeat)
                    {
                        animator.SetTrigger(HighHeatHash);
                    }
                    if (!pyro.isHighHeat && wasHighHeat)
                    {
                        animator.SetTrigger(LowHeatHash);
                    }
                    wasHighHeat = pyro.isHighHeat;
                }
                
            }
        }
    }
}
