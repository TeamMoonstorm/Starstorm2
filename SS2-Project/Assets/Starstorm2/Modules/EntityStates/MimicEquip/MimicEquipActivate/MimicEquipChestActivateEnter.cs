using RoR2;
using RoR2.CharacterAI;
using RoR2.Hologram;
using SS2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.MimicEquip
{
    public class MimicEquipChestActivateEnter : BaseState
    {
        public static float baseDuration;
        private float duration;

        protected PurchaseInteraction purchaseInter;

        private bool endedSuccessfully = false;
        BaseAI ai;

        GameObject lVFX;
        GameObject rVFX;

        public CharacterBody? target;
        public HurtBox? hurt;

        protected virtual bool enableInteraction => false;

        public override void OnEnter()
        {
            duration = baseDuration / attackSpeedStat;
            base.OnEnter();

            PlayCrossfade("FullBody, Override", "ActivateEnter", "Activate.playbackRate", duration, 0.05f);

            var animator = GetModelAnimator();
            animator.SetBool("isGrounded", false);

            purchaseInter = GetComponent<PurchaseInteraction>();
            if (NetworkServer.active && purchaseInter)
            {
                purchaseInter.SetAvailable(enableInteraction);
            }

            //Replicating the chest opening zipper
            var master = characterBody.master;
            if (master)
            {
                ai = master.GetComponent<BaseAI>();
            }


            var zipL = FindModelChild("ZipperL");
            if (zipL)
            {
                lVFX = UnityEngine.Object.Instantiate<GameObject>(SS2.Monsters.MimicEquip.zipperVFX, zipL);
            }

            var zipR = FindModelChild("ZipperR");
            if (zipR)
            {
                rVFX = UnityEngine.Object.Instantiate<GameObject>(SS2.Monsters.MimicEquip.zipperVFX, zipR);
            }

            //Trying to aim at the interactor if it was purchased, or just at the nearest player
            GetComponent<CapsuleCollider>().enabled = true;
            
            if (target)
            {
                AimInDirection(ref ai.bodyInputs, (target.corePosition - transform.position).normalized);
            }
            else if(NetworkServer.active && isAuthority)
            {
                SphereSearch sphere = new SphereSearch();
                List<HurtBox> list = new List<HurtBox>();

                sphere.origin = this.transform.position;
                sphere.mask = LayerIndex.entityPrecise.mask;
                sphere.radius = 25f;
                sphere.RefreshCandidates();
                sphere.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(characterBody.teamComponent.teamIndex));
                sphere.FilterCandidatesByDistinctHurtBoxEntities();
                sphere.OrderCandidatesByDistance();
                sphere.GetHurtBoxes(list);
                sphere.ClearCandidates();
                if(list.Count > 0)
                {
                    hurt = list[0];
                    AimInDirection(ref ai.bodyInputs, (hurt.transform.position - transform.position).normalized);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration/4 && isAuthority && ai)
            {
                SS2Log.Debug($"updating aim to look at {ai.bodyInputs.desiredAimDirection}");
                ai.UpdateBodyAim(Time.fixedDeltaTime);
            }

            if (fixedAge >= duration)
            {
                endedSuccessfully = true;
                if (isAuthority)
                {
                    outer.SetNextState(new MimicEquipChestActivateLoop());
                }
            }
        }

        public override void OnExit()
        {
            Destroy(lVFX);
            Destroy(rVFX);

            if (!endedSuccessfully)
            {
                PlayAnimation("FullBody, Override", "BufferEmpty");
            }
            
            //Setup more of the CharacterBody to actually work as an enemy
            var intermediate = GetComponent<ModelLocator>();
            var hbxg = intermediate.modelTransform.GetComponent<HurtBoxGroup>();
            foreach (var box in hbxg.hurtBoxes)
            {
                box.gameObject.SetActive(true);
            }

            GetComponent<HologramProjector>().enabled = false;
            intermediate.modelTransform.GetComponent<ChildLocator>().FindChildGameObject("HologramPivot").SetActive(false);

            intermediate.modelTransform.GetComponent<BoxCollider>().enabled = false;

            characterBody.skillLocator.special.RemoveAllStocks();
            characterBody.skillLocator.secondary.AddOneStock();
            GetComponent<GenericDisplayNameProvider>().displayToken = "SS2_MIMIC_EQUIP_BODY_NAME";
            
            base.OnExit();
        }
        

        protected void AimInDirection(ref BaseAI.BodyInputs dest, Vector3 aimDirection)
        {
            if (aimDirection != Vector3.zero)
            {
                dest.desiredAimDirection = aimDirection;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
