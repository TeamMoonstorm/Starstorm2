using Moonstorm.Starstorm2.Items;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Components
{

    //To do : revise this
    class SoulPickup : MonoBehaviour
    {
        /* per shooty's suggestion:
         * - store drop chance on ss2item object
         * - start drop chance at 1%, increase by 1 on every kill up to 20
         * - if pickup drops item, reset drop chance to 1
         * there are flaws to this implementation that would make it unworkable. if we nerf the item it'd be better to just reduce the drop chance for now
         */

        public GameObject baseObject;
        public TeamFilter team;
        public float chance;
        public StirringSoul.Behavior Behavior;
        private bool alive = true;

        private void FixedUpdate()
        {
            if (NetworkServer.active)
                chance = Behavior.currentChance;
        }
        private void OnTriggerStay(Collider other)
        {
            if (NetworkServer.active && alive && TeamComponent.GetObjectTeam(other.gameObject) == team.teamIndex)
            {
                CharacterBody body = other.GetComponent<CharacterBody>();
                if (body)
                {
                    alive = false;

                    Util.PlaySound("StirringSoul", gameObject);

                    if (Util.CheckRoll(chance))
                    {
                        //~1% red, ~16% green
                        SS2Util.DropShipCall(transform, 1, 5);
                        Behavior.ChangeChance(true);
                    }
                    else
                    {
                        Behavior.ChangeChance(false);
                    }
                    Destroy(baseObject);
                }
            }
        }
    }
}