using RoR2;
using RoR2.CharacterAI;
using RoR2.Hologram;
using SS2;
using System;
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

            GetComponent<CapsuleCollider>().enabled = true;
            SS2Log.Warning("Finished enter ");
            if (target)
            {
                SS2Log.Warning("rotating to " + target);
                AimInDirection(ref ai.bodyInputs, (target.corePosition - transform.position).normalized);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration/2 && isAuthority && ai)
            {
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

            GetComponent<GenericInspectInfoProvider>().enabled = false;
            //GetComponent<GenericDisplayNameProvider>().enabled = false;
            //GetComponent<PingInfoProvider>().enabled = false;

            GetComponent<HologramProjector>().enabled = false;
            intermediate.modelTransform.GetComponent<ChildLocator>().FindChildGameObject("HologramPivot").SetActive(false);

            intermediate.modelTransform.GetComponent<BoxCollider>().enabled = false;

            characterBody.skillLocator.special.RemoveAllStocks();

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
