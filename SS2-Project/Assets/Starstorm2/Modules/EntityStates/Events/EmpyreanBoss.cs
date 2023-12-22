using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.ScriptableObjects;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.DirectorCore;

namespace EntityStates.Events
{
    public class EmpyreanBoss : GenericBossEventState
    {
        public static NemesisSpawnCard refSpawnCard;
        private Xoroshiro128Plus rng;

        public override void OnEnter()
        {
            rng = Run.instance.spawnRng;

            base.OnEnter();
        }
        public override void SpawnBoss()
        {
            GameObject director = GameObject.Find("Director"); //fuck you
            if (!director)
                return;

            CombatDirector combatDirector = director.GetComponent<CombatDirector>();
            if (!combatDirector)
                return;

            DirectorCard chosenMonsterCard = combatDirector.SelectMonsterCardForCombatShrine(20f * Run.instance.difficultyCoefficient);
            GameObject chosenMonsterPrefab = chosenMonsterCard.spawnCard.prefab;
            CharacterBody monsterBody = chosenMonsterPrefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<CharacterBody>();

            if (!this.characterSpawnCard)
                return;

            Debug.Log(chosenMonsterCard.spawnCard.prefab.name);

            var spawnCard = UnityEngine.Object.Instantiate(chosenMonsterCard.spawnCard);

            Transform spawnTarget = null;
            MonsterSpawnDistance distance = MonsterSpawnDistance.Standard;

            if (chosenPlayer)
                spawnTarget = chosenPlayer.GetComponent<CharacterBody>().coreTransform;
            if (TeleporterInteraction.instance)
            {
                spawnTarget = TeleporterInteraction.instance.transform;
                distance = MonsterSpawnDistance.Close;
            }
            if (!spawnTarget)
            {
                SS2Log.Error("Unable to spawn boss. Returning.");
                return;
            }

            DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
            {
                spawnOnTarget = spawnTarget,
                placementMode = DirectorPlacementRule.PlacementMode.NearestNode
            };
            DirectorCore.GetMonsterSpawnDistance(distance, out directorPlacementRule.minDistance, out directorPlacementRule.maxDistance);


            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, directorPlacementRule, rng);
            directorSpawnRequest.teamIndexOverride = new TeamIndex?(TeamIndex.Monster);
            directorSpawnRequest.ignoreTeamMemberLimit = true;

            CombatSquad combatSquad = null;
            directorSpawnRequest.onSpawnedServer = (spawnResult) =>                      
            {
                if (!combatSquad)
                    combatSquad = UnityEngine.Object.Instantiate(encounterPrefab).GetComponent<CombatSquad>();
                CharacterMaster master = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
                bossBody = master.GetBody();

                combatSquad.AddMember(master);
                master.onBodyDeath.AddListener(OnBodyDeath);
            };
            DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            if (combatSquad)
            {
                combatSquad.GetComponent<TeamFilter>().defaultTeam = TeamIndex.Monster;
                NetworkServer.Spawn(combatSquad.gameObject);

                foreach (CharacterMaster master in combatSquad.membersList)
                {
                    master.inventory.GiveItem(RoR2Content.Items.BoostHp, 250);
                    master.inventory.GiveItem(RoR2Content.Items.BoostDamage, 80);
                    master.inventory.SetEquipmentIndex(SS2Content.Equipments.AffixEmpyrean.equipmentIndex);
                }

            }
            UnityEngine.Object.Destroy(spawnCard);
        }
    }
}
