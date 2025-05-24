using RoR2;
using SS2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Mimic
{
    public class MimicChestOpen : EntityState
    {
        public static float baseDuration;
        private float duration;

        protected PurchaseInteraction purchaseInter;

        protected virtual bool enableInteraction
        {
            get
            {
                return false;
            }
        }

        public override void OnEnter()
        {
            SS2Log.Warning("Mimic Chest Open On Enter HIIIII ");
            base.OnEnter();
            
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
            PlayCrossfade("Body", "Activate", "Activate.playbackRate", 1, 0.05f);

            SS2Log.Warning("FOUND YOU ");

            var hbxg = intermediate.modelTransform.GetComponent<HurtBoxGroup>();
            foreach(var box in hbxg.hurtBoxes)
            {
                box.gameObject.SetActive(true);
            }

            intermediate.modelTransform.GetComponent<GenericInspectInfoProvider>().enabled = false;
            intermediate.modelTransform.GetComponent<PingInfoProvider>().enabled = false;
            intermediate.modelTransform.GetComponent<ChildLocator>().FindChildGameObject("HologramPivot").SetActive(false);

            intermediate.modelTransform.GetComponent<BoxCollider>().enabled = false;

            GetComponent<CapsuleCollider>().enabled = true;
            SS2Log.Warning("Finished enter ");

        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                SS2Log.Warning("leaving state ");
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            SS2Log.Warning("bye bye ");
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
