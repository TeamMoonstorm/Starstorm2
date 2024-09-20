using UnityEngine;
using RoR2;
using RoR2.UI;
using System.Collections.Generic;
namespace SS2.Components
{
    public class SkillRefreshPanel : MonoBehaviour
    {
        private static readonly List<SkillRefreshPanel> instances = new List<SkillRefreshPanel>();
        public static void SetActive(bool active, SkillSlot skillSlot)
        {
            foreach(SkillRefreshPanel panel in instances)
            {
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
            this.panel.gameObject.SetActive(shouldBeActive);
            if (panelActive != shouldBeActive)
            {
                this.panelActive = shouldBeActive;

                if(this.skillIcon && this.skillIcon.targetSkillSlot == skillSlot)
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
}
