using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using RoR2.UI;
namespace Moonstorm.Starstorm2.Components
{
    public class SkillRefreshPanel : MonoBehaviour
    {
        private SkillIcon skillIcon;
        public Transform panel;
        bool panelActive;
        private void Start()
        {
            this.skillIcon = GetComponentInParent<SkillIcon>();
            panel = base.transform.Find("Panel");

        }

        private void Update()
        {
            if(skillIcon.targetSkill && skillIcon.targetSkill.characterBody)
            {
                CharacterBody body = skillIcon.targetSkill.characterBody;
                GenericSkill skill = skillIcon.targetSkill;
                bool shouldBeActive = body.HasBuff(SS2Content.Buffs.BuffUniversalCharger) && skill.finalRechargeInterval > 0 && skill.IsReady();
                this.panel.gameObject.SetActive(shouldBeActive);
                if(panelActive != shouldBeActive)
                {
                    this.panelActive = shouldBeActive;
                    if(panelActive)
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
}
