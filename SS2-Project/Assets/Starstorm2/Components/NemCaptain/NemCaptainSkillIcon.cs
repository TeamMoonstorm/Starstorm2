using RoR2;
using RoR2.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Moonstorm.Starstorm2.Components
{
    public class NemCaptainSkillIcon : MonoBehaviour
    {
        public GenericSkill targetSkill;
        public NemCaptainController ncc;
        public Image iconImage;
        public TooltipProvider tooltipProvider;
        public PlayerCharacterMasterController pcmc;

        private void Update()
        {
            if (targetSkill)
            {
                if (tooltipProvider)
                {
                    Color color = targetSkill.characterBody.bodyColor;
                    SurvivorCatalog.GetSurvivorIndexFromBodyIndex(targetSkill.characterBody.bodyIndex);
                    float h;
                    float s;
                    float v;
                    Color.RGBToHSV(color, out h, out s, out v);
                    v = ((v > 0.7f ) ? 0.7f : v);
                    color = Color.HSVToRGB(h, s, v);
                    tooltipProvider.titleColor = color;
                    tooltipProvider.titleToken = targetSkill.skillNameToken;
                    tooltipProvider.bodyToken = targetSkill.skillDescriptionToken;
                }
                if (iconImage)
                {
                    iconImage.enabled = true;
                    iconImage.color = Color.white;
                    iconImage.sprite = targetSkill.icon;
                }
                return;
            }
            if (tooltipProvider)
            {
                tooltipProvider.bodyColor = Color.gray;
                tooltipProvider.titleToken = "";
                tooltipProvider.bodyToken = "";
            }
            if (iconImage)
            {
                iconImage.enabled = false;
                iconImage.sprite = null;
            }
        }
    }
}
