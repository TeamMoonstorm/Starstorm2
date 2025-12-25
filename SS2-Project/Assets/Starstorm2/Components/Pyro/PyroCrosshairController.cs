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

        private void Awake()
        {
            hudElement = GetComponent<HudElement>();
            ifc = GetComponent<ImageFillController>();
        }

        private void Start()
        {
            if (hudElement.targetCharacterBody.TryGetComponent(out PyroController pc))
            {
                pyro = pc;
            }
            else
            {
                SS2Log.Error("PyroCrosshairController.Start : Failed to find PyroController from HudElement's TargetCharacterBody!!");
                return;
            }
        }

        private void FixedUpdate()
        {
            ifc.SetTValue(pyro.heat / pyro.heatMax);
        }
    }
}
