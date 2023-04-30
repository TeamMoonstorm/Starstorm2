using EntityStates;
using Moonstorm.Starstorm2.Components;
using R2API;
using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Interactables
{
    [DisabledContent]
    public sealed class DroneTable : InteractableBase
    {
        public override GameObject Interactable { get => interactable; }

        private static GameObject interactable;

        public static CostTypeDef voidCostDef;
        public static int voidCostTypeIndex;

        public override MSInteractableDirectorCard InteractableDirectorCard { get; } = SS2Assets.LoadAsset<MSInteractableDirectorCard>("midcDroneTable");

        public override void Initialize()
        {
            CostTypeCatalog.modHelper.getAdditionalEntries += addVoidCostType;
            //On.RoR2.PurchaseInteraction.GetDisplayName += MonolithName;
            interactable = InteractableDirectorCard.prefab;

            var interactionToken = interactable.AddComponent<PortalInteractableToken>();
            interactionToken.PurchaseInteraction = interactable.GetComponent<PurchaseInteraction>();
            interactionToken.symbolTransform = null;


        }

        private void addVoidCostType(List<CostTypeDef> obj)
        {
            //CostTypeIndex voidItem = new CostTypeIndex();
            voidCostDef = new CostTypeDef();
            voidCostDef.costStringFormatToken = "VV_COST_VOIDITEM_FORMAT";
            voidCostDef.isAffordable = new CostTypeDef.IsAffordableDelegate(VoidItemCostTypeHelper.IsAffordable);
            voidCostDef.payCost = new CostTypeDef.PayCostDelegate(VoidItemCostTypeHelper.PayCost);
            voidCostDef.colorIndex = ColorCatalog.ColorIndex.Interactable;
            voidCostDef.saturateWorldStyledCostString = true;
            voidCostDef.darkenWorldStyledCostString = false;
            voidCostTypeIndex = CostTypeCatalog.costTypeDefs.Length + obj.Count;
            //LanguageAPI.Add("VV_COST_VOIDITEM_FORMAT", "1 Item(s)");
            obj.Add(voidCostDef);
        }

        //private string MonolithName(On.RoR2.PurchaseInteraction.orig_GetDisplayName orig, PurchaseInteraction self)
        //{
        //    //Debug.Log("name: " + self.displayNameToken + " | cost: ");
        //    if (self.displayNameToken == $"VV_INTERACTABLE_{InteractableLangToken}_NAME")
        //    {
        //        return InteractableName;
        //    }
        //    return orig(self);
        //}

        private static class VoidItemCostTypeHelper
        {
            public static bool IsAffordable(CostTypeDef costTypeDef, CostTypeDef.IsAffordableContext context)
            {
                CharacterBody component = context.activator.GetComponent<CharacterBody>();
                if (!component)
                {
                    return false;
                }
                Inventory inventory = component.inventory;
                if (!inventory)
                {
                    return false;
                }
                int cost = context.cost;
                //int num = 0;
                var minions = CharacterMaster.readOnlyInstancesList.Where(el => el.minionOwnership.ownerMaster == component.master);
                List<CharacterMaster> validMinions = new List<CharacterMaster>();
                foreach(var minion in minions)
                {

                    SS2Log.Info(minion.miscFlags);
                    var minionBody = minion.GetComponent<CharacterBody>();
                    if (minionBody)
                    {
                        SS2Log.Info(minionBody.bodyIndex + " | " + minionBody.name);
                    }

                    //if("is a drone"){
                    //validMinions.Add(minion);
                    //}
                }

                int itemCount = inventory.GetTotalItemCountOfTier(ItemTier.VoidTier1) + inventory.GetTotalItemCountOfTier(ItemTier.VoidTier2) + inventory.GetTotalItemCountOfTier(ItemTier.VoidTier3) + inventory.GetTotalItemCountOfTier(ItemTier.VoidBoss);
                
                if (itemCount >= cost)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public static void PayCost(CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
            {
                Inventory inventory = context.activator.GetComponent<CharacterBody>().inventory;
                int cost = context.cost;

                List<ItemIndex> list = new List<ItemIndex>(inventory.itemAcquisitionOrder);

                List<ItemIndex> optionList = new List<ItemIndex>();
                foreach (ItemIndex item in list)
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(item);
                    if (itemDef.tier == ItemTier.VoidTier1 || itemDef.tier == ItemTier.VoidTier2 || itemDef.tier == ItemTier.VoidTier3 || itemDef.tier == ItemTier.VoidBoss)
                    {
                        optionList.Add(item);
                    }

                }
                Util.ShuffleList(optionList, context.rng);

                for (int k = 0; k < cost; k++)
                {
                    TakeOne();
                }
                MultiShopCardUtils.OnNonMoneyPurchase(context);
                void TakeOne()//Inventory inventory, CostTypeDef.PayCostContext context, int cost)
                {
                    for (int i = 0; i < optionList.Count(); i++)
                    {
                        if (inventory.GetItemCount(optionList[i]) > 0)
                        {

                            inventory.RemoveItem(optionList[i]);
                            context.results.itemsTaken.Add(optionList[i]);
                            break;

                        }
                    }
                }
            }
        }

        public class PortalInteractableToken : NetworkBehaviour
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
                PurchaseInteraction.costType = (CostTypeIndex)1;
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

            //private bool AttemptSpawnVoidPortal()
            //{
            //    //InteractableSpawnCard portalSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/PortalShop/iscShopPortal.asset").WaitForCompletion();
            //    string spawnMessageToken = "<color=#DD7AC6>The rift opens...</color>";
            //    GameObject exists = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(ShatteredMonolith.VoidFieldsPortalCard, new DirectorPlacementRule
            //    {
            //        minDistance = 1,
            //        maxDistance = 25,
            //        placementMode = DirectorPlacementRule.PlacementMode.Approximate,
            //        position = base.transform.position,
            //        spawnOnTarget = transform
            //    }, Run.instance.stageRng));
            //    if (exists && !string.IsNullOrEmpty(spawnMessageToken))
            //    {
            //        Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            //        {
            //            baseToken = spawnMessageToken
            //        });
            //    }
            //    return exists;
            //}

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
