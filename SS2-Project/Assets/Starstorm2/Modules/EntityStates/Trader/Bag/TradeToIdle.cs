using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using SS2;
using SS2.Components;
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

        private static float dropDuration = 1f;
        private static float dropInterval = 0.33f;
        private float stopwatch;
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
            stopwatch -= Time.fixedDeltaTime;
            if(stopwatch <= 0 && fixedAge < dropDuration)
            {
                stopwatch += dropInterval;
                if(traderController.pendingRewards.Count > 0)
                {
                    TraderController.Reward drop = traderController.pendingRewards.Dequeue();
                    if (drop.drop != PickupIndex.none)
                    {
                        Transform transform = FindModelChild(muzzleString);
                        Vector3 target = transform.forward;
                        if (drop.target) Vector3.RotateTowards(target, drop.target.transform.position, 60f, 0.33f);
                        target.y = 0;
                        PickupDropletController.CreatePickupDroplet(drop.drop, transform.position, Vector3.up * dropUpVelocityStrength + target * dropForwardVelocityStrength);
                    }
                }
            }
            if (fixedAge > duration)
                outer.SetNextState(new Idle());
        }
    }
}
