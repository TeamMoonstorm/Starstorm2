using R2API.ScriptableObjects;
using RoR2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;

namespace Moonstorm.Starstorm2.Artifacts
{
    public sealed class Cognation : ArtifactBase
    {
        public override ArtifactDef ArtifactDef { get; } = SS2Assets.LoadAsset<ArtifactDef>("Cognation", SS2Bundle.Artifacts);
        public override ArtifactCode ArtifactCode { get; } = SS2Assets.LoadAsset<ArtifactCode>("CognationCode", SS2Bundle.Artifacts);

        [RooConfigurableField(SS2Config.IDArtifact, ConfigDesc = "Whether or not cognation ghosts inherit all items from the original body")]
        public static bool InheritInventory = true;
        public static ReadOnlyCollection<MasterCatalog.MasterIndex> BlacklistedMasterIndices { get; private set; }
        private static readonly List<MasterCatalog.MasterIndex> blacklistedMasterIndices = new List<MasterCatalog.MasterIndex>();
        public static readonly List<string> blacklistedMasters = new List<string>
        {
            "ArtifactShell",
            "BrotherHurt",
            "VoidInfestor",
        };

        private static bool hasMadeList;
        private static List<BodyIndex> illegalGhosts = new List<BodyIndex>();

        [SystemInitializer(typeof(MasterCatalog))]
        private static void SystemInit() //yeah this doesnt fucking run and i'm assuming somethings like actually wrong here so instead of trying to fix it im just gonna make a workaround 
        {
            List<string> defaultBlacklistedMasters = new List<string>
            {
                "ArtifactShell",
                "BrotherHurt",
                "VoidInfestor"
            };

            foreach(string masterName in defaultBlacklistedMasters)
            {
                MasterCatalog.MasterIndex masterIndex = MasterCatalog.FindMasterIndex(masterName);
                if(masterIndex != MasterCatalog.MasterIndex.none)
                {
                    SS2Log.Info("Adding " + masterName + " with index " + masterIndex + " to cognation banlist");
                    AddMasterToBlacklist(masterIndex);
                }
            }
        }

        public static void AddMasterToBlacklist(MasterCatalog.MasterIndex masterIndex)
        {
            if(masterIndex == MasterCatalog.MasterIndex.none)
            {
                SS2Log.Debug($"Tried to add a master to the blacklist, but it's index is none.");
                return;
            }

            if(blacklistedMasterIndices.Contains(masterIndex))
            {
                GameObject prefab = MasterCatalog.GetMasterPrefab(masterIndex);
                SS2Log.Debug($"Master Prefab {prefab} is already blacklisted.");
                return;
            }

            blacklistedMasterIndices.Add(masterIndex);
            BlacklistedMasterIndices = new ReadOnlyCollection<MasterCatalog.MasterIndex>(blacklistedMasterIndices); //is this needed here? i have no fucking idea what this means tbh
            foreach(MasterCatalog.MasterIndex index in BlacklistedMasterIndices)
            {
                SS2Log.Info("Collected indexes: " + index);
            }
        }
        
        private static void InitializeIllegalGhostList()
        {
            List<string> defaultBodyNames = new List<string>
            {
                "BrotherHurtBody",
                "VoidInfestorBody",
                "ArtifactShellBody",
        
                //"BrotherBody",
                //"BrotherGlassBody",
                //"BrotherHauntBody",
                //"ShopkeeperBody",
                //"MiniVoidRaidCrabBodyBase",
                //"MiniVoidRaidCrabBodyPhase1",
                //"MiniVoidRaidCrabBodyPhase2",
                //"MiniVoidRaidCrabBodyPhase3",
                //"VoidRaidCrabJointBody",
                //"VoidRaidCrabBody",
                //"ScavLunar1Body",
                //"ScavLunar2Body",
                //"ScavLunar3Body",
                //"ScavLunar4Body",
                //"ScavSackProjectile",
                //"VagrantTrackingBomb",
                //"LunarWispTrackingBomb",
                //"GravekeeperTrackingFireball",
                //"BeetleWard",
                //"AffixEarthHealerBody",
                //"AltarSkeletonBody",
                //"DeathProjectile",
                //"TimeCrystalBody",
                //"SulfurPodBody",
                //"FusionCellDestructibleBody",
                //"ExplosivePotDestructibleBody",
                //"SMInfiniteTowerMaulingRockLarge",
                //"SMInfiniteTowerMaulingRockMedium",
                //"SMInfiniteTowerMaulingRockSmall",
                //"SMMaulingRockLarge",
                //"SMMaulingRockMedium",
                //"SMMaulingRockSmall",
            };
        
            foreach (string bodyName in defaultBodyNames)
            {
                BodyIndex index = BodyCatalog.FindBodyIndexCaseInsensitive(bodyName);
                if (index != BodyIndex.None)
                {
                    //SS2Log.Info("found body " + bodyName + " with index " + index + ", adding them");
                    AddBodyToIllegalTerminationList(index);
                }
            }
        }
        
        public static void AddBodyToIllegalTerminationList(BodyIndex bodyIndex)
        {
            if (bodyIndex == BodyIndex.None)
            {
                SS2Log.Info($"Tried to add a body to the illegal ghost list, but it's index is none");
                return;
            }
        
            if (illegalGhosts.Contains(bodyIndex))
            {
                GameObject prefab = BodyCatalog.GetBodyPrefab(bodyIndex);
                SS2Log.Info($"Body prefab {prefab} is already in the illegal ghost list.");
                return;
            }
            illegalGhosts.Add(bodyIndex);
            //BodiesThatGiveSuperCharge = new ReadOnlyCollection<BodyIndex>(bodiesThatGiveSuperCharge);
        }

