using RoR2;
using RoR2.Hologram;
using SS2;
using SS2.Components;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EntityStates.Mimic
{
    public class MimicChestRechest : BaseState
    {
        protected PurchaseInteraction purchaseInter;

        protected virtual bool enableInteraction
        {
            get
            {
                return false;
            }
        }

        public static float baseDuration;
        private float duration;

        public bool taunting = false;

        private MimicInventoryManager mim;
		
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;

            purchaseInter = GetComponent<PurchaseInteraction>();
            if (NetworkServer.active && purchaseInter)
            {
                purchaseInter.SetAvailable(enableInteraction);
            }

            PlayAnimation("Gesture, Override", "BufferEmpty");
            PlayAnimation("FullBody, Override", "BufferEmpty");

            mim = GetComponent<MimicInventoryManager>();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            bool tryRechest = true; //Probably not needed, but nice to have
            if(mim && mim.rechestPreventionTime > 0 && !taunting){
                skillLocator.special.RemoveAllStocks();
                skillLocator.special.cooldownOverride = mim.rechestPreventionTime;
                if (isAuthority)
                {
                    tryRechest = false;
                    outer.SetNextStateToMain();
                }
            }
            
            if (fixedAge >= duration && tryRechest)
            {
                skillLocator.special.cooldownOverride = 0;
                var body = purchaseInter.gameObject;
                body.GetComponent<BoxCollider>().enabled = true;
                body.GetComponent<CapsuleCollider>().enabled = false;

                body.GetComponent<GenericDisplayNameProvider>().displayToken = "CHEST1_NAME";

                var intermediate = GetComponent<ModelLocator>();
                intermediate.modelTransform.GetComponent<ChildLocator>().FindChildGameObject("HologramPivot").SetActive(true);

                if (modelLocator && modelLocator.modelTransform)
                {
                    var mdl = modelLocator.modelTransform;
                    mdl.GetComponent<BoxCollider>().enabled = true;
                }

                purchaseInter.SetAvailable(true);

                var impact = SS2.Monsters.Mimic.rechestVFX;
                EffectData effectData = new EffectData { origin = characterBody.corePosition };
                effectData.SetNetworkedObjectReference(impact);
                EffectManager.SpawnEffect(impact, effectData, transmit: true);

                PlayAnimation("Body", "IntermediateIdle");

                GetComponent<HologramProjector>().enabled = true;
                if (isAuthority)
                {
                    var next = new MimicChestInteractableIdle { rechest = true };
                    outer.SetNextState(next);
                }
            }

        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

    }
}
