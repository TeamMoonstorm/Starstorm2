using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SS2.Components
{
    [RequireComponent(typeof(CrosshairController))]
    public class NukeCrosshairController : MonoBehaviour
    {
        public HudElement hudElement { get; private set; }
        public CrosshairController crosshairController { get; private set; }
        public NukeSelfDamageController nukeSelfDamageController { get; private set; }

        [SerializeField] private Image _radialFillImage;
        [SerializeField] private Gradient _radialFillColor;
        private void Awake()
        {
            hudElement = GetComponent<HudElement>();
            crosshairController = GetComponent<CrosshairController>();
        }
        private void Start()
        {
            nukeSelfDamageController = hudElement.targetBodyObject.GetComponent<NukeSelfDamageController>();
        }
        private void Update()
        {
            if(nukeSelfDamageController)
            {
                _radialFillImage.fillAmount = nukeSelfDamageController.crosshairChargeRemap;
                _radialFillImage.color = _radialFillColor.Evaluate(nukeSelfDamageController.crosshairChargeRemap);
            }
        }
    }
}