using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2
{
    [RequireComponent(typeof(CombatDirector))]
    [RequireComponent(typeof(CombatSquad))]
    public class AmbushBehavior : NetworkBehaviour
    {
        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            SS2Log.Info("Initializing AmbushBehavior");
            SceneDirector.onPrePopulateMonstersSceneServer += OnPrePopulateMonstersSceneServer;
            TeleporterInteraction.onTeleporterBeginChargingGlobal += OnTeleporterBeginCharging;
        }

        private static void OnPrePopulateMonstersSceneServer(SceneDirector sceneDirector)
        {
            foreach (var ambushBehavior in InstanceTracker.GetInstancesList<AmbushBehavior>())
            {
                sceneDirector.ReduceMonsterCredits(Mathf.RoundToInt(ambushBehavior.monsterCredit * sceneDirectorMonsterCreditSubtractionCoefficient));
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
        public int baseMonsterCredit = 100;
        private float monsterCredit
        {
            get
            {
                return baseMonsterCredit * Stage.instance.entryDifficultyCoefficient;
            }
        }
        public int maxMonsterTypes = 2;
        public int maxMonstersPerType = 6;
        public static float sceneDirectorMonsterCreditSubtractionCoefficient = 0.75f;

        public GameObject spawnPositionEffectPrefab;
        
        private DirectorCard[] chosenDirectorCards = Array.Empty<DirectorCard>();
        private bool active = true;
        // {You have been ambushed by }{___}{___ and ___}{___, ___, and ___}

        private CombatDirector combatDirector;

        private void Awake()
        {
            combatDirector = GetComponent<CombatDirector>();
        }
        private void OnEnable()
        {
            InstanceTracker.Add(this);
        }
        private void OnDisable()
        {
            InstanceTracker.Remove(this);
        }
        public void SetActive(bool newActive)
        {
            active = newActive;
        }

    }
}
