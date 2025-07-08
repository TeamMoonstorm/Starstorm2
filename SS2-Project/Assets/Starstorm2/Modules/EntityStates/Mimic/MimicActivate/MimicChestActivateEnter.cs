using RoR2;
using RoR2.CharacterAI;
using RoR2.Hologram;
using SS2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Mimic
{
    public class MimicChestActivateEnter : BaseState
    {
        public static float baseDuration;
        private float duration;

        protected PurchaseInteraction purchaseInter;

        private bool endedSuccessfully = false;
        BaseAI ai;
        private bool hasRotated = false;
        public CharacterBody? target;
        public HurtBox? hurt;


        GameObject lVFX;
        GameObject rVFX;
        protected virtual bool enableInteraction
        {
            get
            {
                return false;
            }
        }
        public override void OnEnter()
        {
            duration = baseDuration / attackSpeedStat;
            SS2Log.Warning("Mimic Chest Open On Enter HIIIII ");
            base.OnEnter();
            PlayCrossfade("FullBody, Override", "ActivateEnter", "Activate.playbackRate", duration, 0.05f);
            PlayCrossfade("Body", "Idle", .05f);

            var animator = GetModelAnimator();
            animator.SetBool("isGrounded", false);

            purchaseInter = GetComponent<PurchaseInteraction>();
            if (NetworkServer.active && purchaseInter)
            {
                purchaseInter.SetAvailable(enableInteraction);
                
            }
            else
            {
                SS2Log.Error("Fuck (activate  enter)");
            }

            //var ai = base.GetComponent<BaseAI>();

            var master = characterBody.master;
            if (master)
            {
                ai = master.GetComponent<BaseAI>();
            }

            //var hbxg = intermediate.modelTransform.GetComponent<HurtBoxGroup>();
            //foreach(var box in hbxg.hurtBoxes)
            //{
            //    box.gameObject.SetActive(true);
            //}

            //intermediate.modelTransform.GetComponent<GenericInspectInfoProvider>().enabled = false;
            //intermediate.modelTransform.GetComponent<GenericDisplayNameProvider>().enabled = false;
            //intermediate.modelTransform.GetComponent<PingInfoProvider>().enabled = false;
            //intermediate.modelTransform.GetComponent<ChildLocator>().FindChildGameObject("HologramPivot").SetActive(false);
            //
            //intermediate.modelTransform.GetComponent<BoxCollider>().enabled = false;

            var zipL = FindModelChild("ZipperL");
            if (zipL)
            {
                lVFX = UnityEngine.Object.Instantiate<GameObject>(SS2.Monsters.Mimic.zipperVFX, zipL);
            }

            var zipR = FindModelChild("ZipperR");
            if (zipR)
            {
                rVFX = UnityEngine.Object.Instantiate<GameObject>(SS2.Monsters.Mimic.zipperVFX, zipR);
            }


            GetComponent<CapsuleCollider>().enabled = true;
            SS2Log.Warning("Finished enter ");
            if (target)
            {
                SS2Log.Warning("rotating to " + target);
                AimInDirection(ref ai.bodyInputs, (target.corePosition - transform.position).normalized);
            }
            else
            {
                SphereSearch sphere = new SphereSearch();
                List<HurtBox> list = new List<HurtBox>();

                sphere.origin = this.transform.position;
                sphere.mask = LayerIndex.entityPrecise.mask;
                sphere.radius = 10f;
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
                Debug.Log("Updating Aim");
                ai.UpdateBodyAim(Time.fixedDeltaTime);
            }

            if (fixedAge >= duration && isAuthority)
            {
                endedSuccessfully = true;
                outer.SetNextState(new MimicChestActivateLoop());
                SS2Log.Warning("leaving state ");
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            SS2Log.Warning("MimicChestActivateEnter EXIT ");

            Destroy(lVFX);
            Destroy(rVFX);

            if (!endedSuccessfully)
            {
                PlayAnimation("FullBody, Override", "BufferEmpty");
            }

            var intermediate = GetComponent<ModelLocator>();
            var hbxg = intermediate.modelTransform.GetComponent<HurtBoxGroup>();
            foreach (var box in hbxg.hurtBoxes)
            {
                box.gameObject.SetActive(true);
            }

            //GetComponent<GenericInspectInfoProvider>().enabled = false;
            //GetComponent<GenericDisplayNameProvider>().enabled = false;
            //GetComponent<PingInfoProvider>().enabled = false;

            GetComponent<HologramProjector>().enabled = false;
            intermediate.modelTransform.GetComponent<ChildLocator>().FindChildGameObject("HologramPivot").SetActive(false);

            intermediate.modelTransform.GetComponent<BoxCollider>().enabled = false;

            characterBody.skillLocator.special.RemoveAllStocks();
            GetComponent<GenericDisplayNameProvider>().displayToken = "SS2_MIMIC_BODY_NAME";

            //SphereSearch sphere = new SphereSearch();
            //List<HurtBox> list = new List<HurtBox>();
            //
            //sphere.origin = this.transform.position;
            //sphere.mask = LayerIndex.entityPrecise.mask;
            //sphere.radius = 10f;
            //sphere.RefreshCandidates();
            //sphere.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(characterBody.teamComponent.teamIndex));
            //sphere.FilterCandidatesByDistinctHurtBoxEntities();
            //sphere.OrderCandidatesByDistance();
            //sphere.GetHurtBoxes(list);
            //sphere.ClearCandidates();
            //
            //if (target)
            //{
            //    SS2Log.Warning("rotating to " + target);
            //    AimInDirection(ref ai.bodyInputs, (target.corePosition - transform.position).normalized);
            //}
            //
            //if(list.Count > 0)
            //{
            //    SS2Log.Warning("rotating to target from SphereSearch: " + target);
            //    AimInDirection(ref ai.bodyInputs, (list[0].transform.position - transform.position).normalized);
            //}
        }

        protected void AimAt(ref BaseAI.BodyInputs dest, BaseAI.Target aimTarget)
        {
            if (aimTarget == null)
            {
                return;
            }
            Vector3 a;
            if (aimTarget.GetBullseyePosition(out a))
            {
                dest.desiredAimDirection = (a - inputBank.aimOrigin).normalized;
                
            }
        }

        // Token: 0x06001C93 RID: 7315 RVA: 0x00085C16 File Offset: 0x00083E16
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
