using R2API.ScriptableObjects;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;

namespace Moonstorm.Starstorm2.Artifacts
{
    public sealed class Cognation : ArtifactBase
    {
        public override ArtifactDef ArtifactDef { get; } = SS2Assets.LoadAsset<ArtifactDef>("Cognation");
        public override ArtifactCode ArtifactCode { get; } = SS2Assets.LoadAsset<ArtifactCode>("Cognation");

        [ConfigurableField(ConfigDesc = "Wether or not cognation ghosts inherit all items from the original body")]
        public static bool InheritInventory = true;

        //Swuff, I Hate You 3000
        public override void Initialize()
        {
            SceneManager.sceneLoaded += AddCode;
        }

        private void AddCode(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name == "lobby")
            {
                var code = UObject.Instantiate(SS2Assets.LoadAsset<GameObject>("CognationCode"), Vector3.zero, Quaternion.identity);
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
        /*private void PreventDeath(MonoMod.Cil.ILContext il)
        {
            ILLabel illabel = null;
            ILCursor c = new ILCursor(il);
                c.GotoNext(MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdstr("PlayExtraLifeSFX"),
                    x => x.MatchLdcR4(1f),
                    x => x.MatchCall<MonoBehaviour>("Invoke"),
                    x => x.MatchBr(out illabel));

            if(illabel != null)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<CharacterMaster, bool>>(RespawnAsGhost);
                c.Emit(OpCodes.Brtrue, illabel);
            }    
        }

        private bool RespawnAsGhost(CharacterMaster master)
        {
            if(!master.GetComponent<CognationController>() && CanBecomeGhost(master))
            {
                master.gameObject.AddComponent<CognationController>();

                Vector3 deathPos = master.deathFootPosition;
                if(master.killedByUnsafeArea)
                {
                    deathPos = (TeleportHelper.FindSafeTeleportDestination(deathPos, master.bodyPrefab.GetComponent<CharacterBody>(), RoR2Application.rng)) ?? master.deathFootPosition;
                }
                var charBody = master.Respawn(deathPos, Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0));
                if (charBody)
                {
                    charBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, 1f);
                    if(master.bodyInstanceObject)
                    {
                        foreach(EntityStateMachine esm in master.bodyInstanceObject.GetComponents<EntityStateMachine>())
                        {
                            esm.initialStateType = esm.mainStateType;
                        }
                    }
                }
                return true;
            }
            return false;
        }*/

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
            bool flag1 = victimMaster.teamIndex != TeamIndex.Player;
            bool flag2 = victimMaster.hasBody;
            bool flag3 = victimMaster.inventory.GetItemCount(SS2Content.Items.Cognation) == 0;
            bool flag4 = !(victimMaster.name.Contains("ArtifactShell") || victimMaster.name.Contains("BrotherHurt"));

            return flag1 && flag2 && flag3 && flag4;
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
