using EntityStates;
using Moonstorm.Starstorm2.Components;
using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Moonstorm.Starstorm2.Interactables
{
    public sealed class DroneTable : InteractableBase
    {
        public override GameObject Interactable { get; } = SS2Assets.LoadAsset<GameObject>("DroneTablePrefab", SS2Bundle.Indev);

        private static GameObject interactable;

        //private static List<InteractableSpawnCard> interactableSpawnCards = new List<InteractableSpawnCard>();
        //private static List<CharacterBody> characterBodies = new List<CharacterBody>();
        //
        //private static List<BodyIndex> illegalMarks = new List<BodyIndex>();


        /// <summary>
        /// List of drone and interactable cost pairs. String is the body.name, int is the price a player pays, not the director.
        /// </summary>
        public static Dictionary<string, int> dronePairs = new Dictionary<string, int>();

        public static CostTypeDef droneCostDef;
        public static int droneCostIndex;

        public override MSInteractableDirectorCard InteractableDirectorCard { get; } = SS2Assets.LoadAsset<MSInteractableDirectorCard>("midcDroneTable");

        public override void Initialize()
        {
            CostTypeCatalog.modHelper.getAdditionalEntries += addDroneCostType;
            //On.RoR2.PurchaseInteraction.GetDisplayName += MonolithName;
            interactable = InteractableDirectorCard.prefab;

            var interactionToken = interactable.AddComponent<RefabricatorInteractionToken>();
            interactionToken.PurchaseInteraction = interactable.GetComponent<PurchaseInteraction>();
            interactionToken.symbolTransform = null;

            //list = StringFinder.Instance.InteractableSpawnCards;
            //getInteractableCards();

            SetupDroneValueList();
        }

        private void SetupDroneValueList()
        {
            dronePairs.Add("Turret1Body", 35);
            dronePairs.Add("Drone1Body", 40); //gunner
            dronePairs.Add("Drone2Body", 40); //healing
            dronePairs.Add("MissileDroneBody", 60);
            dronePairs.Add("EquipmentDroneBody", 60); //takes equipment so uhhhh i dunno
            dronePairs.Add("EmergencyDroneBody", 100);
            dronePairs.Add("FlameDroneBody", 100);
            dronePairs.Add("MegaDroneBody", 350);

            dronePairs.Add("ShockDroneBody", 40);
            
            //dronePairs.Add("", 15);
            //dronePairs.Add("", 15);

            //foreach (string bodyName in defaultBodyNames)
            //{
            //    BodyIndex index = BodyCatalog.FindBodyIndexCaseInsensitive(bodyName);
            //    if (index != BodyIndex.None)
            //    {
            //        AddBodyToIllegalTerminationList(index);
            //    }
            //}
        }

        //private void getInteractableCards() //
        //{
        //    GatherAddressableAssets<InteractableSpawnCard>("/isc", (asset) => interactableSpawnCards.Add(asset));
        //    On.RoR2.ClassicStageInfo.Start += AddCurrentStageIscsToCache;
        //    //On.RoR2.ClassicStageInfo.Start += GetValidBodies;
        //}
        //
        //private void GetValidBodies(On.RoR2.ClassicStageInfo.orig_Start orig, ClassicStageInfo self)
        //{
        //    orig(self);
        //    GameObject[] list = BodyCatalog.allBodyPrefabs.ToArray();
        //    foreach(var body in list)
        //    {
        //        var cbody = body.GetComponent<CharacterBody>();
        //        if((cbody.bodyFlags & CharacterBody.BodyFlags.Mechanical) > CharacterBody.BodyFlags.None){
        //            characterBodies.Add(cbody);
        //        }
        //    }
        //}
        //
        //private static void GatherAddressableAssets<T>(string filterKey, Action<T> onAssetLoaded)
        //{
        //    RoR2Application.onLoad += () =>
        //    {
        //        foreach (var resourceLocator in Addressables.ResourceLocators)
        //        {
        //            foreach (var key in resourceLocator.Keys)
        //            {
        //                var keyString = key.ToString();
        //                if (keyString.Contains(filterKey))
        //                {
        //                    var iscLoadRequest = Addressables.LoadAssetAsync<T>(keyString);
        //
        //                    iscLoadRequest.Completed += (completedAsyncOperation) =>
        //                    {
        //                        if (completedAsyncOperation.Status == AsyncOperationStatus.Succeeded)
        //                        {
        //                            onAssetLoaded(completedAsyncOperation.Result);
        //                        }
        //                    };
        //                }
        //            }
        //        }
        //    };
        //}
        //
        //// There is no real good way to query for all custom iscs afaik
        //// So let's lazily add them as the player encounter stages
        //private void AddCurrentStageIscsToCache(On.RoR2.ClassicStageInfo.orig_Start orig, ClassicStageInfo self)
        //{
        //    orig(self);
        //    var iscsOfCurrentStage =
        //        self.interactableCategories.categories.
        //        SelectMany(category => category.cards).
        //        Select(directorCard => directorCard.spawnCard).
        //        Where(spawnCard => interactableSpawnCards.All(existingIsc => existingIsc.name != spawnCard.name)).
        //        Cast<InteractableSpawnCard>();
        //    interactableSpawnCards.AddRange(iscsOfCurrentStage);
        //
        //
        //    GameObject[] list = BodyCatalog.allBodyPrefabs.ToArray();
        //    foreach (var body in list)
        //    {
        //        var cbody = body.GetComponent<CharacterBody>();
        //        if ((cbody.bodyFlags & CharacterBody.BodyFlags.Mechanical) > CharacterBody.BodyFlags.None)
        //        {
        //            characterBodies.Add(cbody);
        //        }
        //    }
        //
        //    foreach(var card in interactableSpawnCards)
        //    {
        //        var summon = card.prefab.GetComponent<SummonMasterBehavior>();
        //        if (summon)
        //        {
        //            SS2Log.Info("summon master for " + card.name + " is " + summon.masterPrefab);
        //            foreach(var body in characterBodies)
        //            {
        //                if(body.master.name == "")
        //                {
        //
        //                }
        //            }
        //        }
        //    }
        //
        //}

        private void addDroneCostType(List<CostTypeDef> obj)
        {
            //CostTypeIndex voidItem = new CostTypeIndex();
            droneCostDef = new CostTypeDef();
            droneCostDef.costStringFormatToken = "SS2_COST_DRONE_FORMAT";
            droneCostDef.isAffordable = new CostTypeDef.IsAffordableDelegate(DroneCostTypeHelper.IsAffordable);
            droneCostDef.payCost = new CostTypeDef.PayCostDelegate(DroneCostTypeHelper.PayCost);
            droneCostDef.colorIndex = ColorCatalog.ColorIndex.Interactable;
            droneCostDef.saturateWorldStyledCostString = true;
            droneCostDef.darkenWorldStyledCostString = false;
            droneCostIndex = CostTypeCatalog.costTypeDefs.Length + obj.Count;
            //LanguageAPI.Add("VV_COST_VOIDITEM_FORMAT", "1 Item(s)");
            obj.Add(droneCostDef);
        }

        private static class DroneCostTypeHelper
        {
            public static bool IsAffordable(CostTypeDef costTypeDef, CostTypeDef.IsAffordableContext context)
            {
                CharacterBody body = context.activator.GetComponent<CharacterBody>();
                if (!body)
                {
                    return false;
                }
                Inventory inventory = body.inventory;
                if (!inventory)
                {
                    return false;
                }
                int cost = context.cost;
                //int num = 0;

                //MinionOwnership.MinionGroup minionGroup;

                if ((body != null) ? body.master : null)
                {
                    MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(body.master.netId);
                    if (minionGroup != null)
                    {
                        var members = minionGroup.members;
                        List<CharacterMaster> validMinions = new List<CharacterMaster>();
                        foreach (var drone in members)
                        {
                            //SS2Log.Info("hi | " + members.Length);
                            if (drone)
                            {
                                CharacterMaster master = drone.GetComponent<CharacterMaster>();
                                if (master)
                                {
                                    var droneBody = master.GetBody();
                                    
                                    //SS2Log.Info("flags: " + master.miscFlags + " | ind: " + master.masterIndex + " | body flags " + droneBody.bodyFlags + " | " + droneBody);
                                    if((droneBody.bodyFlags & CharacterBody.BodyFlags.Mechanical) > CharacterBody.BodyFlags.None)
                                    {
                                        foreach(var pair in dronePairs)
                                        {
                                            //SS2Log.Info("testing " + pair.Key + " || " + droneBody.bodyIndex + " | " + BodyCatalog.FindBodyIndex(pair.Key));
                                            if (droneBody.bodyIndex == BodyCatalog.FindBodyIndex(pair.Key))
                                            {
                                                validMinions.Add(master);
                                                //SS2Log.Info("Added " + pair.Key + " | " + droneBody.name);
                                                break;
                                            }
                                        }
                                        //SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/" + text)
                                        //validMinions.Add(master);
                                    }
                                    //validMinions.Add(master);
                                }
                                //SS2Log.Info("flags: " + master.miscFlags + " | ind: " + master.masterIndex + " | " + master);
                            }
                            if (validMinions.Count >= cost)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                            //CharacterMaster master = drone.GetComponent<CharacterMaster>();
                            //SS2Log.Info("flags: " + master.miscFlags + " | ind: " + master.masterIndex + " | " + master);

                        }
                    }
                }
                return false;
            }

            public static void PayCost(CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
            {

                //SS2Log.Debug("all bodies: " + BodyCatalog.allBodyPrefabs.ToString());
                GameObject[] list = BodyCatalog.allBodyPrefabs.ToArray();
                int a = 0;
                foreach(GameObject obj in list)
                {
                    var body1 = obj.GetComponent<CharacterBody>();
                    if (body1)
                    {
                        //SS2Log.Info("body " + a + " name token: " + body1.baseNameToken + " | " + body1.name);
                    }
                    //SS2Log.Debug(a + " - obj name: " + obj.name);
                    ++a;
                }

                CharacterBody body = context.activator.GetComponent<CharacterBody>();
                int cost = context.cost;

                MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(body.master.netId);
                if (minionGroup != null)
                {
                    var members = minionGroup.members;
                    List<CharacterMaster> validMinions = new List<CharacterMaster>();
                    foreach (var drone in members)
                    {
                        //SS2Log.Info("hi | " + members.Length);
                        if (drone)
                        {
                            CharacterMaster master = drone.GetComponent<CharacterMaster>();
                            if (master)
                            {
                                var droneBody = master.GetBody();

                                //SS2Log.Info("flags: " + master.miscFlags + " | ind: " + master.masterIndex + " | body flags " + droneBody.bodyFlags + " | " + droneBody.baseNameToken);
                                if ((droneBody.bodyFlags & CharacterBody.BodyFlags.Mechanical) > CharacterBody.BodyFlags.None)
                                {
                                    foreach (var pair in dronePairs)
                                    {
                                        //SS2Log.Info("testing " + pair.Key);
                                        if (droneBody.bodyIndex == BodyCatalog.FindBodyIndex(pair.Key))
                                        {
                                            validMinions.Add(master);
                                        }
                                    }
                                }
                            }
                            //SS2Log.Info("flags: " + master.miscFlags + " | ind: " + master.masterIndex + " | " + master);
                        }
                        //CharacterMaster master = drone.GetComponent<CharacterMaster>();
                        //SS2Log.Info("flags: " + master.miscFlags + " | ind: " + master.masterIndex + " | " + master);

                    }

                    Util.ShuffleList(validMinions, context.rng);

                    for (int k = 0; k < cost; k++)
                    {
                        TakeOne();
                    }
                    MultiShopCardUtils.OnNonMoneyPurchase(context);
                    void TakeOne()//Inventory inventory, CostTypeDef.PayCostContext context, int cost)
                    {
                        for (int i = 0; i < validMinions.Count(); i++)
                        {
                            if (validMinions[i])
                            {
                                foreach (var pair in dronePairs)
                                {
                                    //SS2Log.Info("testing " + pair.Key);
                                    if (validMinions[i].GetBody().bodyIndex == BodyCatalog.FindBodyIndex(pair.Key))
                                    {
                                        SS2Log.Info("IOU one item with value modifier " + pair.Value);
                                    }
                                }

                                validMinions[i].GetBody().healthComponent.Suicide();
                                //SS2Log.Info("IOU one item ");
                                break;

                            }
                        }
                    }
                }
            }
        }

        public class RefabricatorInteractionToken : NetworkBehaviour
        {
            //public CharacterBody Owner;
            public CharacterBody LastActivator;
            //public Transform selfpos;
            public PurchaseInteraction PurchaseInteraction;
            public Transform symbolTransform;

            public void Start()
            {
                if (NetworkServer.active && Run.instance)
                {
                    PurchaseInteraction.SetAvailableTrue();
                }
                PurchaseInteraction.costType = (CostTypeIndex)droneCostIndex;
                PurchaseInteraction.onPurchase.AddListener(DronePurchaseAttempt);

                //InteractableBodyModelPrefab.transform.Find("Symbol");
                //BuffBrazierStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Body");

                //ConstructFlameChoice();
                //BaseCostDetermination = (int)(PurchaseInteraction.cost * ChosenBuffBrazierBuff.CostModifier);
                //SetCost();

            }
            public void DronePurchaseAttempt(Interactor interactor)
            {
                if (!interactor) { return; }

                var body = interactor.GetComponent<CharacterBody>();
                if (body && body.master)
                {
                    if (NetworkServer.active)
                    {
                        SS2Log.Info("Purchase Successful");
                        //AttemptSpawnVoidPortal();
                        GameObject effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ShrineUseEffect.prefab").WaitForCompletion();
                        EffectManager.SimpleImpactEffect(effectPrefab, this.transform.position, new Vector3(0, 0, 0), true);

                        //GameObject portal = UnityEngine.Object.Instantiate<GameObject>(ShatteredMonolith.voidFieldPortalObject, this.transform.position, new Quaternion(0, 70, 0, 0));
                        //NetworkServer.Spawn(portal);
                        LastActivator = body;

                        //symbolTransform.gameObject.SetActive(false);
                        PurchaseInteraction.SetAvailable(false);
                    }
                }
            }
        }

        //public override void Initialize()
        //{
        //    //DirectorAPI.MonsterActions += HandleAvailableMonsters;
        //
        //    var survivorPod = Resources.Load<GameObject>("prefabs/networkedobjects/SurvivorPod");
        //    interactable = PrefabAPI.InstantiateClone(survivorPod, "DropPod", false);
        //
        //    Interactable.transform.position = Vector3.zero;
        //
        //    DestroyUneededObjectsAndComponents();
        //    EnableObjects();
        //    ModifyExistingComponents();
        //    AddNewComponents();
        //
        //    HG.ArrayUtils.ArrayAppend(ref SS2Content.Instance.SerializableContentPack.networkedObjectPrefabs, Interactable);
        //    InteractableDirectorCard.prefab = Interactable;
        //}
        //
        //private void HandleAvailableMonsters(System.Collections.Generic.List<DirectorAPI.DirectorCardHolder> arg1, DirectorAPI.StageInfo arg2)
        //{
        //    DropPodController.currentStageMonsters = arg1.Where(cardHolder => cardHolder.MonsterCategory == DirectorAPI.MonsterCategory.BasicMonsters)
        //                                                 .Select(cardHolder => cardHolder.Card)
        //                                                 .ToArray();
        //}
        //
        //private void DestroyUneededObjectsAndComponents()
        //{
        //    Object.Destroy(Interactable.GetComponent<SurvivorPodController>());
        //    Object.Destroy(Interactable.GetComponent<VehicleSeat>());
        //    //Interactable.GetComponents<InstantiatePrefabOnStart>().ToList().ForEach(component => Object.Destroy(component));
        //    Object.Destroy(Interactable.GetComponent<BuffPassengerWhileSeated>());
        //
        //    var flames = Interactable.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base/Flames");
        //    Object.Destroy(flames.gameObject);
        //
        //    var donut = Interactable.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base/FireDonut");
        //    Object.Destroy(donut.gameObject);
        //
        //    /*var mdl = Interactable.transform.Find("Base/mdlEscapePod");
        //    Object.Destroy(mdl.GetComponent<Animator>());*/
        //}
        //
        //private void EnableObjects()
        //{
        //    var debris = Interactable.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base/DebrisParent");
        //    debris.gameObject.SetActive(true);
        //}
        //
        //private void ModifyExistingComponents()
        //{
        //    var stateMachine = Interactable.GetComponent<EntityStateMachine>();
        //    stateMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.DropPod.Idle));
        //    stateMachine.mainStateType = new SerializableEntityStateType(typeof(Idle));
        //
        //    var modelLocator = Interactable.GetComponent<ModelLocator>();
        //    modelLocator.enabled = true;
        //    modelLocator.modelBaseTransform = null;
        //
        //    var exitPos = Interactable.transform.Find("Base/mdlEscapePod/EscapePodArmature/ExitPosition");
        //    exitPos.transform.localPosition = new Vector3(0, -2, 0);
        //    var cLocator = modelLocator.modelTransform.GetComponent<ChildLocator>();
        //    HG.ArrayUtils.ArrayAppend(ref cLocator.transformPairs, new ChildLocator.NameTransformPair { name = "ExitPos", transform = exitPos });
        //}
        //
        //private void AddNewComponents()
        //{
        //    var networkTransform = Interactable.AddComponent<NetworkTransform>();
        //    networkTransform.transformSyncMode = NetworkTransform.TransformSyncMode.SyncTransform;
        //    Interactable.AddComponent<GenericDisplayNameProvider>().displayToken = "SS2_INTERACTABLE_DROPPOD_NAME";
        //    Interactable.AddComponent<DropPodController>();
        //
        //    PaintDetailsBelow details = Interactable.AddComponent<PaintDetailsBelow>();
        //    details.influenceOuter = 2;
        //    details.influenceInner = 1;
        //    details.layer = 0;
        //    details.density = 0.5f;
        //    details.densityPower = 3;
        //
        //    details = Interactable.AddComponent<PaintDetailsBelow>();
        //    details.influenceOuter = 2;
        //    details.influenceInner = 1;
        //    details.layer = 1;
        //    details.density = 0.3f;
        //    details.densityPower = 3;
        //
        //    var podMesh = Interactable.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base").gameObject;
        //    podMesh.AddComponent<EntityLocator>().entity = Interactable;
        //}
    }
}
