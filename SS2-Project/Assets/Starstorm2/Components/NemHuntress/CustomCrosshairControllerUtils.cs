using UnityEngine;
using RoR2.UI;
namespace SS2.UI
{
    [RequireComponent(typeof(HudElement))]
    [RequireComponent(typeof(RectTransform))]
    public class CustomCrosshairControllerUtils : MonoBehaviour
    {
        public RectTransform rectTransform { get; private set; }
        public HudElement hudElement { get; private set; }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            hudElement = GetComponent<HudElement>();
        }
        
        /*private void SetCrosshairTransparency()
        {
            float bloomAngle = 0f;
            if (hudElement.targetCharacterBody)
            {
                bloomAngle = hudElement.targetCharacterBody.spreadBloomAngle;
            }
            for (int i = 0; i <)
        }    */
    }
}
