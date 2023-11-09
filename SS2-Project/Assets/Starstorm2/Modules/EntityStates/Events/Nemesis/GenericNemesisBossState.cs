using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.Components;
using Moonstorm.Starstorm2.ScriptableObjects;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
using MonsterSpawnDistance = RoR2.DirectorCore.MonsterSpawnDistance;

namespace EntityStates.Events
{
    //TODO: Post processing
    //Does it need it? 
    public class GenericNemesisEvent : EventState
    {
        public static GameObject musicOverridePrefab;

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

        public override bool OverrideTimer => true;

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

            if(NetworkServer.active) //Spawn target gets serialized and deserialized later by the network state machine
                FindSpawnTarget();

            if (musicOverridePrefab && introTrack && mainTrack)
            {
                musicTrack = GameObject.Instantiate(musicOverridePrefab).GetComponent<MusicTrackOverride>();
                musicTrack.track = introTrack;
            }

            /*if (effectPrefab)
            {
                effectInstance = UnityEngine.Object.Instantiate(effectPrefab);
                eventStateEffect = effectInstance.GetComponent<EventStateEffect>();
            }*/
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }


        public override void StartEvent()
        {
            base.StartEvent();
            if (musicOverridePrefab)
                musicTrack.track = mainTrack;
            /*if (eventStateEffect)
                eventStateEffect.OnEffectStart();*/
            if(NetworkServer.active)
                SpawnNemesisBoss();
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
                if(charMaster && charMaster.body)
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
            }
            UnityEngine.Object.Destroy(spawnCard);
        }

        

        public virtual void OnBodyDeath()
        {
            onNemesisDefeatedGlobal?.Invoke(nemesisBossBody);
            // we dont want to go back to main state, since we only want one nemesis boss per stage
            //outer.SetNextStateToMain();
            outer.SetNextState(new IdleRestOfStage());
        }

        public override void OnExit()
        {
            base.OnExit();


            /*if (eventStateEffect)
                eventStateEffect.OnEndingStart(fadeDuration);*/

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