        //Swuff, I Hate You 3000
        public override void Initialize()
        {
            //SceneManager.sceneLoaded += AddCode; kill!!!!!!!
        }

        private void AddCode(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name == "lobby")
            {
                var code = UObject.Instantiate(SS2Assets.LoadAsset<GameObject>("CognationCode", SS2Bundle.Artifacts), Vector3.zero, Quaternion.identity);
                code.transform.position = new Vector3(4, 0, 8);
                code.transform.rotation = Quaternion.Euler(10, 90, 0);
                code.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
        }

        public override void OnArtifactEnabled()
        {
            //IL.RoR2.CharacterMaster.OnBodyDeath += PreventDeath;
            GlobalEventManager.onCharacterDeathGlobal += SpawnCognationGhost;
        }

        public override void OnArtifactDisabled()
        {
            //IL.RoR2.CharacterMaster.OnBodyDeath -= PreventDeath;
            GlobalEventManager.onCharacterDeathGlobal -= SpawnCognationGhost;
        }

        //private string AddCognateName2(On.RoR2.Util.orig_GetBestBodyName orig, GameObject bodyObject)
        //{
        //    var result = orig(bodyObject);
        //    //SS2Log.Info("ahhhh");
        //    CharacterBody characterBody = bodyObject?.GetComponent<CharacterBody>();
        //    if (characterBody && characterBody.inventory && characterBody.inventory.GetItemCount(SS2Content.Items.Cognation) > 0)
        //    {
        //        result = Language.GetStringFormatted("SS2_ARTIFACT_COGNATION_PREFIX", result);
        //    }
        //    return result;
        //}

        private void SpawnCognationGhost(DamageReport report)
        {
            if (!hasMadeList)
            {
                InitializeIllegalGhostList(); //this is so fucking jank
                hasMadeList = true;
            }
            CharacterMaster victimMaster = report.victimMaster;

            if (victimMaster)
            {
                if (CanBecomeGhost(victimMaster))
                {
                    var summon = PrepareMasterSummon(victimMaster);
                    if (summon != null)
                    {
                        SummonGhost(summon.Perform(), victimMaster);
                    }
                }
            }
        }
        private string AddCognateName(On.RoR2.Util.orig_GetBestBodyName orig, GameObject bodyObject)
        {
            var body = bodyObject.GetComponent<CharacterBody>();
            if (body)
            { 
                if (body.inventory)
                {
                    if (body.inventory.GetItemCount(SS2Content.Items.Cognation) > 0)
                    {
                        return "Cognate " + orig(bodyObject);
                    }
                }
            }

            return orig(bodyObject);
        }
        private bool CanBecomeGhost(CharacterMaster victimMaster)
        {
            bool isMonster = victimMaster.teamIndex != TeamIndex.Player;
            bool hasBody = victimMaster.hasBody;
            bool notGhost = victimMaster.inventory.GetItemCount(SS2Content.Items.Cognation) == 0;
            //SS2Log.Info("victim master: " + victimMaster.masterIndex);
            //bool notBlacklisted = !BlacklistedMasterIndices.Contains(victimMaster.masterIndex);
            bool notBlacklisted = !illegalGhosts.Contains(victimMaster.backupBodyIndex);
            //if(victimMaster.masterIndex == ) { 
            //}
            //bool notBlacklisted
            bool notVoidTouched = victimMaster.inventory.currentEquipmentIndex != DLC1Content.Equipment.EliteVoidEquipment.equipmentIndex;
            
            return isMonster && hasBody && notGhost && notBlacklisted && notVoidTouched;
        }

        private MasterSummon PrepareMasterSummon(CharacterMaster master)
        {
            var body = master.GetBody();

            var ghostSummon = new MasterSummon();
            ghostSummon.ignoreTeamMemberLimit = true;
            ghostSummon.masterPrefab = MasterCatalog.GetMasterPrefab(master.masterIndex);
            ghostSummon.position = body.corePosition;
            ghostSummon.rotation = Quaternion.LookRotation(body.inputBank.GetAimRay().direction);
            ghostSummon.teamIndexOverride = master.teamIndex;
            ghostSummon.summonerBodyObject = null;
            ghostSummon.inventoryToCopy = InheritInventory ? master.inventory : null;

            return ghostSummon;
        }

        private void SummonGhost(CharacterMaster ghostMaster, CharacterMaster originalMaster)
        {
            var originalBody = originalMaster.GetBody();
            var ghostBody = ghostMaster.GetBody();

            if (NetworkServer.active)
            {
                ghostBody.AddTimedBuff(RoR2Content.Buffs.SmallArmorBoost, 1);
                DeathRewards ghostRewards = ghostBody.GetComponent<DeathRewards>();
                DeathRewards origRewards = originalBody.GetComponent<DeathRewards>();
                if (ghostRewards && origRewards)
                {
                    ghostRewards.goldReward = (uint)(origRewards.goldReward / 1.25f);
                    ghostRewards.expReward = (uint)(origRewards.expReward / 1.25);
                }

                if (ghostBody)
                {
                    foreach (EntityStateMachine esm in ghostBody.GetComponents<EntityStateMachine>())
                    {
                        esm.initialStateType = esm.mainStateType;
                    }
                }

                ghostMaster.inventory.SetEquipmentIndex(originalMaster.inventory.currentEquipmentIndex);
                ghostMaster.inventory.GiveItem(SS2Content.Items.Cognation);
            }

            var timer = ghostMaster.gameObject.AddComponent<MasterSuicideOnTimer>();
            timer.lifeTimer = (originalBody.isChampion || originalBody.isBoss) ? 30 * 2 : 30;
        }
    }
}
