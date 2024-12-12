using System;
using System.Collections.Generic;
using RoR2.Items;
using UnityEngine;
using RoR2;
using RoR2.ExpansionManagement;
using R2API;
namespace SS2
{
    [CreateAssetMenu(fileName = "EventCard", menuName = "Starstorm2/EventCard")]
    public class EventCard : ScriptableObject
    {
        public GameObject eventPrefab;
        public bool canStartDuringTeleporterEvent;

        public string requiredUnlockable;
        public UnlockableDef requiredUnlockableDef;
        public string requiredFlag;
        public string forbiddenFlag;
        private ExpansionDef requiredExpansionDef;
        public Expansion requiredExpansion;
        public int requiredStagesCompleted;
        public enum Expansion
        {
            None,
            DLC1,
            DLC2
        }
        public bool IsAvailable()
        {
            Run run = Run.instance;
            if (!requiredUnlockableDef && !string.IsNullOrEmpty(requiredUnlockable))
            {
                requiredUnlockableDef = UnlockableCatalog.GetUnlockableDef(requiredUnlockable);
                requiredUnlockable = null;
            }
            if (!requiredExpansionDef && requiredExpansion > Expansion.None)
            {
                switch (requiredExpansion)
                {
                    case Expansion.DLC1:
                        requiredExpansionDef = SS2Util.DLC1;
                        break;
                    case Expansion.DLC2:
                        requiredExpansionDef = SS2Util.DLC2;
                        break;
                }
            }
            bool hasUnlock = !requiredUnlockableDef || run.IsUnlockableUnlocked(requiredUnlockableDef);
            bool hasFlag = string.IsNullOrEmpty(requiredFlag) || run.GetEventFlag(requiredFlag);
            bool noForbiddenFlag = string.IsNullOrEmpty(forbiddenFlag) || !run.GetEventFlag(forbiddenFlag);
            bool hasExpansion = !requiredExpansionDef || run.IsExpansionEnabled(requiredExpansionDef);
            bool hasStages = run.stageClearCount >= requiredStagesCompleted;
            return hasUnlock && hasFlag && noForbiddenFlag && hasExpansion && hasStages;
        }
    }
}
