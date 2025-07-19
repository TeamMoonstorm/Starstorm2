using KinematicCharacterController;
using RoR2;
using SS2;
using SS2.Components;
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
        private CharacterBody target;
        public bool rechest = false;
        public float healthPrevious;


        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;

            //Turn off gravity, so they don't fall through the ground on some uneven surfaces
            characterMotor.velocity = Vector3.zero;
            var characterGravityParameterProvider = gameObject.GetComponent<ICharacterGravityParameterProvider>();
            if (characterGravityParameterProvider != null)
            {
                CharacterGravityParameters gravityParameters = characterGravityParameterProvider.gravityParameters;
                ++gravityParameters.channeledAntiGravityGranterCount;
                characterGravityParameterProvider.gravityParameters = gravityParameters;
            } //Maybe turn this off for Rechest so they don't float in the air 

            purchaseInter = GetComponent<PurchaseInteraction>();
            if (NetworkServer.active && purchaseInter)
            {
                purchaseInter.SetAvailable(enableInteraction);
            }

            purchaseInter.onPurchase.AddListener(delegate (Interactor interactor)
            {
                OnPurchaseMimic(interactor, purchaseInter);
            });

            if (!rechest)
            {
                int adjust = UnityEngine.Random.Range(0, 2) - 1;
                purchaseInter.cost += adjust;
            }
            else if (NetworkServer.active)
            {
                ProcChainMask mask = new ProcChainMask();
                healthComponent.HealFraction(.5f, mask);
            }

            var pingc = GetComponent<MimicPingCorrecter>();
            pingc.isInteractable = true;
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
                    var next = new MimicChestActivateEnter { target = this.target };
                    outer.SetNextState(next); //leap begin
                }
            }
            
            if (healthPrevious > healthComponent.health && isAuthority)
            {
                var next = new MimicChestActivateEnter { target = this.target };
                outer.SetNextState(next); //leap begin
            }
            else
            {
                healthPrevious = healthComponent.health;
            }

            //Lock then in place so they don't fall through the map on uneven ground
            characterMotor.velocity = Vector3.zero;

        }

        //Allows mimic to cast Jumpscare while in chest mode
        void HandleSkill(GenericSkill skillSlot, ref InputBankTest.ButtonState buttonState)
        {
            if (skillSlot && !(skillSlot.skillDef == null) && (buttonState.down || !skillSlot.skillDef) && (!skillSlot.mustKeyPress || !buttonState.hasPressBeenClaimed))
            {
                if (rechest)
                {
                    if (UnityEngine.Random.Range(0, 3) == 0)
                    {
                        skillSlot.ExecuteIfReady();
                        buttonState.hasPressBeenClaimed = true;
                    }
                    else
                    {
                        skillSlot.RemoveAllStocks();
                    }
                }
                else
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
        }

        public override void OnExit()
        {
            base.OnExit();
            var anim = characterBody.modelLocator.modelTransform.GetComponent<Animator>();
            if (anim)
            {
                anim.SetBool("aimActive", true);
            }

            var characterGravityParameterProvider = gameObject.GetComponent<ICharacterGravityParameterProvider>();
            if (characterGravityParameterProvider != null)
            {
                CharacterGravityParameters gravityParameters = characterGravityParameterProvider.gravityParameters;
                --gravityParameters.channeledAntiGravityGranterCount;
                characterGravityParameterProvider.gravityParameters = gravityParameters;
            }

            var pingc = GetComponent<MimicPingCorrecter>();
            pingc.isInteractable = false;
            var kinematic = GetComponent<KinematicCharacterMotor>();
            kinematic.SetCapsuleDimensions(kinematic.CapsuleRadius, kinematic.CapsuleHeight, .925f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
