using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RoR2;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace SS2
{
    public class BuriedTreasureBehavior : MonoBehaviour
    {
        [ConCommand(commandName = "treasure_debug", flags = (ConVarFlags.Cheat), helpText = "Toggle Buried Treasure debugging.")]
        private static void CCToggleDebug(ConCommandArgs args)
        {
            debug = !debug;
            Debug.Log("Buried Treasure debugging is now " + (debug ? "ON" : "OFF"));
        }

        private static bool debug;

        public float treasureSpawnMinDistance = 4f;
        public float treasureSpawnMaxDistance = 9f;
        public GameObject treasureSpawnEffectPrefab;
        public SpawnCard treasureSpawnCard;
        public EntityStates.SerializableEntityStateType treasureOpenState;
        public PickupDropTable dropTable;
        
        public float radius = 50f;
        public Transform debugIndicator;


        private Xoroshiro128Plus rng;
        private bool active = true;

        private void OnEnable()
        {
            InstanceTracker.Add(this);
        }
        private void OnDisable()
        {
            InstanceTracker.Remove(this);
        }

        private void Start()
        {
            if (NetworkServer.active)
            {
                rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
            }
        }

        private void FixedUpdate()
        {
            if (debugIndicator)
            {
                debugIndicator.localScale = Vector3.one * radius;
                debugIndicator.gameObject.SetActive(debug && active);
            }
        }

        public bool IsPointInRadius(Vector3 position)
        {
            Vector3 between = transform.position - position;
            return between.sqrMagnitude <= (radius * radius);
        }

        public void SpawnTreasure(Vector3 position)
        {
            if (Run.instance)
            {
                int i = 0;
                while (i < 10)
                {
                    DirectorPlacementRule placementRule = new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                        position = position,
                        minDistance = treasureSpawnMinDistance,
                        maxDistance = treasureSpawnMaxDistance,
                    };
                    GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(treasureSpawnCard, placementRule, this.rng));
                    if (gameObject)
                    {
                        if (treasureSpawnEffectPrefab)
                        {
                            EffectManager.SimpleEffect(treasureSpawnEffectPrefab, gameObject.transform.position, Quaternion.identity, true);
                        }

                        PurchaseInteraction component = gameObject.GetComponent<PurchaseInteraction>();
                        if (component && component.costType == CostTypeIndex.Money)
                        {
                            component.Networkcost = Run.instance.GetDifficultyScaledCost(component.cost);
                        }

                        ChestBehavior chestBehavior = gameObject.GetComponent<ChestBehavior>();
                        if (chestBehavior)
                        {
                            chestBehavior.dropTable = dropTable; // i think this makes the drop non-deterministic
                            chestBehavior.openState = treasureOpenState;
                        }

                        SfxLocator sfxLocator = gameObject.GetComponent<SfxLocator>();
                        if (sfxLocator)
                        {
                            sfxLocator.openSound = "Play_UI_buriedTreasure_unlock";
                        }

                        RandomizeSplatBias[] splats = gameObject.GetComponentsInChildren<RandomizeSplatBias>();
                        foreach (RandomizeSplatBias splat in splats)
                        {
                            splat.minGreenBias = 0.4f;
                            splat.maxGreenBias = 0.6f;
                        }
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }
    }
}
