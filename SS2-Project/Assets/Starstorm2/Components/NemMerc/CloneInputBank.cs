using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using RoR2.Skills;
using EntityStates.NemMerc;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(CharacterMaster))]
    class CloneInputBank : NetworkBehaviour
    {
        private NemMercCloneTracker tracker;

        private InputBankTest ownerInputBank;

        private InputBankTest inputBank;

        [NonSerialized]
        [SyncVar]
        public GameObject ownerMasterObject;

        private NetworkUser ownerUser;

        public CharacterMaster master;

        public bool copyMovements;

        public bool copyEquipment;

        public EntityStateMachine bodyStateMachine;
        public CharacterBody body;

        [NonSerialized]
        public static DeployableSlot deployableSlot = Survivors.NemMerc.clone;
        public GameObject indicatorPrefab;

        [NonSerialized]
        public int maxRecasts = 1;


        public NetworkIdentity ownerNetId;
        private void Start()
        {
            this.body = master.GetBody();
            if (body)
            {
                this.tracker = body.GetComponent<NemMercCloneTracker>();
                this.bodyStateMachine = EntityStateMachine.FindByCustomName(body.gameObject, "Body");
                this.inputBank = body.inputBank;
                body.teamComponent.RequestDefaultIndicator(this.indicatorPrefab); // DOESNT FUCKING DO ANYTGING LOLLLLLLLLL LOL
            }


            if (!this.ownerMasterObject)
            {
                
                if(!master.minionOwnership.ownerMaster.gameObject)
                {
                    SS2Log.Error("CloneInputBank has no owner master.");
                    Destroy(this);
                    return;
                }
                this.ownerMasterObject = master.minionOwnership.ownerMaster.gameObject;

            }
            

            CharacterMaster ownerMaster = this.ownerMasterObject.GetComponent<CharacterMaster>();
            
            CharacterBody ownerBody = ownerMaster.GetBody();
            if (ownerBody)
            {
                CloneOwnership clonership = ownerBody.gameObject.AddComponent<CloneOwnership>();
                clonership.clone = this;
                clonership.maxRecasts = this.maxRecasts;
                this.ownerInputBank = ownerBody.inputBank;        
                


            }


            if (ownerMaster.playerCharacterMasterController)
            {
                this.ownerUser = ownerMaster.playerCharacterMasterController.networkUser;
            }

            if (NetworkServer.active)
            {
                ownerMaster.AddDeployable(base.GetComponent<Deployable>(), Survivors.NemMerc.clone);

                if(this.ownerUser)
                {
                    NetworkIdentity masterId = base.GetComponent<NetworkIdentity>();
                    masterId.AssignClientAuthority(this.ownerUser.connectionToClient);

                    NetworkIdentity bodyId = body.networkIdentity;
                    bodyId.AssignClientAuthority(this.ownerUser.connectionToClient);
                }              
            }


            if (this.body && this.ownerInputBank)
            {
                AttackerOverrideManager.AddOverride(this.body.gameObject, ownerInputBank.gameObject);
            }
        }

        private void OnDestroy()
        {
            if (this.body)
            {
                AttackerOverrideManager.RemoveOverride(this.body.gameObject);
            }
        }

        private void AttemptResolveBody()
        {
            SS2Log.Info("CloneInputBank: Attempting to resolve clone body");
            this.body = master.GetBody();
            if (body)
            {
                this.tracker = body.GetComponent<NemMercCloneTracker>();
                this.bodyStateMachine = EntityStateMachine.FindByCustomName(body.gameObject, "Body");
                this.bodyStateMachine.SetNextStateToMain(); // HAVE TO DO THIS TO READ OWNER INPUTS
                this.inputBank = body.inputBank;
                body.teamComponent.RequestDefaultIndicator(this.indicatorPrefab); // ????????????????????????????????????????
                this.tracker.ownerTracker = ownerInputBank.GetComponent<NemMercTracker>();

                // it just works



                SS2Log.Info("CloneInputBank: Clone body resolved!");
            }
        }

        //float s;
        //public bool l;
        private void FixedUpdate()
        {
            //if(Util.HasEffectiveAuthority(base.gameObject))
            //{
            //    s -= Time.fixedDeltaTime;
            //    if(s <= 0)
            //    {
            //        s = 1f;
            //        SS2Log.Warning("clone master auth");
            //    }
            //}

            if (!this.body && this.master && this.master.bodyResolved)
            {
                this.AttemptResolveBody();
            }

            if(this.inputBank && this.ownerInputBank)
            {
                //if(l)
                //    SS2Log.Info("in: " + this.ownerInputBank.skill1.down + "|" + this.ownerInputBank.skill2.down + "|" + this.ownerInputBank.skill3.down);
                this.inputBank.aimDirection = this.HandleAimDirection();
                this.inputBank.skill1.PushState(this.ownerInputBank.skill1.down);
                this.inputBank.skill2.PushState(this.ownerInputBank.skill2.down);
                this.inputBank.skill3.PushState(this.ownerInputBank.skill3.down);
                //this.inputBank.skill4.PushState(this.ownerInputBank.skill4.down); // FUNNY
            }
            else
            {
                SS2Log.Error("CloneInputBank: Missing InputBanks! inputBank: " + this.inputBank + " | ownerInputBank: " + this.ownerInputBank);
            }
            
            

            

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
            if(this.body && this.body.healthComponent.alive && this.bodyStateMachine)
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
            public int maxRecasts = 1;
            private int timesRecasted;
            private float cooldownRefundTimer;
            private void Awake()
            {
                this.body = base.GetComponent<CharacterBody>();

                this.detonateSkillDef = SS2Assets.LoadAsset<SkillDef>("sdCloneDash", SS2Bundle.NemMercenary);
                if (this.body)
                {
                    this.skillLocator = body.skillLocator;
                    this.skillLocator.special.SetSkillOverride(this, detonateSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
            }

            public void Reactivate()
            {
                this.clone.OnReactivation();
                this.timesRecasted++;

                if(this.timesRecasted >= maxRecasts)
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
