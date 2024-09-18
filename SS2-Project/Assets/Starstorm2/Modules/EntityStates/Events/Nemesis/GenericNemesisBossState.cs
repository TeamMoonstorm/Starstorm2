
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
    //TODO: Post processing
    //Does it need it? 
    public class GenericNemesisEvent : EntityState
    {
        [SerializeField, Tooltip("The minimum duration for this EventState")]
        public float minDuration = 30f;
        [SerializeField, Tooltip("The maximum duration for this EventState")]
        public float maxDuration = 90f;
        [SerializeField, Tooltip("The amount of time before the event officially starts")]
        public float warningDur = 10f;

        public static GameObject musicOverridePrefab;

        /// <summary>
        /// Wether or not this event should end on a timer
        /// </summary>
        public virtual bool OverrideTimer => false;

        /// <summary>
        /// Wether this event is already past the "Warned" phase
        /// </summary>
        public bool HasWarned { get; protected set; }

        /// <summary>
        /// The actual duration of the event
        /// <para>Duration is taken by remaping the current <see cref="DiffScalingValue"/> capping the in value with min 1 and max 3.5, and keeping the result between min <see cref="minDuration"/> and max <see cref="maxDuration"/></para>
        /// </summary>
        public float DiffScaledDuration { get; protected set; }

        /// <summary>
        /// The current run's difficulty scaling value, taken from the difficultyDef.
        /// </summary>
        public float DiffScalingValue { get; protected set; }

        /// <summary>
        /// The total duration of the event, calculated from the sum of <see cref="DiffScaledDuration"/> and <see cref="warningDur"/>
        /// </summary>
        public float TotalDuration { get; protected set; }



        [SerializeField]
        public MusicTrackDef introTrack;
        [SerializeField]
        public MusicTrackDef mainTrack;
        [SerializeField]
        public MusicTrackDef outroTrack;

        //[SerializeField]
        //public GameObject effectPrefab;

        public static GameObject encounterPrefab;
        [SerializeField]
        public NemesisSpawnCard spawnCard;
        [SerializeField]
        public string spawnDistanceString;

        public static float fadeDuration = 7f;

        /// <summary>This is gotten from the string</summary>
        private MonsterSpawnDistance spawnDistance;
        private Xoroshiro128Plus rng;
        private MusicTrackOverride musicTrack;

        //private EventStateEffect eventStateEffect;
        //private GameObject effectInstance;

        public GameObject chosenPlayer;

        public CharacterBody nemesisBossBody;

        public static event Action<CharacterBody> onNemesisDefeatedGlobal;

        private static int minimumLevel = 30;

        private bool hasSpawned;
        public override void OnEnter()
        {
            base.OnEnter();

            GameplayEventTextController.EventTextRequest request = new GameplayEventTextController.EventTextRequest
            {
                eventToken = "SS2_EVENT_GENERICNEMESIS_START",
                eventColor = new Color(.67f, .168f, .168f),
                textDuration = 7,
            };
            GameplayEventTextController.instance.EnqueueNewTextRequest(request, false);

            rng = Run.instance.spawnRng;
            if (!Enum.TryParse(spawnDistanceString, out spawnDistance))
                spawnDistance = MonsterSpawnDistance.Standard;

            if (NetworkServer.active) //Spawn target gets serialized and deserialized later by the network state machine
                FindSpawnTarget();

            if (musicOverridePrefab && introTrack && mainTrack)
            {
                musicTrack = GameObject.Instantiate(musicOverridePrefab, Stage.instance.transform).GetComponent<MusicTrackOverride>();
                musicTrack.track = introTrack;
            }
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
            MonsterSpawnDistance distance = spawnDistance;

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
            DirectorCore.GetMonsterSpawnDistance(spawnDistance, out directorPlacementRule.minDistance, out directorPlacementRule.maxDistance);


            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, directorPlacementRule, rng);
            directorSpawnRequest.teamIndexOverride = new TeamIndex?(TeamIndex.Monster);
            directorSpawnRequest.ignoreTeamMemberLimit = true;

            CombatSquad combatSquad = null;
            directorSpawnRequest.onSpawnedServer = (spawnResult) =>
            {
                if (!combatSquad)
                    combatSquad = UnityEngine.Object.Instantiate(encounterPrefab).GetComponent<CombatSquad>();
                CharacterMaster master = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
                //master.gameObject.AddComponent<NemesisResistances>();
                nemesisBossBody = master.GetBody();

                master.onBodyStart += (body) =>
                {
                    body.gameObject.AddComponent<NemesisResistances>();
                    AddHurtboxForBody(body);
                };

                new NemesisSpawnCard.SyncBaseStats(nemesisBossBody).Send(R2API.Networking.NetworkDestination.Clients);
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
                    master.inventory.GiveItem(RoR2Content.Items.AdaptiveArmor);
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
                }
            }
            UnityEngine.Object.Destroy(spawnCard);
        }


        public void AddHurtboxForBody(CharacterBody body)
        {
            if (body.mainHurtBox)
            {
                CapsuleCollider capsuleCollider = body.mainHurtBox.GetComponent<CapsuleCollider>();
                if (capsuleCollider)
                {
                    capsuleCollider.height = 4f;
                    capsuleCollider.radius = 4f;
                }

            }
        }
        public virtual void OnBodyDeath()
        {
            onNemesisDefeatedGlobal?.Invoke(nemesisBossBody);
            // we dont want to go back to main state, since we only want one nemesis boss per stage
            //outer.SetNextStateToMain();
            if (musicOverridePrefab)
                musicTrack.track = outroTrack;
            outer.SetNextState(new IdleRestOfStage());
        }

        public override void OnExit()
        {
            base.OnExit();

            GameplayEventTextController.EventTextRequest request = new GameplayEventTextController.EventTextRequest
            {
                eventToken = "SS2_EVENT_GENERICNEMESIS_END",
                eventColor = new Color(.67f, .168f, .168f),
                textDuration = 7,
            };
            GameplayEventTextController.instance.EnqueueNewTextRequest(request, false);

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
