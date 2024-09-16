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
using RoR2.Items;

namespace SS2.Artifacts
{
    public sealed class Cognation : SS2Artifact
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ArtifactAssetCollection>("acCognation", SS2Bundle.Artifacts);

        [RiskOfOptionsConfigureField(SS2Config.ID_ARTIFACT, configDescOverride = "Whether or not cognation ghosts inherit all items from the original body")]
        public static bool inheritInventory = true;

        private static Material ghostMaterial;

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
            ghostMaterial = AssetCollection.FindAsset<Material>("matCognation");
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
            ghostSummon.inventoryToCopy = inheritInventory ? master.inventory : null;

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

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Cognation;

            private CharacterModel model;

            private void Start()
            {
                if (body.teamComponent.teamIndex == TeamIndex.Player)
                {
                    body.inventory.RemoveItem(SS2Content.Items.Cognation, stack);
                    Destroy(this);
                }

                if (body.inventory.GetItemCount(SS2Content.Items.TerminationHelper) > 0)
                {
                    body.inventory.RemoveItem(SS2Content.Items.TerminationHelper);
                }

                body.baseMaxHealth *= 3;
                body.baseMoveSpeed *= 1.25f;
                body.baseAttackSpeed *= 1.25f;
                body.baseDamage *= 0.9f;
                body.baseArmor -= 25;
                body.PerformAutoCalculateLevelStats();
                body.RecalculateStats();

                ModelLocator modelLoc = body.modelLocator;
                if (modelLoc)
                {
                    Transform modelTransform = modelLoc.modelTransform;
                    if (modelTransform)
                    {
                        model = modelTransform.GetComponent<CharacterModel>();
                    }
                }

                if (model)
                {
                    //SS2Log.Info("swapping shader");
                    ModifyCharacterModel();
                }
            }

            private void ModifyCharacterModel()
            {
                for (int i = 0; i < model.baseRendererInfos.Length; i++)
                {
                    var mat = model.baseRendererInfos[i].defaultMaterial;
                    if (mat.shader.name.StartsWith("Hopoo Games/Deferred"))
                    {
                        //SS2Log.Info("swapping shader real " +mat.shader.name + " | " + ghostMaterial + "
                        mat = ghostMaterial;
                        model.baseRendererInfos[i].defaultMaterial = mat;
                    }
                }
            }

            private void OnDestroy()
            {
                if (model)
                {
                    var modelDestroyOnUnseen = model.GetComponent<DestroyOnUnseen>() ?? model.gameObject.AddComponent<DestroyOnUnseen>();
                    modelDestroyOnUnseen.cull = true;
                }
            }
        }
    }
}
