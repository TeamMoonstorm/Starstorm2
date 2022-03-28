/*using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.Components;
using Moonstorm.Starstorm2.ScriptableObjects;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
using MonsterSpawnDistance = RoR2.DirectorCore.MonsterSpawnDistance;

namespace EntityStates.Events
{
    //TODO: Post processing
    public class GenericNemesisEvent : EventState
    {
        public static GameObject musicOverridePrefab;
        public static MusicTrackDef canticumVitaeA;
        public static MusicTrackDef canticumVitaeB;

        [SerializeField]
        //public GameObject effectPrefab;

        public static GameObject encounterPrefab;
        [SerializeField]
        public NemesisSpawnCard spawnCard;
        [SerializeField]
        public string spawnDistanceString;

        public static float fadeDuration = 7f;

        public override bool overrideTimer => true;

        /// <summary>This is gotten from the string</summary>
        private MonsterSpawnDistance spawnDistance;
        private Xoroshiro128Plus rng;
        private MusicTrackOverride musicTrack;

        //private EventStateEffect eventStateEffect;
        //private GameObject effectInstance;

        public GameObject chosenPlayer;

        public CharacterBody nemesisBossBody;

        public static event Action<CharacterBody> onNemesisDefeatedGlobal;

        public override void OnEnter()
        {
            base.OnEnter();

            rng = Run.instance.spawnRng;
            if (!Enum.TryParse(spawnDistanceString, out spawnDistance))
                spawnDistance = MonsterSpawnDistance.Standard;
            FindSpawnTarget();

            if (musicOverridePrefab && canticumVitaeA && canticumVitaeB)
            {
                musicTrack = GameObject.Instantiate(musicOverridePrefab).GetComponent<MusicTrackOverride>();
                musicTrack.track = canticumVitaeA;
            }

            /*if (effectPrefab)
            {
                effectInstance = UnityEngine.Object.Instantiate(effectPrefab);
                eventStateEffect = effectInstance.GetComponent<EventStateEffect>();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }


        public override void StartEvent()
        {
            base.StartEvent();
            if (musicOverridePrefab)
                musicTrack.track = canticumVitaeB;
            /*if (eventStateEffect)
                eventStateEffect.OnEffectStart();
            if (NetworkServer.active)
                SpawnNemesisServer();
        }


        private void FindSpawnTarget()
        {
            if (NetworkServer.active)
            {
                ReadOnlyCollection<PlayerCharacterMasterController> instances = PlayerCharacterMasterController.instances;
                List<PlayerCharacterMasterController> list = new List<PlayerCharacterMasterController>();
                foreach (PlayerCharacterMasterController playerCharacterMasterController in instances)
                {
                    if (playerCharacterMasterController.master.hasBody)
                        list.Add(playerCharacterMasterController);
                }
                if (list.Count > 0)
                    chosenPlayer = rng.NextElementUniform(list).body.gameObject;
            }
        }


        public virtual void SpawnNemesisServer()
        {
            if (!this.spawnCard)
                return;

            var spawnCard = UnityEngine.Object.Instantiate(this.spawnCard);
            Transform spawnTarget = null;
            MonsterSpawnDistance distance = spawnDistance;

            if (chosenPlayer)
                spawnTarget = chosenPlayer.GetComponent<CharacterBody>().coreTransform;
            if (TeleporterInteraction.instance && TeleporterInteraction.instance.currentState.GetType() != typeof(TeleporterInteraction.ChargingState) || !chosenPlayer)
            {
                spawnTarget = TeleporterInteraction.instance.transform;
                distance = MonsterSpawnDistance.Close;
            }
            if (!spawnTarget)
            {
                LogCore.LogD("Unable to spawn Nemesis Event. Returning.");
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
            DirectorSpawnRequest directorSpawnRequest2 = directorSpawnRequest;
            directorSpawnRequest2.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest2.onSpawnedServer, new Action<SpawnCard.SpawnResult>(delegate (SpawnCard.SpawnResult result)

            {
                if (!combatSquad)
                    combatSquad = UnityEngine.Object.Instantiate(encounterPrefab).GetComponent<CombatSquad>();
                CharacterMaster master = result.spawnedInstance.GetComponent<CharacterMaster>();
                master.gameObject.AddComponent<NemesisResistances>();
                nemesisBossBody = master.GetBody();
                combatSquad.AddMember(master);
                master.onBodyDeath.AddListener(OnBodyDeath);
            }));
            DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            if (combatSquad)
            {
                combatSquad.GetComponent<TeamFilter>().defaultTeam = TeamIndex.Monster;
                NetworkServer.Spawn(combatSquad.gameObject);
            }
            UnityEngine.Object.Destroy(spawnCard);
        }



        public virtual void OnBodyDeath()
        {
            onNemesisDefeatedGlobal?.Invoke(nemesisBossBody);
            outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();
            if (musicTrack)
                Destroy(musicTrack.gameObject);
            /*if (eventStateEffect)
                eventStateEffect.OnEndingStart(fadeDuration);
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
*/