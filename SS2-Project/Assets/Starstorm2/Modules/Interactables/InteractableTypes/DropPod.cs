using EntityStates;
using SS2.Components;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using MSU;
using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using SS2.Items;
using RoR2.Navigation;
using HG;

namespace SS2.Interactables
{
    public sealed class DropPod : SS2Interactable
    {
        public override SS2AssetRequest<InteractableAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<InteractableAssetCollection>("acDropPod", SS2Bundle.Interactables);

        public static GameObject interactablePrefab;

        
        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta;
        }
        public override void Initialize()
        {
            var survivorPod = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPod.prefab").WaitForCompletion();
            interactablePrefab = PrefabAPI.InstantiateClone(survivorPod, "DropPod", false);

            interactablePrefab.transform.position = Vector3.zero;

            var spawnCard = SS2Assets.LoadAsset<SpawnCard>("iscDropPod", SS2Bundle.Interactables);
            spawnCard.prefab = interactablePrefab;

            DestroyUneededObjectsAndComponents();
            EnableObjects();
            ModifyExistingComponents();
            AddNewComponents();

            SS2Content.SS2ContentPack.networkedObjectPrefabs.AddSingle(interactablePrefab);

            IL.RoR2.SceneDirector.PopulateScene += SceneDirector_PopulateScene;
            SceneDirector.onGenerateInteractableCardSelection += SceneDirector_onGenerateInteractableCardSelection;
        }


        public static bool higherStage1Weight = true;
        private static int stage1Weight = 6;
        private static int stage1MaxSpawns = 3;
        private static int standardWeight = 2;
        private static int standardMaxSpawns = -1;
        // spawn more early, spawn less later
        // the dccs arg is a duplicate that gets deleted after, so we can modify it safely.
        private void SceneDirector_onGenerateInteractableCardSelection(SceneDirector sceneDirector, DirectorCardCategorySelection dccs)
        {
            if (!higherStage1Weight)
            {
                return;
            }

            for (int i = 0; i < dccs.categories.Length; i++)
            {
                ref DirectorCardCategorySelection.Category category = ref dccs.categories[i];
                for (int j = category.cards.Length - 1; j >= 0; j--)
                {
                    DirectorCard directorCard = category.cards[j];
                    var spawnCard = directorCard.GetSpawnCard();
                    if (spawnCard.prefab == interactablePrefab)
                    {
                        if (Run.instance && Run.instance.stageClearCount == 0)
                        {
                            directorCard.selectionWeight = stage1Weight;
                            (spawnCard as InteractableSpawnCard).maxSpawnsPerStage = stage1MaxSpawns;
                        }
                        else
                        {
                            directorCard.selectionWeight = standardWeight;
                            (spawnCard as InteractableSpawnCard).maxSpawnsPerStage = standardMaxSpawns;
                        }
                    }
                }
            }
        }

        // instead of spawning randomly, drop pods will try to pick a player spawn location
        // if no spawnpoints found, default to normal random spawning
        private void SceneDirector_PopulateScene(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            // directly before DirectorCore.instance.TrySpawnObject
            // if the selected itneractable is a DropPod, replace the directorReplacementRule with our own
            int directorPlacementRuleVarIndex = -1;
            int directorCardVarIndex = -1;

            bool a = c.TryGotoNext(MoveType.After,
                x => x.MatchCall<SceneDirector>(nameof(SceneDirector.SelectCard)),
                x => x.MatchStloc(out directorCardVarIndex));
            if (!a)
            {
                SS2Log.Fatal("DropPod.SceneDirector_PopulateScene: ILHook failed 1.");
                return;
            }

            bool b = c.TryGotoNext(MoveType.After,
                x => x.MatchStfld<DirectorPlacementRule>(nameof(DirectorPlacementRule.placementMode)),
                x => x.MatchStloc(out directorPlacementRuleVarIndex));
            if (b)
            {
                c.Emit(OpCodes.Ldloc, directorPlacementRuleVarIndex);
                c.Emit(OpCodes.Ldloc, directorCardVarIndex);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<DirectorPlacementRule, DirectorCard, SceneDirector>>((directorPlacementRule, directorCard, sceneDirector) =>
                {
                    var spawnCard = directorCard.GetSpawnCard();

                    if (spawnCard.prefab == interactablePrefab)
                    {
                        SS2Log.Info("Placing DropPod!");
                        bool foundSpawnPoint = TryGetDropPodSpawnPoint(sceneDirector, out Vector3 spawnPosition, out Quaternion spawnRotation);
                        if (foundSpawnPoint)
                        {
                            SS2Log.Info("Found SpawnPoint at " + spawnPosition);
                            directorPlacementRule.placementMode = DirectorPlacementRule.PlacementMode.Direct;
                            directorPlacementRule.position = spawnPosition;
                            directorPlacementRule.rotation = spawnRotation;
                        }
                    }
                });
            }
            else
            {
                SS2Log.Fatal("DropPod.SceneDirector_PopulateScene: ILHook failed 2.");
            }
        }

