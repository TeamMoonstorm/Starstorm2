
using SS2;
using SS2.Components;
using SS2.ScriptableObjects;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
using MonsterSpawnDistance = RoR2.DirectorCore.MonsterSpawnDistance;
using MSU;

namespace EntityStates.Events
{
    public class GenericNemesisEvent : EntityState
    {
        [SerializeField, Tooltip("The minimum duration for this EventState")]
        public float minDuration = 30f;
        [SerializeField, Tooltip("The maximum duration for this EventState")]
        public float maxDuration = 90f;
        [SerializeField, Tooltip("The amount of time before the event officially starts")]
        public float warningDur = 10f;

        public static GameObject musicOverridePrefab;
        public bool HasWarned { get; protected set; }

        [SerializeField]
        public MusicTrackDef introTrack;
        [SerializeField]
        public MusicTrackDef mainTrack;
        [SerializeField]
        public MusicTrackDef outroTrack;
        public static GameObject encounterPrefab;
        [SerializeField]
        public NemesisSpawnCard spawnCard;

        public static float fadeDuration = 7f;

        private Xoroshiro128Plus rng;
        private MusicTrackOverride musicTrack;

        public GameObject chosenPlayer;

        public CharacterBody nemesisBossBody;

        public static event Action<CharacterBody> onNemesisDefeatedGlobal;

        private static int minimumLevel = 60;

        private bool hasSpawned;
        public override void OnEnter()
        {
            base.OnEnter();     
            rng = Run.instance.spawnRng;
            if (NetworkServer.active)
            {
                this.spawnCard = EventDirector.instance.availableNemesisSpawnCards.Evaluate(EventDirector.instance.rng.nextNormalizedFloat);
                FindSpawnTarget();
            }
                
            if (musicOverridePrefab && introTrack && mainTrack)
            {
                musicTrack = GameObject.Instantiate(musicOverridePrefab, Stage.instance.transform).GetComponent<MusicTrackOverride>();
                musicTrack.track = introTrack;
            }

            GameplayEventTextController.EventTextRequest request = new GameplayEventTextController.EventTextRequest
            {
                eventToken = "SS2_EVENT_GENERICNEMESIS_START",
                eventColor = new Color(.67f, .168f, .168f),
                textDuration = 7,
            };
            GameplayEventTextController.instance.EnqueueNewTextRequest(request, false);

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!HasWarned && fixedAge >= warningDur)
                StartEvent();
        }
        public void StartEvent()
        {
            if (musicOverridePrefab)
                musicTrack.track = mainTrack;
            if (NetworkServer.active && !hasSpawned)
            {
                SpawnNemesisBoss();
                hasSpawned = true;
            }
        }
        private void FindSpawnTarget()
        {
            ReadOnlyCollection<PlayerCharacterMasterController> instances = PlayerCharacterMasterController.instances;
            List<PlayerCharacterMasterController> list = new List<PlayerCharacterMasterController>();
            foreach (PlayerCharacterMasterController playerCharacterMasterController in instances)
            {
                if (playerCharacterMasterController.master.hasBody)
                    list.Add(playerCharacterMasterController);
            }
            if (list.Count > 0)
            {
                var charMaster = rng.NextElementUniform(list);
                if (charMaster && charMaster.body)
                {
                    chosenPlayer = charMaster.body.gameObject;
                }
            }
        }


        public virtual void SpawnNemesisBoss()
        {
            if (!this.spawnCard)
                return;

            var spawnCard = UnityEngine.Object.Instantiate(this.spawnCard);
            Transform spawnTarget = null;
            MonsterSpawnDistance distance = MonsterSpawnDistance.Far;

            if (chosenPlayer)
                spawnTarget = chosenPlayer.GetComponent<CharacterBody>().coreTransform;
            if (TeleporterInteraction.instance)
            {
                spawnTarget = TeleporterInteraction.instance.transform;
                distance = MonsterSpawnDistance.Close;
            }
            if (!spawnTarget)
            {
                SS2Log.Error("Unable to spawn Nemesis Event. Returning.");
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
            directorSpawnRequest.onSpawnedServer += OnBossSpawned;
            DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            UnityEngine.Object.Destroy(spawnCard);
        }

        private void OnBossSpawned(SpawnCard.SpawnResult spawnResult)
        {          
            CombatSquad combatSquad = UnityEngine.Object.Instantiate(encounterPrefab).GetComponent<CombatSquad>();
            CharacterMaster master = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
            //master.gameObject.AddComponent<NemesisResistances>();
            nemesisBossBody = master.GetBody();
            master.onBodyDeath.AddListener(OnBodyDeath);
            master.onBodyStart += (body) =>
            {
                FriendManager.instance.RpcSetupNemBoss(body.gameObject, spawnCard.visualEffect?.name); // lol. lmao
            };
            int itemCount = Run.instance.stageClearCount * Mathf.Max(Run.instance.loopClearCount, 1);
            if (EtherealBehavior.instance.runIsEthereal) itemCount *= 2;
            master.inventory.GiveItem(SS2Content.Items.MaxHealthPerMinute, itemCount);
            //master.inventory.GiveItem(RoR2Content.Items.AdaptiveArmor);
            master.inventory.GiveItem(RoR2Content.Items.UseAmbientLevel);
            int level = Mathf.FloorToInt(Run.instance.ambientLevel);
            if (level < minimumLevel)
            {
                master.inventory.GiveItem(RoR2Content.Items.LevelBonus, minimumLevel - level);
            }
            // remove level cap
            else if (Run.instance.ambientLevel >= Run.ambientLevelCap)
            {
                int extraLevels = Mathf.FloorToInt(SS2Util.AmbientLevelUncapped()) - Run.instance.ambientLevelFloor;
                master.inventory.GiveItem(RoR2Content.Items.LevelBonus, extraLevels);
            }
            GameObject target = null;
            foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController.instances)
            {
                if (pcmc && pcmc.master && pcmc.master.inventory.GetItemCount(SS2Content.Items.VoidRock) > 0)
                {
                    target = pcmc.master.GetBodyObject();
                    break;
                }

            }
            RoR2.CharacterAI.BaseAI ai = master.GetComponent<RoR2.CharacterAI.BaseAI>();
            if (ai)
                ai.currentEnemy.gameObject = target;
            new FriendManager.SyncBaseStats(nemesisBossBody).Send(R2API.Networking.NetworkDestination.Clients);
            combatSquad.AddMember(master);
            combatSquad.GetComponent<TeamFilter>().defaultTeam = TeamIndex.Monster;
            NetworkServer.Spawn(combatSquad.gameObject);
        }

        public virtual void OnBodyDeath()
        {
            GameplayEventTextController.EventTextRequest request = new GameplayEventTextController.EventTextRequest
            {
                eventToken = "SS2_EVENT_GENERICNEMESIS_END",
                eventColor = new Color(.67f, .168f, .168f),
                textDuration = 7,
            };
            GameplayEventTextController.instance.EnqueueNewTextRequest(request, false);           
            if (musicOverridePrefab)
                musicTrack.track = outroTrack;
            outer.SetNextState(new IdleRestOfStage());
            onNemesisDefeatedGlobal?.Invoke(nemesisBossBody);
        }

        public override void OnExit()
        {
            base.OnExit();        
            // need to do outro here instead of destroying
            if (musicTrack)
                Destroy(musicTrack.gameObject);
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(chosenPlayer);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            chosenPlayer = reader.ReadGameObject();
        }
    }

}
