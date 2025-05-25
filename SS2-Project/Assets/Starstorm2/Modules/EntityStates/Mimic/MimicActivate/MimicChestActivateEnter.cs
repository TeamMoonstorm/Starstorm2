using RoR2;
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
            var intermediate = GetComponent<ModelLocator>();

            purchaseInter = intermediate.modelTransform.GetComponent<PurchaseInteraction>();
            if (NetworkServer.active && purchaseInter)
            {
                purchaseInter.SetAvailable(enableInteraction);
            }
            else
            {
                SS2Log.Error("Fuck");
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

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
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

            intermediate.modelTransform.GetComponent<GenericInspectInfoProvider>().enabled = false;
            intermediate.modelTransform.GetComponent<GenericDisplayNameProvider>().enabled = false;
            intermediate.modelTransform.GetComponent<PingInfoProvider>().enabled = false;
            intermediate.modelTransform.GetComponent<ChildLocator>().FindChildGameObject("HologramPivot").SetActive(false);

            intermediate.modelTransform.GetComponent<BoxCollider>().enabled = false;

            characterBody.skillLocator.special.RemoveAllStocks();

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
