using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;

namespace Moonstorm.Starstorm2.Components
{
    public class CyborgChargeController : MonoBehaviour
    {
        private GenericSkill secondary;
        private GenericSkill secondaryCharged;
        private SkillLocator skillLocator;
        private CharacterBody body;
        private bool isCharged;
        public float primaryBuffDuration = 5f;

        private int previousStock;
        private float chargeExpireStopwatch;
        public float chargeExpireTime = 5f;
        private void Awake()
        {
            ///
            ///////
            //NEED UI FOR THIS 
            ///
            //
            //
            this.body = base.GetComponent<CharacterBody>();
            this.skillLocator = base.GetComponent<SkillLocator>();
            this.secondary = skillLocator.secondary;
            this.secondaryCharged = skillLocator.FindSkill("SecondaryCharged");
            if (!this.secondaryCharged)
            {
                Debug.LogError("CyborgChargeController found no suitable GenericSkill! wtf dude!");
                Destroy(this);
                return;
            }         
        }
        private void OnEnable()
        {
            this.body.onSkillActivatedServer += AddPrimaryBuff;
        }
        private void OnDisable()
        {
            this.body.onSkillActivatedServer -= AddPrimaryBuff;
        }
        private void AddPrimaryBuff(GenericSkill skill)
        {
            if(skill != this.skillLocator.primary)
            {
                this.body.AddTimedBuff(SS2Content.Buffs.BuffCyborgPrimary, this.primaryBuffDuration);
            }
        }

        private void FixedUpdate()
        {
            if(this.previousStock > 0)
                this.chargeExpireStopwatch -= Time.fixedDeltaTime;

            if(this.secondaryCharged.stock != this.previousStock)
            {
                this.chargeExpireStopwatch = this.chargeExpireTime;
            }

            if(this.chargeExpireStopwatch <= 0)
            {
                this.secondaryCharged.stock = 0;
            }

            this.previousStock = this.secondaryCharged.stock;
            this.isCharged = this.secondaryCharged.stock > 0;
            this.skillLocator.secondary = isCharged ? this.secondaryCharged : this.secondary;
        }
    }
}
