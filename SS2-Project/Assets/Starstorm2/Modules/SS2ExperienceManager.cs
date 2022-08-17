using Moonstorm;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using RoR2.Items;
using System.Collections.Generic;
using HG;
using HG.Reflection;
using JetBrains.Annotations;
using System.Reflection;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2.UI;

namespace Moonstorm.Starstorm2
{
	public static class SS2ExperienceManager
	{
        public static Dictionary<CharacterMaster, ulong> masterToFoundExperience = new Dictionary<CharacterMaster, ulong>();
        public static Dictionary<CharacterMaster, long> masterToExperienceOffset = new Dictionary<CharacterMaster, long>();

        private static bool extraHooksSet = false;
        private static long? activeExperienceOffset = null;

        public static ulong SS2GetAdjustedExperience(this CharacterMaster characterMaster)
        {
            if (characterMaster.hasBody)
            {
                characterMaster.GetBody().RecalculateStats();
            }
            if(masterToFoundExperience.TryGetValue(characterMaster, out ulong experience))
            {
                return experience;
            }
            return 0U;
        }
        public static void SS2OffsetExperience(this CharacterMaster characterMaster, long amount)
        {
            long previousOffset = masterToExperienceOffset.ContainsKey(characterMaster) ? masterToExperienceOffset[characterMaster] : 0;
            masterToExperienceOffset[characterMaster] = previousOffset + amount;
            EvaluateExtraHooks();
            if (characterMaster.hasBody)
            {
                characterMaster.GetBody().MarkAllStatsDirty();
            }
        }

        [SystemInitializer]
        public static void Init()
        {
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.CharacterMaster.OnDestroy += CharacterMaster_OnDestroy;
        }

        private static void CharacterBody_RecalculateStats(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.MoveAfterLabels();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<CharacterBody>>((body) =>
            {
                if (body.master)
                {
                    masterToFoundExperience[body.master] = TeamManager.instance.GetTeamExperience(body.master.teamIndex);
                }
            });
        }

        private static void CharacterMaster_OnDestroy(On.RoR2.CharacterMaster.orig_OnDestroy orig, CharacterMaster self)
        {
            orig(self);
            if (masterToFoundExperience.ContainsKey(self))
            {
                masterToFoundExperience.Remove(self);
            }
            if (masterToExperienceOffset.ContainsKey(self))
            {
                masterToExperienceOffset.Remove(self);
            }
        }
        private static void EvaluateExtraHooks()
        {
            bool hooksRequired = masterToExperienceOffset.Count > 0;
            if (hooksRequired != extraHooksSet)
            {
                if (hooksRequired)
                {
                    SetExtraHooks();
                    return;
                }
                UnsetExtraHooks();
            }
        }
        private static void SetExtraHooks()
        {
            On.RoR2.UI.ExpBar.Update += ExpBar_Update;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats1;
            On.RoR2.TeamManager.GetTeamCurrentLevelExperience += TeamManager_GetTeamCurrentLevelExperience;
            On.RoR2.TeamManager.GetTeamNextLevelExperience += TeamManager_GetTeamNextLevelExperience;
            On.RoR2.TeamManager.GetTeamExperience += TeamManager_GetTeamExperience;
            On.RoR2.TeamManager.GetTeamLevel += TeamManager_GetTeamLevel;
            extraHooksSet = true;
        }
        private static void UnsetExtraHooks()
        {
            On.RoR2.UI.ExpBar.Update -= ExpBar_Update;
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats1;
            On.RoR2.TeamManager.GetTeamCurrentLevelExperience -= TeamManager_GetTeamCurrentLevelExperience;
            On.RoR2.TeamManager.GetTeamNextLevelExperience -= TeamManager_GetTeamNextLevelExperience;
            On.RoR2.TeamManager.GetTeamExperience -= TeamManager_GetTeamExperience;
            On.RoR2.TeamManager.GetTeamLevel -= TeamManager_GetTeamLevel;
            extraHooksSet = false;
        }
        private static void ExpBar_Update(On.RoR2.UI.ExpBar.orig_Update orig, ExpBar self)
        {
            if (masterToExperienceOffset.TryGetValue(self.source, out long offset))
            {
                activeExperienceOffset = offset;
                try
                {
                    orig(self);
                }
                finally
                {
                    activeExperienceOffset = null;
                }
            }
            else
            {
                orig(self);
            }
        }

        private static void CharacterBody_RecalculateStats1(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            if (masterToExperienceOffset.TryGetValue(self.master, out long offset))
            {
                activeExperienceOffset = offset;
                try
                {
                    orig(self);
                }
                finally
                {
                    activeExperienceOffset = null;
                }
            }
            else
            {
                orig(self);
            }
        }

        private static ulong TeamManager_GetTeamCurrentLevelExperience(On.RoR2.TeamManager.orig_GetTeamCurrentLevelExperience orig, TeamManager self, TeamIndex teamIndex)
        {
            if (activeExperienceOffset != null)
            {
                return TeamManager.GetExperienceForLevel(self.GetTeamLevel(teamIndex));
            }
            return orig(self, teamIndex);
        }

        private static ulong TeamManager_GetTeamNextLevelExperience(On.RoR2.TeamManager.orig_GetTeamNextLevelExperience orig, TeamManager self, TeamIndex teamIndex)
        {
            if (activeExperienceOffset != null)
            {
                return TeamManager.GetExperienceForLevel(self.GetTeamLevel(teamIndex) + 1U);
            }
            return orig(self, teamIndex);
        }

        private static ulong TeamManager_GetTeamExperience(On.RoR2.TeamManager.orig_GetTeamExperience orig, TeamManager self, TeamIndex teamIndex)
        {
            ulong original = orig(self, teamIndex);
            if (activeExperienceOffset != null)
            {
                return (ulong)Math.Max((long)original + (long)activeExperienceOffset, 0);
            }
            return original;
        }

        private static uint TeamManager_GetTeamLevel(On.RoR2.TeamManager.orig_GetTeamLevel orig, TeamManager self, TeamIndex teamIndex)
        {
            if (activeExperienceOffset != null)
            {
                ulong currentExperience = self.GetTeamExperience(teamIndex);
                uint level = TeamManager.FindLevelForExperience(currentExperience);
                return level;
            }
            return orig(self, teamIndex);
        }
    }
}
