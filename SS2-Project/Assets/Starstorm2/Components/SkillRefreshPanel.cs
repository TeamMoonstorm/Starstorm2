using UnityEngine;
using RoR2;
using RoR2.UI;
using System.Collections.Generic;
namespace SS2.Components
{
    public class SkillRefreshPanel : MonoBehaviour
    {
        private static readonly List<SkillRefreshPanel> instances = new List<SkillRefreshPanel>();

        // ideally theres just a field in characterbody that tells us if charger is ready
        public static void SetActiveForBody(bool active, CharacterBody body, SkillSlot skillSlot)
        {
            foreach(SkillRefreshPanel panel in instances)
            {
                if (panel.skillIcon && panel.skillIcon.targetSkill && panel.skillIcon.targetSkill.characterBody == body)
                    panel.SetActiveSingle(active, skillSlot);
            }
        }

        private SkillIcon skillIcon;
        public Transform panel;
        bool panelActive;
        private void Start()
        {
            instances.Add(this);

            this.skillIcon = GetComponentInParent<SkillIcon>();
            panel = base.transform.Find("Panel");
            panel.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            instances.Remove(this);
        }

        public void SetActiveSingle(bool shouldBeActive, SkillSlot skillSlot)
        {
            if(skillIcon && CanSkillRefresh(skillIcon.targetSkill))
            {
                
                if (panelActive != shouldBeActive)
                {
                    this.panel.gameObject.SetActive(shouldBeActive);
                    this.panelActive = shouldBeActive;

                    if (this.skillIcon.targetSkillSlot == skillSlot)
                    {
                        if (panelActive)
                            Util.PlaySound("Play_UI_cooldownRefresh", RoR2Application.instance.gameObject);
                        else
                        {
                            // flash when charger gets consumed
                            this.skillIcon.flashPanelObject.SetActive(true);
                        }
                    }
                }
            }           
        }

        private bool CanSkillRefresh(GenericSkill skill)
        {
            return skill && skill.baseRechargeInterval > 0 && skill.skillDef.stockToConsume > 0 && skill.characterBody.skillLocator.primary != skill;
        }
    }
}
