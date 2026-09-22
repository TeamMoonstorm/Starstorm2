using RoR2;
using RoR2.UI;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.UI;
namespace SS2.Components
{
    public class NemCaptainSkillIcon : MonoBehaviour
    {
        public SkillDef targetSkill;
        public CharacterBody characterBody;
        public Image iconImage;
        public TooltipProvider tooltipProvider;
        public PlayerCharacterMasterController pcmc;

        public void UpdateSkillRef(SkillDef _targetSkill)
        {
            if (_targetSkill)
            {
                targetSkill = _targetSkill;
            }
        }

        private void Update()
        {
            if (targetSkill && characterBody)
            {
                if (tooltipProvider)
                {
                    Color color = characterBody.bodyColor;
                    SurvivorCatalog.GetSurvivorIndexFromBodyIndex(characterBody.bodyIndex);
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
