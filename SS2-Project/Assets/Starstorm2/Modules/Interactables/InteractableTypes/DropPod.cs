﻿using EntityStates;
using SS2.Components;
using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Interactables
{
#if DEBUG
    //This fucking thing sucks ass and unironically is awful, i hate and will forever hate having to clone prefabs.
    //Not to mention, using the default survivor pod doesnt really make sense since these are supposed to be leftover pods from contact light, so again, this shit should just be re-written and homemade instead of cloned.
    // - Nebby -
    /*public sealed class DropPod : SS2Interactable
    {
        public override GameObject Interactable { get => interactable; }

        private static GameObject interactable;

        public override List<MSInteractableDirectorCard> InteractableDirectorCards => new List<MSInteractableDirectorCard>()
        {
            SS2Assets.LoadAsset<MSInteractableDirectorCard>("msidcDropPod", SS2Bundle.Indev)
        };

        public override void Initialize()
        {
            //DirectorAPI.MonsterActions += HandleAvailableMonsters;
            
            var survivorPod = Resources.Load<GameObject>("prefabs/networkedobjects/SurvivorPod");
            interactable = PrefabAPI.InstantiateClone(survivorPod, "DropPod", false);

            Interactable.transform.position = Vector3.zero;

            DestroyUneededObjectsAndComponents();
            EnableObjects();
            ModifyExistingComponents();
            AddNewComponents();

            HG.ArrayUtils.ArrayAppend(ref SS2Content.Instance.SerializableContentPack.networkedObjectPrefabs, Interactable);
            InteractableDirectorCards[0].prefab = Interactable;
            InteractableDirectorCards[0].maxSpawnsPerStage = 2;

            DirectorAPI.MonsterActions += HandleAvailableMonsters2;

            SS2Log.Info("Finished setting up Drop Pod.");
        }

        private void HandleAvailableMonsters2(DccsPool arg1, List<DirectorAPI.DirectorCardHolder> arg2, DirectorAPI.StageInfo arg3) //i mean maybe this works lol
        {
            DropPodController.currentStageMonsters = arg2.Where(cardHolder => cardHolder.MonsterCategory == DirectorAPI.MonsterCategory.BasicMonsters)
                                                         .Select(cardHolder => cardHolder.Card)
                                                         .ToArray();
            //throw new System.NotImplementedException();
        }

        private void HandleAvailableMonsters(List<DirectorAPI.DirectorCardHolder> arg1, DirectorAPI.StageInfo arg2)
        {
            DropPodController.currentStageMonsters = arg1.Where(cardHolder => cardHolder.MonsterCategory == DirectorAPI.MonsterCategory.BasicMonsters)
                                                         .Select(cardHolder => cardHolder.Card)
                                                         .ToArray();
        }

        private void DestroyUneededObjectsAndComponents()
        {
            Object.Destroy(Interactable.GetComponent<SurvivorPodController>());
            Object.Destroy(Interactable.GetComponent<VehicleSeat>());
            //Interactable.GetComponents<InstantiatePrefabOnStart>().ToList().ForEach(component => Object.Destroy(component));
            Object.Destroy(Interactable.GetComponent<BuffPassengerWhileSeated>());

            var flames = Interactable.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base/Flames");
            Object.Destroy(flames.gameObject);

            var donut = Interactable.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base/FireDonut");
            Object.Destroy(donut.gameObject);

            /*var mdl = Interactable.transform.Find("Base/mdlEscapePod");
            Object.Destroy(mdl.GetComponent<Animator>());
        }

        private void EnableObjects()
        {
            var debris = Interactable.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base/DebrisParent");
            debris.gameObject.SetActive(true);
        }

        private void ModifyExistingComponents()
        {
            var stateMachine = Interactable.GetComponent<EntityStateMachine>();
            stateMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.DropPod.Idle));
            stateMachine.mainStateType = new SerializableEntityStateType(typeof(Idle));

            var modelLocator = Interactable.GetComponent<ModelLocator>();
            modelLocator.enabled = true;
            modelLocator.modelBaseTransform = null;

            var exitPos = Interactable.transform.Find("Base/mdlEscapePod/EscapePodArmature/ExitPosition");
            exitPos.transform.localPosition = new Vector3(0, -2, 0);
            var cLocator = modelLocator.modelTransform.GetComponent<ChildLocator>();
            HG.ArrayUtils.ArrayAppend(ref cLocator.transformPairs, new ChildLocator.NameTransformPair { name = "ExitPos", transform = exitPos });
        }

        private void AddNewComponents()
        {
            var networkTransform = Interactable.AddComponent<NetworkTransform>();
            networkTransform.transformSyncMode = NetworkTransform.TransformSyncMode.SyncTransform;
            Interactable.AddComponent<GenericDisplayNameProvider>().displayToken = "SS2_INTERACTABLE_DROPPOD_NAME";
            Interactable.AddComponent<DropPodController>();

            PaintDetailsBelow details = Interactable.AddComponent<PaintDetailsBelow>();
            details.influenceOuter = 2;
            details.influenceInner = 1;
            details.layer = 0;
            details.density = 0.5f;
            details.densityPower = 3;

            details = Interactable.AddComponent<PaintDetailsBelow>();
            details.influenceOuter = 2;
            details.influenceInner = 1;
            details.layer = 1;
            details.density = 0.3f;
            details.densityPower = 3;

            var podMesh = Interactable.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base").gameObject;
            podMesh.AddComponent<EntityLocator>().entity = Interactable;
        }
    }*/
#endif
}
