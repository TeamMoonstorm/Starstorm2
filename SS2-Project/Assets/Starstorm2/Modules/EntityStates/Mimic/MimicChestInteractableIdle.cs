using RoR2;
using SS2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Mimic
{
    public class MimicChestInteractableIdle : BaseState
    {
        protected PurchaseInteraction purchaseInter;

        protected virtual bool enableInteraction
        {
            get
            {
                return true;
            }
        }

        private bool activated = false;

        public static float baseDuration;
        private float duration;

        private float timer = 0;
        private Animator anim;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            var intermediate = GetComponent<ModelLocator>();

            int rng = UnityEngine.Random.Range(0, 2);
            SS2Log.Warning("HIIII " + rng + " | " + anim.GetBool("canTransition") + " | " + anim.GetBool("aimActive"));

            anim = characterBody.modelLocator.modelTransform.GetComponent<Animator>();
            anim.SetInteger("spawnIndex", rng);
            anim.SetBool("canTransition", true);

            purchaseInter = intermediate.modelTransform.GetComponent<PurchaseInteraction>();
            if (NetworkServer.active && purchaseInter)
            {
                purchaseInter.SetAvailable(enableInteraction);
            }
            else
            {
                SS2Log.Error("Fuck");
            }

            purchaseInter.onPurchase.AddListener(delegate (Interactor interactor)
            {
                OnPurchaseMimic(interactor, purchaseInter);
            });

            //characterBody.inventory.GiveItem()


        }

        private void OnPurchaseMimic(Interactor interactor, PurchaseInteraction purchaseInter)
        {
            PlayCrossfade("Body", "Activate", "Activate.playbackRate", 1, 0.05f);
            timer = 0;
            activated = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (activated)
            {
                timer += Time.fixedDeltaTime;
                if (timer >= duration && isAuthority)
                {
                    outer.SetNextState(new MimicChestOpen()); //leap
                }
            }
        }


        public override void OnExit()
        {
            base.OnExit();
            var anim = characterBody.modelLocator.modelTransform.GetComponent<Animator>();
            if (anim)
            {
                SS2Log.Warning("exiting InteractableIdle");
                anim.SetBool("aimActive", true);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

    }
}
