using EntityStates;
using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.Items;
using RoR2.UI;
using System;
using System.Collections.Generic;
using Starstorm2.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
        
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "How quickly you must claim your birthright (base).")]
        [FormatToken("SS2_ITEM_PRIMAL_BIRTHRIGHT_DESC", 2)]
        public static float birthrightCompletionTime = 200f;
        
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "How quickly you must claim your birthright (stacking).")]
        [FormatToken("SS2_ITEM_PRIMAL_BIRTHRIGHT_DESC", 3)]
        public static float birthrightCompletionTimeStacking = 125f;
        
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount the price of the Legendary chest is multiplied by. (1 = 100%, normal value for current level)")]
        public static float birthrightPriceModifier = 1.3f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Required stage count for Lunar Wisps to start replacing Lunar Golems upon failing to claim your birthright.")]
        public static float lunarWispStageCount = 3f;
        
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Chance for Lunar Wisps to replace Lunar Golems upon failing to claim your birthright.")]
        public static float lunarWispChance = 20f;
        
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Chance for perfected enemies to replace regular enemy spawns upon failing to claim your birthright.")]
        public static float perfectedReplacementChance = 15f;
        
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Wait time between waves of Lunar Chimeras upon failing to claim your birthright.")]
        public static float chimeraWaitTime = 30f;
        
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Wait time variance between waves of Lunar Chimeras upon failing to claim your birthright. (Minimum wait time will be base wait time - variance)")]
        public static float chimeraWaitTimeVariance = 5f;
        
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Whether Lunar Chimeras should use Lunar team. (causes enemy infighting (silly))")]
        public static bool useLunarTeam = false;
        
        public GameObject indevChest; // this probably not be here in a live build i just wanted to do it fast
        public InteractableSpawnCard indevCard;
        public static PrimalPrevention? primalToken;
        public Xoroshiro128Plus birthrightRng;
        public static DamageColorIndex primalStormDamageColor;
        public static GameObject stormPrefab;
        
        public override void Initialize()
        {
            On.RoR2.TeleporterInteraction.IdleState.OnInteractionBegin += TeleporterInteractionPrimalOverride;
            On.RoR2.SceneDirector.PopulateScene += PopulateSceneAddPrimalChest;
            On.RoR2.PurchaseInteraction.GetDisplayName += GetDisplayNameAlterPrimalName;
            On.RoR2.CombatDirector.Spawn += CombatDirectorOnSpawnPerfected;
            On.RoR2.TeleporterInteraction.Awake += TeleporterInteractionAwakeAddPrimalPrevention;

            var tempChest = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/GoldChest/GoldChest.prefab").WaitForCompletion();
            indevChest = PrefabAPI.InstantiateClone(tempChest, "PrimalChest");

            indevChest.TryGetComponent<PurchaseInteraction>(out var pinter);

            if (pinter != null)
            {
                pinter.displayNameToken = "SS2_BIRTHRIGHT_CHEST_NAME_FORMAT";
                pinter.contextToken = "SS2_BIRTHRIGHT_CHEST_CONTEXT";
            }
            
            indevChest.AddComponent<PrimalBirthrightObjectiveToken>();

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
            
            PrefabAPI.RegisterNetworkPrefab(indevChest);

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
            
            //storm stuff .,. ,
            primalStormDamageColor = ColorsAPI.RegisterDamageColor(new Color32(200, 200, 255, 255));
            stormPrefab = AssetCollection.FindAsset<GameObject>("PrimalMeteorStorm");
        }

        private void TeleporterInteractionAwakeAddPrimalPrevention(On.RoR2.TeleporterInteraction.orig_Awake orig, TeleporterInteraction self)
        {
            //seriouslyt why were we doing the prefab stuff before this works fine TT ,.,.
            orig(self);
            self.gameObject.AddComponent<PrimalPrevention>();
        }

        private bool CombatDirectorOnSpawnPerfected(On.RoR2.CombatDirector.orig_Spawn orig, CombatDirector self, SpawnCard spawncard, EliteDef elitedef, Transform spawntarget, DirectorCore.MonsterSpawnDistance spawndistance, bool preventoverhead, float valuemultiplier, DirectorPlacementRule.PlacementMode placementmode, bool singlescaledboss)
        {
            if (NetworkServer.active && BirthrightObjectiveTimer.instance?.spawnedMeteors == true && !elitedef && Run.instance.spawnRng.RangeFloat(0, 100) <= perfectedReplacementChance && spawncard.prefab.GetComponent<CharacterMaster>()?.bodyPrefab?.GetComponent<CharacterBody>()?.isChampion != true)
            {
                elitedef = RoR2Content.Elites.Lunar;
                SS2Log.Debug($"set {spawncard.prefab.name} to be perfected ,.., ,.");
            }

            return orig(self, spawncard, elitedef, spawntarget, spawndistance, preventoverhead, valuemultiplier, placementmode, singlescaledboss);
        }

        private string GetDisplayNameAlterPrimalName(On.RoR2.PurchaseInteraction.orig_GetDisplayName orig, PurchaseInteraction self)
        {
            var pbot = self.GetComponent<PrimalBirthrightObjectiveToken>();
            if (pbot)
            {
                var intermediate = Language.GetString(self.displayNameToken);
                
                if (pbot.playerName != "") 
                {
                    return intermediate.Replace("{0}", pbot.playerName);
                }
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

        private void TeleporterInteractionPrimalOverride(On.RoR2.TeleporterInteraction.IdleState.orig_OnInteractionBegin orig, BaseState self, Interactor activator)
        {
            self.outer.gameObject.TryGetComponent<PrimalPrevention>(out var tc);
            if (tc)
            {
                //var listcopy = tc.purchaseInteractions;
                var listcopy = PrimalBirthrightObjectiveToken.instanceList;
                Util.ShuffleList(listcopy);
                if (birthrightRng == null)
                {
                    birthrightRng = new Xoroshiro128Plus(Run.instance.seed);
                }

                foreach (PurchaseInteraction pinter in listcopy)
                {
                    if (!pinter) continue;
                    
                    var pbot = pinter.GetComponent<PrimalBirthrightObjectiveToken>();
                    if (!pbot) continue;
                    
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

            orig(self, activator);
        }

        private void PopulateSceneAddPrimalChest(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            primalToken = null;
            orig(self); //things after orig occur after PlaceTeleporter

            if (self.teleporterInstance){
                // Score reported an issue where this was null, but how?
                // Ah it was null because it happened on a stage where this component was not added most likely
                self.teleporterInstance.TryGetComponent<PrimalPrevention>(out primalToken);

                if (primalToken != null)
                {
                    primalToken.filter = self.teleporterInstance.AddComponent<InteractionProcFilter>();
                    primalToken.filter.shouldAllowOnInteractionBeginProc = false;
                }
            }

            var sceneDef = SceneCatalog.GetSceneDefForCurrentScene();
            if (primalToken && sceneDef && sceneDef.sceneType == SceneType.Stage)
            {
                if (birthrightRng == null){ birthrightRng = new Xoroshiro128Plus(Run.instance.seed); }
                foreach (var player in PlayerCharacterMasterController.instances)
                {
                    //SS2Log.Info("Found a player with item");
                    int itemCount = player.master.inventory.GetItemCountEffective(SS2Content.Items.PrimalBirthright);
                    for (int i = 0; i < itemCount; ++i)
                    {
                        var chest = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(indevCard, new DirectorPlacementRule { placementMode = DirectorPlacementRule.PlacementMode.Random }, birthrightRng));
                        //SS2Log.Warning("Chest : " + chest + " | " + chest.name);
                        var pinter = chest.GetComponent<PurchaseInteraction>();
                        var behav = chest.GetComponent<ChestBehavior>();
                        if (!pinter || !behav) continue;
                        
                        pinter.Networkcost = (int)(Run.instance.GetDifficultyScaledCost(pinter.cost) * birthrightPriceModifier);

                        pinter.onPurchase.AddListener(delegate (Interactor interactor)
                        {
                            this.OnPurchaseBirthrightChest(interactor, pinter);
                        });

                        var objtoken = chest.GetComponent<PrimalBirthrightObjectiveToken>();
                        objtoken.master = player.master;
                        objtoken.playerName = player.networkUser.userName;
                    }
                }
            }
        }
        
        public class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = false)]
            private static ItemDef GetItemDef() => SS2Content.Items.PrimalBirthright;

            private static GameObject stormObject;

            private void OnEnable()
            {
                // !BirthrightObjectiveTimer.instance would probably work here too but im worried if 2 players have it and instance is set too late or something and it spawns 2 and oough ,.,.
                if (!stormObject)
                {
                    stormObject = Instantiate(stormPrefab);
                    NetworkServer.Spawn(stormObject);
                }
            }
        }
    }

    public class PrimalPrevention : MonoBehaviour
    {
        public InteractionProcFilter filter;
        //i swear this makes sense

    }

    public class PrimalBirthrightObjectiveToken : NetworkBehaviour
    {
        private float oobTimer;
        private bool dontTryRemove;
        public static List<PurchaseInteraction> instanceList = new List<PurchaseInteraction>();
        public PurchaseInteraction pinter;
        public CharacterMaster master;

        [SyncVar] 
        public string playerName;
        
        public void OnEnable()
        {
            pinter = this.gameObject.GetComponent<PurchaseInteraction>();
            instanceList.Add(pinter);
        }

        public void OnDisable()
        {
            instanceList.Remove(pinter);

            if (instanceList.Count <= 0)
            {
                if (!dontTryRemove)
                {
                    BirthrightObjectiveTimer.instance?.TryRemoveObjective();
                }

                if (PrimalBirthright.primalToken)
                {
                    PrimalBirthright.primalToken.filter.shouldAllowOnInteractionBeginProc = true;
                }
            }
        }

        public void FixedUpdate()
        {
            oobTimer += Time.deltaTime;
            if (oobTimer > 15)
            {
                oobTimer = 0;

                if (!Util.IsPositionWithinMapBounds(gameObject.transform.position))
                {
                    if (instanceList.Count == 1)
                    {
                        //if youre drifter and throw the last birthright off the map be evil and spawn meteors. ,., 
                        BirthrightObjectiveTimer.instance.timer = 0;
                        dontTryRemove = true;
                    }
                    
                    if (NetworkServer.active)
                    {
                        Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                        {
                            baseToken = "SS2_BIRTHRIGHT_UNCLAIMED_RARE"
                        });
                    }
                    
                    Destroy(gameObject);
                }
            }
        }
        
        [ClientRpc]
        public void RpcSetToken(bool enable)
        {
            enabled = enable;
        }
    }
}