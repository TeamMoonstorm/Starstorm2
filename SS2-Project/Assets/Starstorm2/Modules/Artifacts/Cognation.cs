using MSU.Config;
using MSU;
using R2API.ScriptableObjects;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;
using RoR2.ContentManagement;
namespace SS2.Artifacts
{
    public sealed class Cognation : SS2Artifact
    {
        public override NullableRef<ArtifactCode> ArtifactCode => _artifactCode;
        private ArtifactCode _artifactCode;
        public override ArtifactDef ArtifactDef => _artifactDef;
        private ArtifactDef _artifactDef;

        [RiskOfOptionsConfigureField(SS2Config.ID_ARTIFACT, ConfigDescOverride = "Whether or not cognation ghosts inherit all items from the original body")]
        public static bool InheritInventory = true;

        public static ReadOnlyCollection<MasterCatalog.MasterIndex> BlacklistedMasterIndices { get; private set; }
        private static readonly HashSet<string> blacklistedMasters = new HashSet<string>
        {
            "ArtifactShell",
            "BrotherHurt",
            "VoidInfestor",
        };

        public static void AddBlacklistedMaster(string masterName)
        {
            blacklistedMasters.Add(masterName);
            UpdateBlacklistedMasters();
        }

        public override void Initialize()
        {
            //For masters since it doesnt have a resource availability, hotpoo!!!!
            RoR2Application.onLoad += UpdateBlacklistedMasters;
        }

        private static void UpdateBlacklistedMasters()
        {
            List<MasterCatalog.MasterIndex> newBlacklist = new List<MasterCatalog.MasterIndex>();
            foreach (var masterName in blacklistedMasters)
            {
                var index = MasterCatalog.FindMasterIndex(masterName);
                if (index == MasterCatalog.MasterIndex.none)
                    continue;

                newBlacklist.Add(index);
            }
            BlacklistedMasterIndices = new ReadOnlyCollection<MasterCatalog.MasterIndex>(newBlacklist);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*ParallelAssetLoadCoroutineHelper helper = new ParallelAssetLoadCoroutineHelper();
            helper.AddAssetToLoad<ArtifactDef>("Cognation", SS2Bundle.Artifacts);
            helper.AddAssetToLoad<ArtifactCode>("CognationCode", SS2Bundle.Artifacts);

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            _artifactCode = helper.GetLoadedAsset<ArtifactCode>("CognationCode");
            _artifactDef = helper.GetLoadedAsset<ArtifactDef>("Cognation");*/
            yield break;
        }

        public override void OnArtifactDisabled()
        {
            GlobalEventManager.onCharacterDeathGlobal -= SpawnCognationGhost;
        }

        public override void OnArtifactEnabled()
        {
            GlobalEventManager.onCharacterDeathGlobal += SpawnCognationGhost;
        }

        private void SpawnCognationGhost(DamageReport obj)
        {
            CharacterMaster victimMaster = obj.victimMaster;
            if (!victimMaster)
                return;

            if (!CanMasterBecomeGhost(victimMaster))
            {
                return;
            }

            var summon = PrepareMasterSummon(victimMaster);
            if (summon == null)
                return;

            SummonGhost(summon.Perform(), victimMaster);
        }

        private bool CanMasterBecomeGhost(CharacterMaster victimMaster)
        {
            if (victimMaster.teamIndex == TeamIndex.Player)
                return false;

            if (!victimMaster.hasBody)
                return false;

            if (victimMaster.inventory.GetItemCount(SS2Content.Items.Cognation) != 0)
                return false;

            if (BlacklistedMasterIndices.Contains(victimMaster.masterIndex))
                return false;

            if (victimMaster.inventory.currentEquipmentIndex == DLC1Content.Equipment.EliteVoidEquipment.equipmentIndex)
                return false;

            return true;
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
