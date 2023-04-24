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
        public override ArtifactCode ArtifactCode { get; } = SS2Assets.LoadAsset<ArtifactCode>("Cognation", SS2Bundle.Artifacts);

        [ConfigurableField(ConfigDesc = "Whether or not cognation ghosts inherit all items from the original body")]
        public static bool InheritInventory = true;
        public static ReadOnlyCollection<MasterCatalog.MasterIndex> BlacklistedMasterIndices { get; private set; }
        private static readonly List<MasterCatalog.MasterIndex> blacklistedMasterIndices = new List<MasterCatalog.MasterIndex>();
        public static readonly List<string> blacklistedMasters = new List<string>
        {
            "ArtifactShell",
            "BrotherHurt",
            "VoidInfestor",
        };

        [SystemInitializer(typeof(MasterCatalog))]
        private static void SystemInit()
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
                SS2Log.Debug($"Master PRefab {prefab} is already blacklisted.");
                return;
            }

            blacklistedMasterIndices.Add(masterIndex);
            BlacklistedMasterIndices = new ReadOnlyCollection<MasterCatalog.MasterIndex>(blacklistedMasterIndices);
        }

        //Swuff, I Hate You 3000
        public override void Initialize()
        {
            SceneManager.sceneLoaded += AddCode;
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

        public override void OnArtifactDisabled()
        {
            //IL.RoR2.CharacterMaster.OnBodyDeath -= PreventDeath;
            GlobalEventManager.onCharacterDeathGlobal -= SpawnCognationGhost;

        }

        public override void OnArtifactEnabled()
        {
            //IL.RoR2.CharacterMaster.OnBodyDeath += PreventDeath;
            GlobalEventManager.onCharacterDeathGlobal += SpawnCognationGhost;
        }

        private void SpawnCognationGhost(DamageReport report)
        {
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

        private bool CanBecomeGhost(CharacterMaster victimMaster)
        {
            bool isMonster = victimMaster.teamIndex != TeamIndex.Player;
            bool hasBody = victimMaster.hasBody;
            bool notGhost = victimMaster.inventory.GetItemCount(SS2Content.Items.Cognation) == 0;
            bool notBlacklisted = !BlacklistedMasterIndices.Contains(victimMaster.masterIndex);
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