        // if a stage has preplaced spawnpoints (aka survivor pod locations), use any of them EXCEPT the furthest away ones (which the stage uses for player spawns)
        // consume or destroy the spawnpoints we do use for droppods. destroy is probably safer
        private bool TryGetDropPodSpawnPoint(SceneDirector sceneDirector, out Vector3 position, out Quaternion rotation)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;

            List<SpawnPoint> list = new List<SpawnPoint>(SpawnPoint.readOnlyInstancesList);
            if (sceneDirector.teleporterInstance)
            {
                // inverse logic of SceneDirector spawnpoint usage
                // get a list of SpawnPoints the stage WONT use for player spawns.
                Vector3 teleporterPosition = sceneDirector.teleporterInstance.transform.position;
                list.Sort((SpawnPoint a, SpawnPoint b) => (teleporterPosition - a.transform.position).sqrMagnitude.CompareTo((teleporterPosition - b.transform.position).sqrMagnitude));
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (i > list.Count - RoR2Application.maxPlayers)
                    {
                        list.RemoveAt(i);
                    }
                }
            }

            SS2Log.Info("DropPod spawnpoint count: " + list.Count);

            if (list.Count > 0)
            {
                var spawnPoint = sceneDirector.rng.NextElementUniform(list);
                position = spawnPoint.transform.position;
                rotation = spawnPoint.transform.rotation;
                GameObject.Destroy(spawnPoint.gameObject);
                return true;
            }

            return false;
        }

        private void DestroyUneededObjectsAndComponents()
        {
            GameObject.Destroy(interactablePrefab.GetComponent<SurvivorPodController>());
            GameObject.Destroy(interactablePrefab.GetComponent<VehicleSeat>());
            GameObject.Destroy(interactablePrefab.GetComponent<BuffPassengerWhileSeated>());

            var components = interactablePrefab.GetComponents<InstantiatePrefabBehavior>();
            for (int i = components.Length - 1; i >= 0; i--)
            {
                components[i].enabled = false;
            }

            var flames = interactablePrefab.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base/Flames");
            GameObject.Destroy(flames.gameObject);

            var donut = interactablePrefab.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base/FireDonut");
            GameObject.Destroy(donut.gameObject);

            var mdl = interactablePrefab.transform.Find("Base/mdlEscapePod");
            // the default spawn anim plays an endless loop sound with AnimationEvents. deleting this will make DropPod spit out a warning on spawn but its fine
            // the alternative is unity animator fuckery
            GameObject.Destroy(mdl.GetComponent<AnimationEvents>()); 
        }

        private void EnableObjects()
        {
            var debris = interactablePrefab.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base/DebrisParent");
            debris.transform.localPosition = new Vector3(debris.transform.localPosition.x, 0, debris.transform.localPosition.z);
            debris.gameObject.SetActive(true);
        }

        private void ModifyExistingComponents()
        {
            var stateMachine = interactablePrefab.GetComponent<EntityStateMachine>();
            stateMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.DropPod.Idle));
            stateMachine.mainStateType = new SerializableEntityStateType(typeof(Idle));

            var modelLocator = interactablePrefab.GetComponent<ModelLocator>();
            modelLocator.enabled = true;
            modelLocator.modelBaseTransform = null;

            var exitPos = interactablePrefab.transform.Find("Base/mdlEscapePod/EscapePodArmature/ExitPosition");
            exitPos.transform.localPosition = new Vector3(0, -2.33f, -0.6f);
            var cLocator = modelLocator.modelTransform.GetComponent<ChildLocator>();
            HG.ArrayUtils.ArrayAppend(ref cLocator.transformPairs, new ChildLocator.NameTransformPair { name = "ExitPos", transform = exitPos });
        }

        private void AddNewComponents()
        {
            var networkTransform = interactablePrefab.AddComponent<NetworkTransform>();
            networkTransform.transformSyncMode = NetworkTransform.TransformSyncMode.SyncTransform;
            interactablePrefab.AddComponent<GenericDisplayNameProvider>().displayToken = "SS2_INTERACTABLE_DROPPOD_NAME";

            var dropPodController = interactablePrefab.AddComponent<DropPodController>();
            dropPodController.dropTransform = interactablePrefab.transform.Find("Base/mdlEscapePod/EscapePodArmature/ExitPosition");
            dropPodController.itemDropTable = SS2Assets.LoadAsset<BasicPickupDropTable>("dtDropPod", SS2Bundle.Interactables);

            PaintDetailsBelow details = interactablePrefab.AddComponent<PaintDetailsBelow>();
            details.influenceOuter = 2;
            details.influenceInner = 1;
            details.layer = 0;
            details.density = 0.5f;
            details.densityPower = 3;

            details = interactablePrefab.AddComponent<PaintDetailsBelow>();
            details.influenceOuter = 2;
            details.influenceInner = 1;
            details.layer = 1;
            details.density = 0.3f;
            details.densityPower = 3;

            var podMesh = interactablePrefab.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base").gameObject;
            podMesh.AddComponent<EntityLocator>().entity = interactablePrefab;
        }
    }
}
