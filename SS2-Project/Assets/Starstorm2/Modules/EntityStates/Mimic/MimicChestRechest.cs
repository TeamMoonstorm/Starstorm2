using RoR2;
using RoR2.Hologram;
using SS2;
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
            SS2Log.Warning("Mimic Rechest Called");
            purchaseInter = GetComponent<PurchaseInteraction>();
            if (NetworkServer.active && purchaseInter)
            {
                purchaseInter.SetAvailable(enableInteraction);
            }
            else
            {
                SS2Log.Error("Fuck");
            }

            PlayAnimation("Gesture, Override", "BufferEmpty");
            PlayAnimation("FullBody, Override", "BufferEmpty");
            //purchaseInter.onPurchase.AddListener(delegate (Interactor interactor)
            //{
            //    OnPurchaseMimic(interactor, purchaseInter);
            //});

            //modelLocator
            //int adjust = UnityEngine.Random.Range(0, 2) - 1;
            //purchaseInter.cost += adjust;
            //characterBody.inventory.GiveItem()


        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //timer += Time.fixedDeltaTime;

            if (fixedAge >= duration && isAuthority)
            {
                ProcChainMask mask = new ProcChainMask();
                healthComponent.HealFraction(.5f, mask);

                var next = new MimicChestInteractableIdle();
                next.rechest = true;
                next.health = healthComponent.health;
                //next.target = target;
                outer.SetNextState(next); //leap begin
                SS2Log.Warning("Fixed Update End");
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            SS2Log.Warning("OnExit");
            //var anim = characterBody.modelLocator.modelTransform.GetComponent<Animator>();
            //if (anim)
            //{
            //    SS2Log.Warning("exiting InteractableIdle");
            //    //anim.SetBool("aimActive", true);
            //}
            SS2Log.Warning("Settinmg stuff");
            var body = purchaseInter.gameObject;
            body.GetComponent<BoxCollider>().enabled = true;
            body.GetComponent<CapsuleCollider>().enabled = false;

            //body.GetComponent<PingInfoProvider>().enabled = true;
            //body.GetComponent<GenericInspectInfoProvider>().enabled = true;

            body.GetComponent<GenericDisplayNameProvider>().displayToken = "SS2_MIMIC_INTERACTABLE_NAME";

            if (modelLocator && modelLocator.modelTransform)
            {
                var mdl = modelLocator.modelTransform;
                mdl.GetComponent<BoxCollider>().enabled = true;

                mdl.Find("PickupPivot").gameObject.SetActive(false);

                //var hbg = mdl.GetComponent<HurtBoxGroup>();
                //if (hbg)
                //{
                //    foreach(var hb in hbg.hurtBoxes)
                //    {
                //        Debug.Log("Disabling : ")
                //        hb.gameObject.SetActive(true);
                //    }
                //}
            }

            purchaseInter.SetAvailable(true);

            SS2Log.Warning("vfx");

            var impact = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/RoboCratePodGroundImpact.prefab").WaitForCompletion();
            EffectData effectData = new EffectData { origin = characterBody.corePosition };
            effectData.SetNetworkedObjectReference(impact);
            EffectManager.SpawnEffect(impact, effectData, transmit: true);

            PlayAnimation("Body", "IntermediateIdle");

            skillLocator.special.AddOneStock();

            GetComponent<HologramProjector>().enabled = true;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

    }
}
