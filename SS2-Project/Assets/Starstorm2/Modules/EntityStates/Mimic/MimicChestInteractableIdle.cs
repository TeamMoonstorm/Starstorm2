using KinematicCharacterController;
using RoR2;
using RoR2.Hologram;
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

                //Turn off gravity, so they don't fall through the ground on some uneven surfaces
                characterMotor.velocity = Vector3.zero;
                var characterGravityParameterProvider = gameObject.GetComponent<ICharacterGravityParameterProvider>();
                if (characterGravityParameterProvider != null)
                {
                    CharacterGravityParameters gravityParameters = characterGravityParameterProvider.gravityParameters;
                    ++gravityParameters.channeledAntiGravityGranterCount;
                    characterGravityParameterProvider.gravityParameters = gravityParameters;
                }
            }
            else
            {
                characterBody.GetComponent<BoxCollider>().enabled = true;
                characterBody.GetComponent<CapsuleCollider>().enabled = false;

                characterBody.GetComponent<GenericDisplayNameProvider>().displayToken = "CHEST1_NAME";

                var intermediate = GetComponent<ModelLocator>();
                intermediate.modelTransform.GetComponent<ChildLocator>().FindChildGameObject("HologramPivot").SetActive(true);

                if (modelLocator && modelLocator.modelTransform)
                {
                    var mdl = modelLocator.modelTransform;
                    mdl.GetComponent<BoxCollider>().enabled = true;
                }

                var impact = SS2.Monsters.Mimic.rechestVFX;
                EffectData effectData = new EffectData { origin = characterBody.corePosition };
                effectData.SetNetworkedObjectReference(impact);
                EffectManager.SpawnEffect(impact, effectData, transmit: true);

                PlayAnimation("Body", "IntermediateIdle");

                GetComponent<HologramProjector>().enabled = true;

                if (NetworkServer.active)
                {
                    ProcChainMask mask = new ProcChainMask();
                    healthComponent.HealFraction(.5f, mask);
                    Util.CleanseBody(characterBody, true, true, true, true, true, false);
                    
                }
            }

            var setstate = GetComponent<SetStateOnHurt>();
            if (setstate)
            {
                setstate.canBeFrozen = false;
                setstate.canBeStunned = false;
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

            
            if (rechest)
            {
                //After rechesting, they'll be fine - just make it so they can't move, but do fall correctly
                characterMotor.velocity.x = 0;
                characterMotor.velocity.z = 0;
            }
            else
            {
                //Lock then in place so they don't fall through the map on uneven ground
                characterMotor.velocity = Vector3.zero;
            }

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

            if (!rechest) 
            {
                var characterGravityParameterProvider = gameObject.GetComponent<ICharacterGravityParameterProvider>();
                if (characterGravityParameterProvider != null)
                {
                    CharacterGravityParameters gravityParameters = characterGravityParameterProvider.gravityParameters;
                    --gravityParameters.channeledAntiGravityGranterCount;
                    characterGravityParameterProvider.gravityParameters = gravityParameters;
                }
            }

            var pingc = GetComponent<MimicPingCorrecter>();
            pingc.isInteractable = false;

            //Set their height to a value more appropriate for moving nicely visually
            var kinematic = GetComponent<KinematicCharacterMotor>();
            kinematic.SetCapsuleDimensions(kinematic.CapsuleRadius, kinematic.CapsuleHeight, .925f);

            var setstate = GetComponent<SetStateOnHurt>();
            if (setstate)
            {
                setstate.canBeFrozen = true;
                setstate.canBeStunned = true;
            }


            if (NetworkServer.active && purchaseInter)
            {
                purchaseInter.SetAvailable(false);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
