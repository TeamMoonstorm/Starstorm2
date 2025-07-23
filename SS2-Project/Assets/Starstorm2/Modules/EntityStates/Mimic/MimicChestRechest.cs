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
        public bool tryRechest = true;
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

            if (mim && mim.rechestPreventionTime > 0 && !taunting){
                tryRechest = false;
                if (isAuthority)
                {
                    skillLocator.utility.RemoveAllStocks();
                    skillLocator.utility.RunRecharge(Mathf.Max(0, skillLocator.special.cooldownRemaining - 3));

                    outer.SetNextStateToMain();
                }
            }
            
            if (fixedAge >= duration && tryRechest)
            {
                if (isAuthority)
                {
                    skillLocator.special.cooldownOverride = 1;
                    skillLocator.special.AddOneStock();

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

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(taunting);
        }
            
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            taunting = reader.ReadBoolean();
        }

        public override void ModifyNextState(EntityState nextState)
        {
            if (nextState is MimicChestInteractableIdle idle)
            {
                idle.rechest = true;
            }
        }
    }
}
