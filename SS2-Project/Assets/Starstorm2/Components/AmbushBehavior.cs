using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2
{
    [RequireComponent(typeof(CombatDirector))]
    public class AmbushBehavior : NetworkBehaviour
    {
        [InitDuringStartupPhase(GameInitPhase.PostProgressBar)]
        private static void Init()
        {
            SS2Log.Info("Initializing AmbushBehavior");
            SceneDirector.onPrePopulateMonstersSceneServer += OnPrePopulateMonstersSceneServer;
            TeleporterInteraction.onTeleporterBeginChargingGlobal += OnTeleporterBeginCharging;
        }
        [ConCommand(commandName = "ambush_debug", flags = (ConVarFlags.Cheat), helpText = "Toggle Ambush debugging.")]
        private static void CCToggleDebug(ConCommandArgs args)
        {
            debug = !debug;
            Debug.Log("Ambush debugging is now " + (debug ? "ON" : "OFF"));
        }
        private static void OnPrePopulateMonstersSceneServer(SceneDirector sceneDirector)
        {
            foreach (var ambushBehavior in InstanceTracker.GetInstancesList<AmbushBehavior>())
            {
                sceneDirector.ReduceMonsterCredits(Mathf.RoundToInt(ambushBehavior.totalMonsterCredit * sceneDirectorMonsterCreditSubtractionCoefficient));
            }
        }

        private static void OnTeleporterBeginCharging(TeleporterInteraction teleporterInteraction)
        {
            if (NetworkServer.active)
            {
                foreach (var ambushBehavior in InstanceTracker.GetInstancesList<AmbushBehavior>())
                {
                    ambushBehavior.SetActive(false);
                }
            }
        }

        private static bool debug;
        public static float sceneDirectorMonsterCreditSubtractionCoefficient = 0.75f;
        private static float maxRage = 100f;

        public float radius = 50f;
        public Transform debugIndicator;

        [Header("Rage Values")]
        public float passiveRageGain = 0.5f;
        public float rageGainAtEdge = 6.67f;
        public float rageGainAtCenter = 33f;
        public float rageOnKill = 33f;
        public float rageOnInteract = 75f;

        [Header("Large Monsters")]
        public CombatDirector largeMonsterDirector;
        public int baseLargeMonsterCredit = 75;
        public int minLargeMonsters = 1;
        public int maxLargeMonsters = 4;
        private float largeMonsterCredit
        {
            get
            {
                return baseLargeMonsterCredit * Stage.instance.entryDifficultyCoefficient;
            }
        }
        [Header("Small Monsters")]
        public CombatDirector smallMonsterDirector;
        public int baseSmallMonsterCredit = 75;
        public int minSmallMonsters = 5;
        public int maxSmallMonsters = 9;
        private float smallMonsterCredit
        {
            get
            {
                return baseSmallMonsterCredit * Stage.instance.entryDifficultyCoefficient;
            }
        }
        private float totalMonsterCredit
        {
            get
            {
                return smallMonsterCredit + largeMonsterCredit;
            }
        }

        private float currentRage;
        private CharacterBody currentTarget;

        private DirectorCard chosenSmallCard;
        private DirectorCard chosenLargeCard;

        public GameObject spawnPositionEffectPrefab;
        public GameObject activatorEffectPrefab;

        private Xoroshiro128Plus rng;
        private bool active = true;
        // {You have been ambushed by }{___}{___ and ___}{___, ___, and ___}

        
        
        private void Awake()
        {
            InstanceTracker.Add(this);

            if (NetworkServer.active)
            {
                largeMonsterDirector.enabled = false;
                smallMonsterDirector.enabled = false;
                rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);
            }
            
        }

        private void Start()
        {
            if (NetworkServer.active)
            {
                chosenSmallCard = SelectMonsterCard(smallMonsterCredit, maxSmallMonsters, minSmallMonsters);
                chosenLargeCard = SelectMonsterCard(largeMonsterCredit, maxLargeMonsters, minLargeMonsters);
            }
        }
        private void OnDestroy()
        {
            InstanceTracker.Remove(this);
        }
        private void OnEnable()
        {
            GlobalEventManager.OnInteractionsGlobal += OnInteractionGlobal;
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
        }
        private void OnDisable()
        {
            GlobalEventManager.OnInteractionsGlobal -= OnInteractionGlobal;
            GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;
        }

        
        public void SetActive(bool newActive)
        {
            active = newActive;
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                if (active)
                {
                    float deltaTime = Time.fixedDeltaTime;
                    AddRage(passiveRageGain * deltaTime);
                    bool anyPlayersInRadius = CheckPlayersInRadius(deltaTime);

                    if (anyPlayersInRadius && currentTarget != null && currentRage >= maxRage)
                    {
                        AmbushActivation(currentTarget);
                    }
                }
            }

            if (debugIndicator)
            {
                debugIndicator.localScale = Vector3.one * radius;
                debugIndicator.gameObject.SetActive(debug && active);
            }
        }

        private void AddRage(float rage)
        {
            currentRage = Mathf.Clamp(currentRage + rage, 0f, maxRage);
        }
        private bool IsPointInRadius(Vector3 position)
        {
            Vector3 between = transform.position - position;
            return between.sqrMagnitude <= (radius * radius);
        }
        private bool CheckPlayersInRadius(float deltaTime)
        {
            var playerTeamMembers = TeamComponent.GetTeamMembers(TeamIndex.Player);
            CharacterBody closestPlayer = null;
            float lowestDistance = Mathf.Infinity;
            bool anyPlayersInRadius = false;
            foreach (TeamComponent member in playerTeamMembers)
            {
                var playerBody = member.body;
                if (playerBody && playerBody.isPlayerControlled && IsPointInRadius(playerBody.footPosition))
                {
                    anyPlayersInRadius = true;

                    float distance = (transform.position - playerBody.footPosition).magnitude;
                    if (lowestDistance > distance)
                    {
                        lowestDistance = distance;
                        closestPlayer = member.body;
                    }
                }
            }

            if (anyPlayersInRadius)
            {
                float t = Mathf.Clamp01(lowestDistance / radius);
                float rageGain = Mathf.Lerp(rageGainAtCenter, rageGainAtEdge, t);
                AddRage(rageGain * deltaTime);
            }

            if (currentTarget == null || IsPointInRadius(currentTarget.footPosition) == false)
            {
                currentTarget = closestPlayer;
            }

            return anyPlayersInRadius;
        }
        private void OnInteractionGlobal(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            if (!active)
            {
                return;
            }

            if (interactor.TryGetComponent(out CharacterBody body) && IsPointInRadius(body.footPosition))
            {
                currentTarget = body;
                AddRage(rageOnInteract);
            }
        }
        private void OnCharacterDeathGlobal(DamageReport damageReport)
        {
            if (!active)
            {
                return;
            }

            // we dont really care about team. things dying = player is probably doing something
            if (damageReport.attackerBody && IsPointInRadius(damageReport.attackerBody.footPosition))
            {
                currentTarget = damageReport.attackerBody;
                AddRage(rageOnKill);
            }
        }

        private DirectorCard SelectMonsterCard(float monsterCredit, int maxMonsters, int minMonsters)
        {
            var weightedSelection = Util.CreateReasonableDirectorCardSpawnList(monsterCredit, maxMonsters, minMonsters);
            if (weightedSelection.Count == 0)
            {
                return null;
            }
            return weightedSelection.Evaluate(rng.nextNormalizedFloat);
        }

        [Server]
        private void AmbushActivation(CharacterBody activator)
        {
            SetActive(false);

            //largeMonsterDirector.currentSpawnTarget = activator.gameObject; // large in middle, small on player
            smallMonsterDirector.currentSpawnTarget = activator ? activator.gameObject : base.gameObject;

            ActivateCombatDirector(largeMonsterDirector, largeMonsterCredit, chosenLargeCard);
            ActivateCombatDirector(smallMonsterDirector, smallMonsterCredit, chosenSmallCard);

            var largeMaster = chosenLargeCard.GetSpawnCard().prefab?.GetComponent<CharacterMaster>();
            CharacterBody largeBody = null;
            if (largeMaster)
            {
                largeBody = largeMaster.bodyPrefab?.GetComponent<CharacterBody>();
            }
            var smallMaster = chosenSmallCard.GetSpawnCard().prefab?.GetComponent<CharacterMaster>();
            CharacterBody smallBody = null;
            if (smallMaster)
            {
                smallBody = smallMaster.bodyPrefab?.GetComponent<CharacterBody>();
            }

            bool single = smallBody == null || largeBody == null || smallBody.bodyIndex == largeBody.bodyIndex;
            if (single)
            {
                CharacterBody singleBody = smallBody ?? largeBody;
                if (singleBody)
                {
                    Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                    {
                        subjectAsCharacterBody = activator,
                        baseToken = "SS2_AMBUSH_MESSAGE_SINGLE",
                        paramTokens = new string[]
                        {
                            singleBody.baseNameToken
                        }
                    });
                }
            }
            else
            {
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = activator,
                    baseToken = "SS2_AMBUSH_MESSAGE_DOUBLE",
                    paramTokens = new string[]
                    {
                        smallBody.baseNameToken,
                        largeBody.baseNameToken,
                    }
                });
            }

            EffectManager.SpawnEffect(spawnPositionEffectPrefab, new EffectData
            {
                origin = transform.position,
                rotation = Quaternion.identity,
                scale = 1f,
            }, true);

            var effectData = new EffectData
            {
                origin = transform.position,
                rotation = Quaternion.identity,
                scale = 1f,
            };
            effectData.SetNetworkedObjectReference(activator.gameObject);
            EffectManager.SpawnEffect(activatorEffectPrefab, effectData, true);

        }

        private void ActivateCombatDirector(CombatDirector director, float monsterCredit, DirectorCard directorCard)
        {
            director.enabled = true;
            director.monsterCredit += monsterCredit;
            director.OverrideCurrentMonsterCard(directorCard);
            director.monsterSpawnTimer = 0;
        }

    }
}
