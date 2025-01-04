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
    /// <summary>
    /// Manages the crosshair of Nucleator
    /// </summary>
    [RequireComponent(typeof(CrosshairController))]
    public class NukeCrosshairController : MonoBehaviour
    {
        /// <summary>
        /// The cached HUDElement that accompanies the crosshair
        /// </summary>
        public HudElement hudElement { get; private set; }
        /// <summary>
        /// The cached Crosshair Controller
        /// </summary>
        public CrosshairController crosshairController { get; private set; }
        /// <summary>
        /// The cached self-damage controller, this should never be null.
        /// </summary>
        public NukeSelfDamageController nukeSelfDamageController { get; private set; }

        [Tooltip("The image component that'll get filled with nucleator's charge")]
        [SerializeField] private Image _radialFillImage;
        [Tooltip("The gradient the fill uses, where the leftmost side is no charge and the rightmost side is max charge.")]
        [SerializeField] private Gradient _radialFillColor;
        private void Awake()
        {
            hudElement = GetComponent<HudElement>();
            crosshairController = GetComponent<CrosshairController>();
        }
        //If this get component returns null, the crosshair wont do shit.
        private void Start()
        {
            nukeSelfDamageController = hudElement.targetBodyObject.GetComponent<NukeSelfDamageController>();
        }
        private void Update()
        {
            if (nukeSelfDamageController)
            {
                _radialFillImage.fillAmount = nukeSelfDamageController.crosshairChargeRemap;
                _radialFillImage.color = _radialFillColor.Evaluate(nukeSelfDamageController.crosshairChargeRemap);
            }
        }
    }
}