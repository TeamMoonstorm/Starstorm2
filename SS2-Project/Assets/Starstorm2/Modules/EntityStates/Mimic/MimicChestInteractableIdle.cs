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
        private CharacterBody target;
		
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            //var intermediate = GetComponent<ModelLocator>();

            purchaseInter = GetComponent<PurchaseInteraction>();
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

            //modelLocator

            //characterBody.inventory.GiveItem()


        }

        private void OnPurchaseMimic(Interactor interactor, PurchaseInteraction purchaseInter)
        {
            PlayCrossfade("Body", "Activate", "Activate.playbackRate", 1, 0.05f);
            timer = 0;
            activated = true;
            target = interactor.GetComponent<CharacterBody>();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            HandleSkill(base.skillLocator.special, ref base.inputBank.skill4);

            if (activated)
            {
                timer += Time.fixedDeltaTime;
                if (timer >= duration && isAuthority)
                {
                    var next = new MimicChestActivateEnter();
                    next.target = target;
                    outer.SetNextState(next); //leap begin
                }
            }
        }
        void HandleSkill(GenericSkill skillSlot, ref InputBankTest.ButtonState buttonState)
        {
            if ((bool)skillSlot && !(skillSlot.skillDef == null) && (buttonState.down || !skillSlot.skillDef) && (!skillSlot.mustKeyPress || !buttonState.hasPressBeenClaimed))
            {
                if (UnityEngine.Random.Range(0, 4) == 0)
                {
                    skillSlot.ExecuteIfReady();
                    buttonState.hasPressBeenClaimed = true;
                }
                else
                {
                    skillSlot.RemoveAllStocks();
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
