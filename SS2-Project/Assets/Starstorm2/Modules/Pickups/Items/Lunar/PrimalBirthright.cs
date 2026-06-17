using EntityStates;
using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using RoR2.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static SS2.Items.FieldAccelerator;
using Console = RoR2.Console;
using GoldTitanManager = IL.RoR2.GoldTitanManager;
using HealthBar = On.RoR2.UI.HealthBar;
using Object = System.Object;
using Random = UnityEngine.Random;

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
        public static DamageColorIndex primalStormDamageColor;
        public static GameObject stormObject;

        public override void Initialize()
        {
            On.RoR2.TeleporterInteraction.IdleState.OnInteractionBegin += TeleporterInteractionPrimalOverride;
            On.RoR2.SceneDirector.PopulateScene += PopulateSceneAddPrimalChest;
            On.RoR2.PurchaseInteraction.GetDisplayName += GetDisplayNameAlterPrimalName;
            On.RoR2.UI.HealthBar.UpdateBarInfos += HealthBarOnUpdateBarInfos;
            //On.RoR2.SceneDirector.OnServerTeleporterPlaced += OnTeleporterPlacedAddPrimalToken;
            //ObjectivePanelController.collectObjectiveSources -= this.OnCollectObjectiveSources;

            var tempChest = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/GoldChest/GoldChest.prefab").WaitForCompletion();
            indevChest = PrefabAPI.InstantiateClone(tempChest, "PrimalChest");

            indevChest.TryGetComponent<PurchaseInteraction>(out var pinter);

            if (pinter != null)
            {
                pinter.displayNameToken = "SS2_BIRTHRIGHT_CHEST_NAME_FORMAT";
                pinter.contextToken = "SS2_BIRTHRIGHT_CHEST_CONTEXT";
            }
            

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

            // TODO: A util method for adding components to all the teleporters in the game so it remains future proof + mod compat
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Teleporters/Teleporter1.prefab").Completed += (r) => r.Result.AddComponent<PrimalPrevention>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Teleporters/LunarTeleporter Variant.prefab").Completed += (r) => r.Result.AddComponent<PrimalPrevention>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/conduitcanyon/Teleporter_ConduitCanyon_Variant.prefab").Completed += (r) => r.Result.AddComponent<PrimalPrevention>();

            //storm stuff .,. ,
            primalStormDamageColor = ColorsAPI.RegisterDamageColor(new Color32(200, 200, 255, 255));
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Meteor/MeteorStorm.prefab").Completed += (r) =>
            {
                stormObject = PrefabAPI.InstantiateClone(r.Result, "PrimalMeteorStorm");
                
                MeteorStormController regularController = stormObject.GetComponent<MeteorStormController>();
                PrimalMeteorStormController primalController = stormObject.AddComponent<PrimalMeteorStormController>();
                primalController.waveCount = 40;
                primalController.waveMinInterval = 10f;
                primalController.waveMaxInterval = 20f;
                primalController.warningEffectPrefab = regularController.warningEffectPrefab;
                primalController.impactEffectPrefab = regularController.impactEffectPrefab;
                primalController.blastDamageType = regularController.blastDamageType;
                primalController.impactDelay = 2;
                primalController.blastRadius = 8;
                primalController.blastDamageCoefficient = 3;
                UnityEngine.Object.Destroy(regularController);
                
                PrefabAPI.RegisterNetworkPrefab(stormObject);
            };
        }

        private void HealthBarOnUpdateBarInfos(HealthBar.orig_UpdateBarInfos orig, RoR2.UI.HealthBar self)
        {
            //stolen from neb neb ,.., probably should use an item like they did too ,., .
            orig(self);
            
            HealthComponent healthComponent = self._source;
            if (!healthComponent) return;
            
            if (PrimalMeteorStormController.golemList.Contains(healthComponent.body))
            {
                self.barInfoCollection.trailingOverHealthbarInfo.color = new Color32(100, 200, 255, 255);
            }
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
        public static float timer;
        public GameObject spawnedMeteors;

        [SyncVar(hook = "SetMaster")]
        public GameObject masterObject;

        public CharacterMaster master;
        
        public void OnEnable()
        {
            pinter = this.gameObject.GetComponent<PurchaseInteraction>();
            instanceList.Add(pinter);
            timer = 270;

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

        public void FixedUpdate()
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                return;
            }

            if (NetworkServer.active && !spawnedMeteors)
            {
                spawnedMeteors = Instantiate(PrimalBirthright.stormObject);
                NetworkServer.Spawn(spawnedMeteors);
                SS2Log.Debug("spawning meteors !!");

                //add meteors at each birthright because i am evil ., 
                foreach (PurchaseInteraction birthrightPurchaseInteraction in instanceList)
                {
                    MeteorStormController.Meteor meteor = new MeteorStormController.Meteor();
                    meteor.impactPosition = birthrightPurchaseInteraction.gameObject.transform.position;
                    
                    Vector3 origin = meteor.impactPosition + Vector3.up * 6f;
                    Vector3 onUnitSphere = Random.onUnitSphere;
                    onUnitSphere.y = -1f;
                    if (Physics.Raycast(origin, onUnitSphere, out var hitInfo, 12f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                    {
                        meteor.impactPosition = hitInfo.point;
                    }
                    else if (Physics.Raycast(meteor.impactPosition, Vector3.down, out hitInfo, float.PositiveInfinity, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                    {
                        meteor.impactPosition = hitInfo.point;
                    }
                    
                    spawnedMeteors.GetComponent<PrimalMeteorStormController>().DetonateMeteor(meteor);
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
            if (PrimalBirthrightObjectiveToken.timer <= 0)
            {
                return string.Format(Language.GetString("SS2_BIRTHRIGHT_OBJECTIVEFAILED"), PrimalBirthrightObjectiveToken.instanceList.Count);
            }
            
            string text = string.Format(Language.GetString("SS2_BIRTHRIGHT_OBJECTIVE"), PrimalBirthrightObjectiveToken.instanceList.Count, PrimalBirthrightObjectiveToken.timer.ToString("0"));
            if (PrimalBirthrightObjectiveToken.timer < 30 && (int)(Time.time * 12f) % 2 == 0)
            {
                text = $"<style=cDeath>{text}</style>";
            }

            return text;
        }

        public override bool IsDirty()
        {
            return true;
        }
    }

    public class PrimalMeteorStormController : MeteorStormController
    {
        public CharacterMaster master;
        public static List<CharacterBody> golemList = new List<CharacterBody>();
        private void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            
            waveTimer -= Time.fixedDeltaTime;
            if (waveTimer <= 0f && waveList.Count == 0)
            {
                waveTimer = UnityEngine.Random.Range(waveMinInterval, waveMaxInterval);
                SS2Log.Debug("adding new wave ,.,.");
                MeteorWave wave = new MeteorWave(master?.GetBody() ? new[]{master.GetBody()} : CharacterBody.readOnlyInstancesList.ToArray(), master?.GetBody() ? master.GetBody().transform.position : CharacterBody.readOnlyInstancesList.FirstOrDefault(body => body != null) != null ? CharacterBody.readOnlyInstancesList.FirstOrDefault(body => body != null).transform.position : base.transform.position);
                waveList.Add(wave);
            }

            for (int i = waveList.Count - 1; i >= 0; i--)
            {
                MeteorWave meteorWave = waveList[i];
                meteorWave.timer -= Time.fixedDeltaTime;
                if (meteorWave.timer <= 0f)
                {
                    meteorWave.timer = UnityEngine.Random.Range(1f, 3f);
                    Meteor nextMeteor = meteorWave.GetNextMeteor();
                    
                    if (nextMeteor == null)
                    {
                        waveList.RemoveAt(i);
                    }
                    else if (nextMeteor.valid)
                    {
                        meteorList.Add(nextMeteor);
                        EffectManager.SpawnEffect(warningEffectPrefab, new EffectData
                        {
                            origin = nextMeteor.impactPosition,
                            scale = blastRadius
                        }, transmit: true);
                    }
                }
            }
            
            float num2 = Run.instance.time - impactDelay;
            float num3 = num2 - travelEffectDuration;
            for (int num4 = meteorList.Count - 1; num4 >= 0; num4--)
            {
                Meteor meteor = meteorList[num4];
                if (meteor.startTime < num3 && !meteor.didTravelEffect)
                {
                    DoMeteorEffect(meteor);
                }
                if (meteor.startTime < num2)
                {
                    meteorList.RemoveAt(num4);
                    DetonateMeteor(meteor);
                }
            }
        }
        
        private void DetonateMeteor(Meteor meteor)
        {
            SS2Log.Debug("spawning evil ,., ");

            if (TeamComponent.GetTeamMembers(TeamIndex.Lunar).Count < 15)
            {
                CharacterSpawnCard spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LunarGolem/cscLunarGolem.asset").WaitForCompletion();
                DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(
                    spawnCard,
                    new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Direct,
                        position = meteor.impactPosition
                    },
                    RoR2Application.rng
                );
                spawnRequest.teamIndexOverride = TeamIndex.Lunar;
                spawnRequest.onSpawnedServer += result =>
                {
                    CharacterMaster golemMaster = result.spawnedInstance?.GetComponent<CharacterMaster>();
                    if (!golemMaster) return;
                
                    golemMaster.onBodyDeath.AddListener(testDeath);
                    golemList.Add(golemMaster.GetBody());
                    return;

                    void testDeath()
                    {
                        golemList.Remove(null);
                    }
                };
                RoR2.DirectorCore.instance.TrySpawnObject(spawnRequest);
            }
     
            EffectData effectData = new EffectData
            {
                origin = meteor.impactPosition
            };
            EffectManager.SpawnEffect(impactEffectPrefab, effectData, transmit: true);
            BlastAttack blastAttack = new BlastAttack
            {
                inflictor = base.gameObject,
                baseDamage = blastDamageCoefficient * golemList[0].damage,
                baseForce = blastForce,
                attackerFiltering = AttackerFiltering.Default,
                crit = false,
                falloffModel = BlastAttack.FalloffModel.Linear,
                attacker = null,
                bonusForce = Vector3.zero,
                damageColorIndex = PrimalBirthright.primalStormDamageColor,
                position = meteor.impactPosition,
                procChainMask = default(ProcChainMask),
                procCoefficient = 1f,
                teamIndex = TeamIndex.Lunar,
                radius = blastRadius,
                damageType = blastDamageType
            };
            blastAttack.Fire();
        }
    }
}


