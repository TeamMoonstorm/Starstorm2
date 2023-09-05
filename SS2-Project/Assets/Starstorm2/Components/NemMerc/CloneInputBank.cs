using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using RoR2.Skills;
using EntityStates.NemMerc;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(CharacterMaster))]
    class CloneInputBank : MonoBehaviour
    {
        private NemMercTracker tracker;

        private InputBankTest ownerInputBank;

        private InputBankTest inputBank;

        public CharacterMaster master;

        public bool copyMovements;

        public bool copyEquipment;

        public EntityStateMachine bodyStateMachine;
        public CharacterBody body;

        [NonSerialized]
        public static DeployableSlot deployableSlot = Survivors.NemMerc.clone;
        public GameObject indicatorPrefab;

        private void Start()
        {
            this.body = master.GetBody();
            if (body)
            {
                this.tracker = body.GetComponent<NemMercTracker>();
                this.bodyStateMachine = EntityStateMachine.FindByCustomName(body.gameObject, "Body");
                this.inputBank = body.inputBank;
                body.teamComponent.RequestDefaultIndicator(this.indicatorPrefab);
            }

            

            CharacterMaster owner = master.minionOwnership.ownerMaster;
            if(owner)
            {
                CharacterBody ownerBody = owner.GetBody();
                if (ownerBody)
                {
                    ownerBody.gameObject.AddComponent<CloneOwnership>().clone = this;
                    this.ownerInputBank = ownerBody.inputBank;
                    
                }
                owner.AddDeployable(base.GetComponent<Deployable>(), deployableSlot);
            }
        }


        private void FixedUpdate()
        {
            this.inputBank.aimDirection = this.HandleAimDirection();
            this.inputBank.skill1.PushState(this.ownerInputBank.skill1.down);
            this.inputBank.skill2.PushState(this.ownerInputBank.skill2.down);
            this.inputBank.skill3.PushState(this.ownerInputBank.skill3.down);
            //this.inputBank.skill4.PushState(this.ownerInputBank.skill4.down); // FUNNY

            

            if(copyMovements)
            {
                this.inputBank.moveVector = this.ownerInputBank.moveVector;
                this.inputBank.jump.PushState(this.ownerInputBank.jump.down);
                this.inputBank.sprint.PushState(this.ownerInputBank.sprint.down);
            }
            if(copyEquipment)
            {
                this.inputBank.activateEquipment.PushState(this.ownerInputBank.activateEquipment.down);
            }
        }

        public Vector3 HandleAimDirection()
        {
            if(this.tracker)
            {
                GameObject gameObject = tracker.GetTrackingTarget();
                if(gameObject)
                {
                    return gameObject.transform.position - this.inputBank.aimOrigin;
                }
            }


            return this.ownerInputBank.aimDirection;
        }

        public void OnReactivation()
        {
            if(this.bodyStateMachine)
            {
                this.bodyStateMachine.SetNextState(new CloneDash { target = ownerInputBank.gameObject });
            }
        }


        public class CloneOwnership : MonoBehaviour
        {
            public CloneInputBank clone;
            private SkillDef detonateSkillDef;
            public CharacterBody body;
            private SkillLocator skillLocator;

            private float cooldownRefundTimer;
            private void Awake()
            {
                this.body = base.GetComponent<CharacterBody>();

                this.detonateSkillDef = SS2Assets.LoadAsset<SkillDef>("sdCloneDash", SS2Bundle.Indev);
                if (this.body)
                {
                    this.skillLocator = body.skillLocator;
                    this.skillLocator.special.SetSkillOverride(this, detonateSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
            }

            public void Reactivate()
            {
                this.clone.OnReactivation();
                this.UnsetOverride();
            }

            public void UnsetOverride()
            {
                if (this.skillLocator)
                {
                    this.skillLocator.special.UnsetSkillOverride(this, detonateSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                    this.skillLocator.special.RunRecharge(this.cooldownRefundTimer); 
                }
                Destroy(this);
            }

            private void FixedUpdate()
            {
                this.cooldownRefundTimer += Time.fixedDeltaTime;

                if (!this.clone)
                {
                    this.UnsetOverride();
                }
            }
        }
    }
}
