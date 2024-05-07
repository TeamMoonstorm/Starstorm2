using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.Items;
using UnityEngine.Networking;
using System.Linq;
using MSU;
using RoR2.ContentManagement;
using System.Collections;

namespace SS2.Items
{
    //N: This is so fucking epic
    public sealed class ChucklingFungus : SS2VoidItem
    {
        public override SS2AssetRequest<ItemAssetCollection> AssetRequest<ItemAssetCollection>()
        {
            return SS2Assets.LoadAssetAsync<ItemAssetCollection>("acChucklingFungus", SS2Bundle.Items);
        }
        public override void OnAssetCollectionLoaded(AssetCollection assetCollection)
        {
            _chungusWard = assetCollection.FindAsset<GameObject>("ChungusWard");
        }
        private static GameObject _chungusWard;

        public static float baseHealFractionPerSecond = 0.055f;

        public static float healFractionPerSecondPerStack = 0.0275f;

        public override List<ItemDef> GetInfectableItems()
        {
            return new List<ItemDef>
            {
                SS2Content.Items.DormantFungus
            };
        }

        public override void Initialize()
        {
        }

        //Should return true only if dungus is available as well, unsure how to do that lol
        public override bool IsAvailable(ContentPack contentPack)
        {
            return contentPack.itemDefs.Find("DormantFungus"); // ^like this
        }


        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ChucklingFungus;
            private bool active = false;
            private GameObject wardInstance;
            private HealingWard healingWard;
            private TeamFilter teamFilter;

            public void FixedUpdate()
            {
                if (!NetworkServer.active) return;
                if (body.notMovingStopwatch > 1f)
                {
                    if (!active)
                    {
                        Vector3 position = body.corePosition;

                        if (wardInstance != null)
                            NetworkServer.Destroy(wardInstance);

                        float networkradius = body.radius + 1.5f + 1.5f * stack;

                        wardInstance = Instantiate(_chungusWard, position, Quaternion.identity);
                        teamFilter = wardInstance.GetComponent<TeamFilter>();
                        healingWard = wardInstance.GetComponent<HealingWard>();
                        NetworkServer.Spawn(wardInstance);

                        if (healingWard != null)
                        {
                            healingWard.interval = 0.25f;
                            healingWard.healFraction = (baseHealFractionPerSecond + healFractionPerSecondPerStack * (stack - 1)) * healingWard.interval;
                            healingWard.healPoints = 0f;
                            healingWard.Networkradius = networkradius;
                        }

                        if (teamFilter != null)
                            teamFilter.teamIndex = body.teamComponent.teamIndex;

                        active = true;
                    }
                }
                else
                {
                    active = false;

                    if (wardInstance != null)
                        Destroy(wardInstance);
                }
            }

            public void OnDestroy()
            {
                if (wardInstance != null)
                    Destroy(wardInstance);
            }

            public void OnDisable()
            {
                if (wardInstance != null)
                    Destroy(wardInstance);
            }
        }
    }
}
