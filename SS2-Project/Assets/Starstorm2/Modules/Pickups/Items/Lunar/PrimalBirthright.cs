using R2API;
using RoR2;
using RoR2.Items;

using MSU;
using System.Collections.Generic;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using EntityStates;
using UnityEngine.AddressableAssets;
using System;
using RoR2.UI;
using UnityEngine.Networking;

namespace SS2.Items
{
    public sealed class PrimalBirthright : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acPrimalBirthright", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of legendary chests the first stack grants per stage.")]
        [FormatToken("SS2_ITEM_PRIMAL_BIRTHRIGHT_DESC", 0)]
        public static float legendaryCountBase = 1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of legendary chests each additional stack grants per stage.")]
        [FormatToken("SS2_ITEM_PRIMAL_BIRTHRIGHT_DESC", 1)]
        public static float legendaryCountStacking = 1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount the price of the Legendary chest is multiplied by. (1 = 100%, normal value for current level)")]
        [FormatToken("SS2_ITEM_RELICOFMASS_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float priceModifier = 2.1f;

        public GameObject indevChest; // this probably not be here in a live build i just wanted to do it fast
        public InteractableSpawnCard indevCard;
        public static PrimalPrevention? primalToken;
        public Xoroshiro128Plus birthrightRng;

        public override void Initialize()
        {
            On.RoR2.TeleporterInteraction.IdleState.OnInteractionBegin += TeleporterInteractionPrimalOverride;
            On.RoR2.SceneDirector.PopulateScene += PopulateSceneAddPrimalChest;
            On.RoR2.PurchaseInteraction.GetDisplayName += GetDisplayNameAlterPrimalName;
            //On.RoR2.SceneDirector.OnServerTeleporterPlaced += OnTeleporterPlacedAddPrimalToken;
            //ObjectivePanelController.collectObjectiveSources -= this.OnCollectObjectiveSources;

            var tempChest = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/GoldChest/GoldChest.prefab").WaitForCompletion();
            indevChest = PrefabAPI.InstantiateClone(tempChest, "PrimalChest");

            var pinter = indevChest.GetComponent<PurchaseInteraction>();
            pinter.displayNameToken = "SS2_BIRTHRIGHT_CHEST_NAME_FORMAT";
            pinter.contextToken = "SS2_BIRTHRIGHT_CHEST_CONTEXT";

            var token = indevChest.AddComponent<PrimalBirthrightObjectiveToken>();
            //token.enabled = false;

            //token.RpcSetToken(false); //unsure if needed

            pinter.onPurchase.AddListener(delegate (Interactor interactor) 
            {
                this.OnPurchaseBirthrightChest(interactor, pinter);
            });
            
            try
            {
                var smr = indevChest.transform.Find("mdlGoldChest").Find("GoldChestArmature").Find("GoldChestMesh").gameObject.GetComponent<SkinnedMeshRenderer>(); //hi nebby!!! hii!!! 
                smr.sharedMaterial = AssetCollection.FindAsset<Material>("matTrimSheetMetalBlue");
            }
            catch(Exception e)
            {
                SS2Log.Error("Unable to apply Primal Chest material. Unexpected hierarchy: " + e);
            }

            //indevChest.AddComponent<PrimalBirthrightNetBehaviorReal>();
            //indevChest.AddComponent<PrimalBirthrightNetBehavior>();
            
            PrefabAPI.RegisterNetworkPrefab(indevChest);

            //var assets = AssetCollection.assets;
            //UnityEngine.Object[] assets2 = new UnityEngine.Object[assets.Length + 1];
            //assets.CopyTo(assets2, 0);
            //assets2[assets2.Length - 1] = indevChest;
            //AssetCollection.assets = assets2;

            UnityEngine.Object[] assets2 = new UnityEngine.Object[1];
            assets2[0] = indevChest;
            AssetCollection.AddAssets(assets2);

            indevCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            indevCard.name = "iscPrimalChest";
            indevCard.prefab = indevChest;
            indevCard.sendOverNetwork = true;
            indevCard.hullSize = HullClassification.Human;
            indevCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            indevCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
            indevCard.forbiddenFlags = RoR2.Navigation.NodeFlags.None;

            indevCard.directorCreditCost = 0;

            indevCard.occupyPosition = true;
            indevCard.orientToFloor = true;
            indevCard.skipSpawnWhenSacrificeArtifactEnabled = false;
            indevCard.maxSpawnsPerStage = -1;


            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Teleporters/Teleporter1.prefab").Completed += (r) => r.Result.AddComponent<PrimalPrevention>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Teleporters/LunarTeleporter Variant.prefab").Completed += (r) => r.Result.AddComponent<PrimalPrevention>();

        }

        private string GetDisplayNameAlterPrimalName(On.RoR2.PurchaseInteraction.orig_GetDisplayName orig, PurchaseInteraction self)
        {
            var pbot = self.GetComponent<PrimalBirthrightObjectiveToken>();
            if (pbot)
            {
                var intermediate = Language.GetString(self.displayNameToken);
                
                if (pbot.master) 
                {
                    return intermediate.Replace("{0}", Util.GetBestMasterName(pbot.master));
                }
                //intermediate.Replace("{0}", pbot.masterObject.GetComponent<CharacterMaster>().GetBody().GetDisplayName())
               
            }
            return orig(self);
        }

        private void OnPurchaseBirthrightChest(Interactor interactor, PurchaseInteraction pinter)
        {
            var pbot = pinter.GetComponent<PrimalBirthrightObjectiveToken>();
            if (pbot)
            {
                pbot.enabled = false;
                pbot.RpcSetToken(false);
            }
        }

        public static void OnCollectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
        {
            var newObjective = new ObjectivePanelController.ObjectiveSourceDescriptor
            {
                master = master,
                objectiveType = typeof(PrimalBirthrightObjectiveTracker),
                source = primalToken
            };

            objectiveSourcesList.Add(newObjective);
        }

        private void TeleporterInteractionPrimalOverride(On.RoR2.TeleporterInteraction.IdleState.orig_OnInteractionBegin orig, BaseState self, Interactor activator)
        {
            var tc = self.outer.gameObject.GetComponent<PrimalPrevention>();
            if (tc)
            {
                //var listcopy = tc.purchaseInteractions;
                var listcopy = PrimalBirthrightObjectiveToken.instanceList;
                Util.ShuffleList(listcopy);
                if (birthrightRng == null)
                {
                    birthrightRng = new Xoroshiro128Plus(Run.instance.seed);
                }

                foreach (var pinter in listcopy)
                {
                    if (pinter)
                    {
                        var pbot = pinter.GetComponent<PrimalBirthrightObjectiveToken>();
                        if (pbot)
                        {
                            if (birthrightRng.RangeFloat(0, 1) >= .975f)
                            {
                                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                                {
                                    baseToken = "SS2_BIRTHRIGHT_UNCLAIMED_RARE"
                                });
                            }
                            else
                            {
                                var body = pbot.master.GetBody();
                                if (body)
                                {
                                    if (body.GetItemCount(SS2Content.Items.PrimalBirthright) <= 0)
                                    {
                                        Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                                        {
                                            subjectAsCharacterBody = pbot.master.GetBody(),
                                            baseToken = "SS2_BIRTHRIGHT_UNCLAIMED_REMOVED"
                                        });
                                    }
                                    else
                                    {
                                        Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                                        {
                                            subjectAsCharacterBody = pbot.master.GetBody(),
                                            baseToken = "SS2_BIRTHRIGHT_UNCLAIMED"
                                        });
                                    }
                                }
                                else
                                {
                                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                                    {
                                        baseToken = "SS2_BIRTHRIGHT_UNCLAIMED_DEAD"
                                    });
                                }
                            }
                            return;
                        }
                    }
                }
                orig(self, activator);
            }
            else
            {
                orig(self, activator);
            }
        }

        private void PopulateSceneAddPrimalChest(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            primalToken = null;
            orig(self); //things after orig occur after PlaceTeleporter

            if (self.teleporterInstance){
                primalToken = self.teleporterInstance.GetComponent<PrimalPrevention>();
                primalToken.filter = self.teleporterInstance.AddComponent<InteractionProcFilter>();
                primalToken.filter.shouldAllowOnInteractionBeginProc = false;
            }

            var sceneDef = SceneCatalog.GetSceneDefForCurrentScene();
            if (sceneDef.sceneType == SceneType.Stage && primalToken)
            {
                if (birthrightRng == null){ birthrightRng = new Xoroshiro128Plus(Run.instance.seed); }
                foreach (var player in PlayerCharacterMasterController.instances)
                {
                    //SS2Log.Info("Found a player with item");
                    int itemCount = player.master.inventory.GetItemCount(SS2Content.Items.PrimalBirthright);
                    for (int i = 0; i < itemCount; ++i)
                    {
                        var chest = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(indevCard, new DirectorPlacementRule { placementMode = DirectorPlacementRule.PlacementMode.Random }, birthrightRng));
                        //SS2Log.Warning("Chest : " + chest + " | " + chest.name);
                        var pinter = chest.GetComponent<PurchaseInteraction>();
                        var behav = chest.GetComponent<ChestBehavior>();
                        if (pinter && behav)
                        {
                            //primalToken.purchaseInteractions.Add((pinter, player.master));
                            pinter.Networkcost = (int)(Run.instance.GetDifficultyScaledCost(pinter.cost) * priceModifier);

                            pinter.onPurchase.AddListener(delegate (Interactor interactor)
                            {
                                this.OnPurchaseBirthrightChest(interactor, pinter);
                            });

                            var objtoken = chest.GetComponent<PrimalBirthrightObjectiveToken>();
                            objtoken.masterObject = player.master.gameObject;
                            objtoken.master = player.master;
                            
                            //objtoken.playername = Util.GetBestMasterName(player.master);

                            //pinter.GetDisplayName

                        }
                    }
                }
            }
        }

    }

    public class PrimalPrevention : MonoBehaviour
    {
        public InteractionProcFilter filter;
        //i swear this makes sense

    }

    public class PrimalBirthrightObjectiveToken : NetworkBehaviour {
        public static List<PurchaseInteraction> instanceList = new List<PurchaseInteraction>();
        public PurchaseInteraction pinter;

        [SyncVar(hook = "SetMaster")]
        public GameObject masterObject;

        public CharacterMaster master;
        
        public void OnEnable()
        {
            pinter = this.gameObject.GetComponent<PurchaseInteraction>();
            instanceList.Add(pinter);

            if (instanceList.Count == 1)
            {
                ObjectivePanelController.collectObjectiveSources += PrimalBirthright.OnCollectObjectiveSources;
            }
        }

        public void OnDisable()
        {
            instanceList.Remove(pinter);

            if (instanceList.Count <= 0)
            {
                ObjectivePanelController.collectObjectiveSources -= PrimalBirthright.OnCollectObjectiveSources;

                if (PrimalBirthright.primalToken)
                {
                    PrimalBirthright.primalToken.filter.shouldAllowOnInteractionBeginProc = true;
                }
            }
        }

        [ClientRpc]
        public void RpcSetToken(bool enable)
        {
            this.enabled = enable;
        }

        public void SetMaster(GameObject masterObj)
        {
            master = masterObj.GetComponent<CharacterMaster>();
        }

    }

    public class PrimalBirthrightObjectiveTracker : ObjectivePanelController.ObjectiveTracker
    {
        public override string GenerateString()
        {
            return string.Format(Language.GetString("SS2_BIRTHRIGHT_OBJECTIVE"), PrimalBirthrightObjectiveToken.instanceList.Count);
        }

        public override bool IsDirty()
        {
            return true;
        }
    }

}


