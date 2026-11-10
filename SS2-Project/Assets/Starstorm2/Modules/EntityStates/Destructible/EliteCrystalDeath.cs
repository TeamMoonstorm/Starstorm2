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

        [SerializeField] public EntityStateGameObject explosionEffectPrefab;
        [SerializeField] public EntityStateGameObject monsterSpawnEffectPrefab;
        [SerializeField] public EliteDef eliteDef;

        private static float expMultiplier = 0.2f;
        private static float goldMultiplier = 0.5f;

        public int eventProgress = 25;

        public float baseSmallMonsterCredit = 30f;
        public int minSmallMonsters = 2;
        public int maxSmallMonsters = 6;

        public float baseLargeMonsterCredit = 30f;
        public int minLargeMonsters = 1;
        public int maxLargeMonsters = 2;

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
        }

        private void Explode()
        {
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
                EffectManager.SpawnEffect(explosionEffectPrefab, new EffectData
                {
                    origin = transform.position,
                    rotation = Quaternion.identity
                }, false);
            }

            SpawnMonsters();

            int progress = GetEventProgress();
            onEliteCrystalDestroyed?.Invoke(progress);

            Destroy(gameObject);
        }

        private void SpawnMonsters()
        {
            if (monsterEncounterPrefab && NetworkServer.active)
            {
                GameObject smallMonsterEncounter = GameObject.Instantiate(monsterEncounterPrefab, transform.position, Quaternion.identity);
                NetworkServer.Spawn(smallMonsterEncounter);
                CombatDirector combatDirector = smallMonsterEncounter.GetComponent<CombatDirector>();
                if (combatDirector && Stage.instance)
                {
                    float smallMonsterCredit = baseSmallMonsterCredit * Stage.instance.entryDifficultyCoefficient;
                    DirectorCard smallMonsterCard = SelectMonsterCard(smallMonsterCredit, maxSmallMonsters, minSmallMonsters, combatDirector.rng);
                    ActivateCombatDirector(combatDirector, smallMonsterCredit, smallMonsterCard);
                }

                GameObject largeMonsterEncounter = GameObject.Instantiate(monsterEncounterPrefab, transform.position, Quaternion.identity);
                NetworkServer.Spawn(largeMonsterEncounter);
                combatDirector = largeMonsterEncounter.GetComponent<CombatDirector>();
                if (combatDirector && Stage.instance)
                {
                    float largeMonsterCredit = baseLargeMonsterCredit * Stage.instance.entryDifficultyCoefficient;
                    DirectorCard largeMonsterCard = SelectMonsterCard(largeMonsterCredit, maxLargeMonsters, minLargeMonsters, combatDirector.rng);
                    ActivateCombatDirector(combatDirector, largeMonsterCredit, largeMonsterCard);
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
        private static int _eventProgress = 15;

        private static float _baseSmallMonsterCredit = 30f;
        private static int _minSmallMonsters = 2;
        private static int _maxSmallMonsters = 6;

        private static float _baseLargeMonsterCredit = 30f;
        private static int _minLargeMonsters = 1;
        private static int _maxLargeMonsters = 2;
        public override void OnEnter()
        {
            eventProgress = _eventProgress;

            baseSmallMonsterCredit = _baseSmallMonsterCredit;
            minSmallMonsters = _minSmallMonsters;
            maxSmallMonsters = _maxSmallMonsters;
            baseLargeMonsterCredit = _baseLargeMonsterCredit;
            minLargeMonsters = _minLargeMonsters;
            maxLargeMonsters = _maxLargeMonsters;

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
        private static int _eventProgress = 5;

        private static float _baseSmallMonsterCredit = 9f;
        private static int _minSmallMonsters = 2;
        private static int _maxSmallMonsters = 6;

        private static float _baseLargeMonsterCredit = 0;
        private static int _minLargeMonsters = 1;
        private static int _maxLargeMonsters = 2;
        public override void OnEnter()
        {
            eventProgress = _eventProgress;

            baseSmallMonsterCredit = _baseSmallMonsterCredit;
            minSmallMonsters = _minSmallMonsters;
            maxSmallMonsters = _maxSmallMonsters;
            baseLargeMonsterCredit = _baseLargeMonsterCredit;
            minLargeMonsters = _minLargeMonsters;
            maxLargeMonsters = _maxLargeMonsters;

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
