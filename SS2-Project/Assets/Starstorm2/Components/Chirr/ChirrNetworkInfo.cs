using RoR2;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Components
{
    // I'd put this in ChirrController but it (probably) needs to be in a NetworkBehaviour.
    public class ChirrNetworkInfo : NetworkBehaviour
    {
        public HurtBox futureFriend;
        public CharacterBody friend;
        public TeamIndex friendOrigIndex;
        public bool hasFriend;

        public Inventory baseInventory;
        public CharacterBody pingTarget;
        public CharacterBody enemyTarget;
    }
}
