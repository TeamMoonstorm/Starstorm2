using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    public class ChirrFriendOrb : GenericDamageOrb
    {
        private static GameObject penis = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoDiseaseOrbEffect.prefab").WaitForCompletion();
        public ChirrFriendTracker tracker;
        public override void Begin()
        {
            base.Begin();
        }

        public override GameObject GetOrbEffect()
        {
            return penis; // >:33333333
        }

        public override void OnArrival()
        {
            base.OnArrival();

            if(this.tracker && this.tracker.friendOwnership)
            {
                this.tracker.friendOwnership.AddFriend(this.target.healthComponent.body);
                SS2Log.Info("ChirrFriendOrb.OnArrival: added fren :)");
            }

        }

    }
}
