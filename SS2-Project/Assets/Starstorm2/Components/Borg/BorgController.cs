/*using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(CharacterBody), typeof(SkillLocator))]
    public class BorgController : MonoBehaviour
    {
        [Serializable]
        public struct HeatSink
        {
            internal bool available;

            internal float cooldown;

            private float timer;

            private const float Cooldown = 10f;
            public void Awake()
            {
                available = true;
                cooldown = 10;
                timer = 0;
            }

            public void Update()
            {
                if (!available)
                {
                    timer += Time.fixedTime;
                    if (timer >= cooldown)
                    {
                        cooldown = 0;
                        available = true;
                    }
                }
            }
        }

        public List<HeatSink> heatSinks;

        private SkillLocator SkillLocator { get => gameObject.GetComponent<SkillLocator>(); }

        private CharacterBody CharBody { get => gameObject.GetComponent<CharacterBody>(); }

        /*private bool HeatSink1 { get => heatSinks[0].available; set => heatSinks[0].available = value; }
        private bool HeatSink2 { get => heatSinks[1].available; set => heatSinks[1].available = value; }
        private bool HeatSink3 { get => heatSinks[2].available; set => heatSinks[2].available = value; }

        private bool HeatSinkAvailable { get => heatSinks.First(x => x.available).available; }

        private bool pressedAlt = false;

        public void Update()
        {
            UpdateHeatsinks();
            if (Input.GetKeyDown(SS2Config.ExtraKeyCode.Value))
            {
                if (HeatSinkAvailable)
                    pressedAlt = !pressedAlt;
                else
                    pressedAlt = false;
            }
            if (HeatSinkAvailable && pressedAlt)
            {
                if (CharBody.inputBank.skill2.justPressed && SkillLocator.secondary.CanApplyAmmoPack())
                {
                    BypassCooldown(SkillSlot.Secondary);
                }

                if (CharBody.inputBank.skill3.justPressed && SkillLocator.secondary.CanApplyAmmoPack())
                {
                    BypassCooldown(SkillSlot.Utility);
                }

                if (CharBody.inputBank.skill4.justPressed && SkillLocator.secondary.CanApplyAmmoPack())
                {
                    BypassCooldown(SkillSlot.Special);
                }
            }
        }
        private void BypassCooldown(SkillSlot slot)
        {
            HeatSink heatSink;
            int index;
            switch (slot)
            {
                case SkillSlot.None:
                    break;
                case SkillSlot.Primary:
                    break;
                case SkillSlot.Secondary:
                    heatSink = heatSinks.First(x => x.available);
                    index = heatSinks.IndexOf(heatSink);
                    heatSink.available = false;
                    heatSinks[index] = heatSink;
                    SkillLocator.secondary.ApplyAmmoPack();
                    break;
                case SkillSlot.Utility:
                    heatSink = heatSinks.First(x => x.available);
                    index = heatSinks.IndexOf(heatSink);
                    heatSink.available = false;
                    heatSinks[index] = heatSink;
                    SkillLocator.utility.ApplyAmmoPack();
                    break;
                case SkillSlot.Special:
                    heatSink = heatSinks.First(x => x.available);
                    index = heatSinks.IndexOf(heatSink);
                    heatSink.available = false;
                    heatSinks[index] = heatSink;
                    SkillLocator.special.ApplyAmmoPack();
                    break;
            }
        }
        private void UpdateHeatsinks()
        {
            foreach (HeatSink hs in heatSinks)
                hs.Update();
        }
    }
}
*/