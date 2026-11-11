using System;
using RoR2;
using RoR2.AsyncManagement;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Destructible
{
    public class EliteCrystalDeath : BaseState
    {
        public static Action<int> onEliteCrystalDestroyed;

        public static GameObject monsterEncounterPrefab;
        public static GameObject pickupPrefab;

        [SerializeField] public GameObject explosionEffectPrefab;
        [SerializeField] public GameObject monsterSpawnEffectPrefab;
        [SerializeField] public EliteDef eliteDef;

        private static float expMultiplier = 0.2f;
        private static float goldMultiplier = 0.5f;
        private static float combatDirectorMonsterCreditSubtractionCoefficient = 0.5f; // each monster spawn will be subtracted from both of the scene's combat directors, so this value will be "doubled"

        public int eventProgress = 25;

        public float baseMonsterCredit = 30f;
        public int maxMonsters = 3;

        public int maxPickups = 1;

        public override void OnEnter()
        {
            base.OnEnter();
            Explode();
        }

        public virtual EliteDef GetEliteDef()
        {
            return eliteDef;
        }
        public virtual int GetEventProgress()
        {
            return eventProgress;
        }

        private DirectorCard SelectMonsterCard(float monsterCredit, int maxMonsters, int minMonsters, Xoroshiro128Plus rng)
        {
            var args = new CombatShrineUtils.CreateReasonableDirectorCardSpawnListArgs
            {
                minimumToSpawn = minMonsters,
                maximumNumberToSpawnBeforeSkipping = maxMonsters,
                availableCredit = monsterCredit,
            };
            var weightedSelection = CombatShrineUtils.CreateReasonableDirectorCardSpawnList(args);
            if (weightedSelection.Count == 0)
            {
                return null;
            }
            return weightedSelection.Evaluate(rng.nextNormalizedFloat);
        }

        private void ActivateCombatDirector(CombatDirector director, float monsterCredit, DirectorCard directorCard)
        {
            director.spawnEffectPrefab = monsterSpawnEffectPrefab;
            director.ActiveEliteDefOverride = GetEliteDef();
            director.ActiveEliteDefOverrideCostMultiplier = 1;
            director.goldRewardCoefficient = goldMultiplier;
            director.expRewardCoefficient = expMultiplier;

            director.enabled = true;
            director.monsterCredit += monsterCredit;
            director.OverrideCurrentMonsterCard(directorCard);
            director.monsterSpawnTimer = 0;

            if (DirectorCore.instance)
            {
                var coreDirectors = DirectorCore.instance.GetComponents<CombatDirector>();
                foreach (CombatDirector coreDirector in coreDirectors)
                {
                    coreDirector.monsterCredit -= monsterCredit * combatDirectorMonsterCreditSubtractionCoefficient;
                }
            }
            
        }

        private void Explode()
        {
            Transform corpse = FindModelChild("Corpse");
            if (corpse)
            {
                corpse.SetParent(null);
            }

            if (modelLocator)
            {
                if (modelLocator.modelBaseTransform)
                {
                    Destroy(modelLocator.modelBaseTransform.gameObject);
                }
                if (modelLocator.modelTransform)
                {
                    Destroy(modelLocator.modelTransform.gameObject);
                }
            }

            if (explosionEffectPrefab)
            {
                Vector3 position = characterBody.corePosition;
                Transform muzzle = FindModelChild("Muzzle");
                if (muzzle)
                {
                    position = muzzle.position;
                }
                EffectManager.SpawnEffect(explosionEffectPrefab, new EffectData
                {
                    origin = position,
                    rotation = transform.rotation,
                }, false);
            }

            SpawnPickups();
            SpawnMonsters();

            int progress = GetEventProgress();
            onEliteCrystalDestroyed?.Invoke(progress);

            Destroy(gameObject);
        }

        private void SpawnPickups()
        {
            if (pickupPrefab && NetworkServer.active)
            {
                Vector3 position = characterBody.corePosition;
                Transform muzzle = FindModelChild("Muzzle");
                if (muzzle)
                {
                    position = muzzle.position;
                }
                for (int i = 0; i < maxPickups; i++)
                {
                    GameObject pickup = GameObject.Instantiate(pickupPrefab, position, UnityEngine.Random.rotation);
                    pickup.GetComponentInChildren<TeamFilter>().teamIndex = TeamIndex.Player; // :/
                    NetworkServer.Spawn(pickup);
                }
            }
            
        }
        private void SpawnMonsters()
        {
            if (monsterEncounterPrefab && NetworkServer.active)
            {
                if (baseMonsterCredit > 0)
                {
                    GameObject smallMonsterEncounter = GameObject.Instantiate(monsterEncounterPrefab, transform.position, Quaternion.identity);
                    NetworkServer.Spawn(smallMonsterEncounter);
                    CombatDirector combatDirector = smallMonsterEncounter.GetComponent<CombatDirector>();
                    if (combatDirector && Stage.instance)
                    {
                        float monsterCredit = baseMonsterCredit * Stage.instance.entryDifficultyCoefficient;
                        DirectorCard monsterCard = SelectMonsterCard(monsterCredit, maxMonsters, 1, combatDirector.rng);
                        ActivateCombatDirector(combatDirector, monsterCredit, monsterCard);
                    }
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }

    #region Variants

    #region Large
    public class EliteCrystalDeathLarge : EliteCrystalDeath
    {
        private static int _eventProgress = 18;

        private static float _baseMonsterCredit = 50f;
        private static int _maxMonsters = 6;

        private static int _maxPickups = 3;
        public override void OnEnter()
        {
            eventProgress = _eventProgress;

            baseMonsterCredit = _baseMonsterCredit;
            maxMonsters = _maxMonsters;

            maxPickups = _maxPickups;

            base.OnEnter();
        }
    }

    public class FireCrystalDeathLarge : EliteCrystalDeathLarge
    {
        public override EliteDef GetEliteDef()
        {
            return RoR2Content.Elites.Fire;
        }
    }
    public class IceCrystalDeathLarge : EliteCrystalDeathLarge
    {
        public override EliteDef GetEliteDef()
        {
            return RoR2Content.Elites.Ice;
        }
    }
    public class LightningCrystalDeathLarge : EliteCrystalDeathLarge
    {
        public override EliteDef GetEliteDef()
        {
            return RoR2Content.Elites.Lightning;
        }
    }
    public class EarthCrystalDeathLarge : EliteCrystalDeathLarge
    {
        public override EliteDef GetEliteDef()
        {
            return DLC1Content.Elites.Earth;
        }
    }
    #endregion

    #region Small
    public class EliteCrystalDeathSmall : EliteCrystalDeath
    {
        private static int _eventProgress = 6;

        private static float _baseMonsterCredit = 0f;
        private static int _maxMonsters = 0;

        private static int _maxPickups = 1;
        public override void OnEnter()
        {
            eventProgress = _eventProgress;

            baseMonsterCredit = _baseMonsterCredit;
            maxMonsters = _maxMonsters;

            maxPickups = _maxPickups;

            base.OnEnter();
        }
    }

    public class FireCrystalDeathSmall : EliteCrystalDeathSmall
    {
        public override EliteDef GetEliteDef()
        {
            return RoR2Content.Elites.Fire;
        }
    }
    public class IceCrystalDeathSmall : EliteCrystalDeathSmall
    {
        public override EliteDef GetEliteDef()
        {
            return RoR2Content.Elites.Ice;
        }
    }
    public class LightningCrystalDeathSmall : EliteCrystalDeathSmall
    {
        public override EliteDef GetEliteDef()
        {
            return RoR2Content.Elites.Lightning;
        }
    }
    public class EarthCrystalDeathSmall : EliteCrystalDeathSmall
    {
        public override EliteDef GetEliteDef()
        {
            return DLC1Content.Elites.Earth;
        }
    }
    #endregion

    #endregion
}
