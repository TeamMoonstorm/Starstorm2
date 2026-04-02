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
        private static int IsHighHeatHash = Animator.StringToHash("isHighHeat");
        private static int HighHeatHash = Animator.StringToHash("highHeat");
        private static int LowHeatHash = Animator.StringToHash("lowHeat");

        private static int IsDownHash = Animator.StringToHash("isDown");
        private static int SwitchDownHash = Animator.StringToHash("switchDown");
        private static int SwitchUpHash = Animator.StringToHash("switchUp");
        private bool wasHighHeat;
        private bool wasDown;

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

        // this should REALLY be separate from the crosshair
        private void UpdateHeat()
        {
            if (pyro)
            {
                if (ifc)
                {
                    ifc.SetTValue(pyro.heat / pyro.heatMax); // not networked for spectators but who honestly cares
                }

                if (animator)
                {
                    // i might be stupid but unity animators are more stupider
                    animator.SetBool(IsHighHeatHash, pyro.isHighHeat);
                    if (pyro.isHighHeat && !wasHighHeat)
                    {
                        animator.SetTrigger(HighHeatHash);
                    }
                    if (!pyro.isHighHeat && wasHighHeat)
                    {
                        animator.SetTrigger(LowHeatHash);
                    }
                    wasHighHeat = pyro.isHighHeat;

                    animator.SetBool(IsDownHash, pyro.inNapalm);
                    if (pyro.inNapalm && !wasDown)
                    {
                        animator.SetTrigger(SwitchDownHash);
                    }
                    if (!pyro.inNapalm && wasDown)
                    {
                        animator.SetTrigger(SwitchUpHash);
                    }
                    wasDown = pyro.inNapalm;

                }

            }
        }
    }
}
