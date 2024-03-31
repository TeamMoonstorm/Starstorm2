using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.Items;
using UnityEngine.Networking;
using System.Linq;

using Moonstorm;
namespace SS2.Items
{
    public sealed class ChucklingFungus : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("ChucklingFungus", SS2Bundle.Items);

        public static GameObject ChungusWard { get; set; } = SS2Assets.LoadAsset<GameObject>("ChungusWard", SS2Bundle.Items);

        public static float baseHealFractionPerSecond = 0.055f;

        public static float healFractionPerSecondPerStack = 0.0275f;

        public override void Initialize()
        {
            base.Initialize();
            On.RoR2.Items.ContagiousItemManager.Init += AddPair;
        }

        private void AddPair(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            List<ItemDef.Pair> newVoidPairs = new List<ItemDef.Pair>();

            ItemDef.Pair chungusPair = new ItemDef.Pair()
            {
                itemDef1 = SS2Content.Items.DormantFungus,
                itemDef2 = ItemDef
            };
            newVoidPairs.Add(chungusPair);

            var voidPairs = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem];
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = voidPairs.Union(newVoidPairs).ToArray();

            orig();
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

                        wardInstance = Instantiate(ChungusWard, position, Quaternion.identity);
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
