using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using SS2;

namespace EntityStates.Trader.Bag
{
    public class TradeToIdle : BagBaseState
    {
        public static string enterSoundString;
        public static string exitSoundString;

        public static float duration;

        public static float dropUpVelocityStrength;
        public static float dropForwardVelocityStrength;

        public static GameObject muzzleFlashEffectPrefab;
        public static string muzzleString;

        protected override bool enableInteraction
        {
            get
            {
                return false;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(enterSoundString, gameObject);
            //sfx, vfx, etc.

            if (!NetworkServer.active)
                return;

            PickupIndex drop = traderController.nextReward;
            SS2Log.Info("zan giving: " + drop.GetPickupNameToken());

            if (drop != PickupIndex.none)
            {
                Transform transform = FindModelChild(muzzleString);
                PickupDropletController.CreatePickupDroplet(drop, transform.position, Vector3.up * dropUpVelocityStrength + transform.forward * dropForwardVelocityStrength);
            }
            
        }

        public override void OnExit()
        {
            //sfx, vfx, etc.
            Util.PlaySound(exitSoundString, gameObject);     
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge > duration)
                outer.SetNextState(new Idle());
        }
    }
}
