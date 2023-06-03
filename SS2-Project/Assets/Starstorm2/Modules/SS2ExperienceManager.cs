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
using R2API.Networking.Interfaces;
using R2API.Networking;
using UnityEngine.UI;

namespace Moonstorm.Starstorm2
{
    //Note from groove - I promise the voices had no say in the creation of this class, it seemed like the best way to handle it
    //Reconciling getting adjusted exp to recalc stats may not seem like the best idea, as there is a slight disconnect between master and body, but it allows other mods to also offset experience
    public static class SS2ExperienceManager
	{
        //exp visuals mostly added to make the exp items feel cooler
        [RooConfigurableField(SS2Config.IDMisc, ConfigSection = "Visuals", ConfigName = "Improved Experience Visuals", ConfigDesc = "Enable the new experience visuals?")]
        public static bool enableNewExperienceVisuals = true;

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
            return TeamManager.instance.GetTeamExperience(characterMaster.teamIndex);
        }
        //transmit to clients provides the option of offsetting experience only from the server
        public static void SS2OffsetExperience(this CharacterMaster characterMaster, long amount, bool transmitToClients = false)
        {
            long previousOffset = masterToExperienceOffset.ContainsKey(characterMaster) ? masterToExperienceOffset[characterMaster] : 0;
            masterToExperienceOffset[characterMaster] = previousOffset + amount;
            EvaluateExtraHooks();
            if (characterMaster.hasBody)
            {
                characterMaster.GetBody().MarkAllStatsDirty();
            }
            if (transmitToClients && NetworkServer.active)
            {
                NetworkIdentity networkIdentity = characterMaster.GetComponent<NetworkIdentity>();
                if (networkIdentity)
                {
                    new TransmitExperienceOffset(amount, networkIdentity.netId).Send(NetworkDestination.Clients);
                }
            }
        }

        [SystemInitializer]
        public static void Init()
        {
            NetworkingAPI.RegisterMessageType<TransmitExperienceOffset>();
            // this get team level hook should be in the extra hooks, but it only works here for some reason...
            On.RoR2.TeamManager.GetTeamLevel += TeamManager_GetTeamLevel;
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.CharacterMaster.OnDestroy += CharacterMaster_OnDestroy;
            if (enableNewExperienceVisuals)
            {
                HUD hud = LegacyResourcesAPI.Load<GameObject>("Prefabs/HUDSimple").GetComponent<HUD>();
                ExpBar expBar = hud.expBar;

                TrailingExpBar.trailingFillBarPrefab = PrefabAPI.InstantiateClone(expBar.fillRectTransform.gameObject, "TrailingFillPanel", false);
                TrailingExpBar.trailingFillBarPrefab.GetComponent<Image>().color = Color.white;
                RectTransform trailingRectTransform = (RectTransform)TrailingExpBar.trailingFillBarPrefab.transform;
                trailingRectTransform.anchorMax = new Vector2(0f, 1f);
                trailingRectTransform.anchorMin = new Vector2(0f, 0f);

                TrailingExpBar trailingExpBar = expBar.gameObject.AddComponent<TrailingExpBar>();
                trailingExpBar.expBar = expBar;
            }
        }

        private static uint TeamManager_GetTeamLevel(On.RoR2.TeamManager.orig_GetTeamLevel orig, TeamManager self, TeamIndex teamIndex)
        {
            //SS2Log.Info("try get level");
            if (activeExperienceOffset != null)
            {
                ulong currentExperience = self.GetTeamExperience(teamIndex);
                uint level = TeamManager.FindLevelForExperience(currentExperience);
                //SS2Log.Info("level: " + level);
                return level;
            }
            return orig(self, teamIndex);
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
            EvaluateExtraHooks();
        }
        //we only need these hooks if there are characters with exp offsets - which will commonly not be the case
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
            On.RoR2.TeamManager.SetTeamExperience += TeamManager_SetTeamExperience;
            On.RoR2.UI.ExpBar.Update += ExpBar_Update;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.TeamManager.GetTeamCurrentLevelExperience += TeamManager_GetTeamCurrentLevelExperience;
            On.RoR2.TeamManager.GetTeamNextLevelExperience += TeamManager_GetTeamNextLevelExperience;
            On.RoR2.TeamManager.GetTeamExperience += TeamManager_GetTeamExperience;
            extraHooksSet = true;
        }

        private static void UnsetExtraHooks()
        {
            On.RoR2.TeamManager.SetTeamExperience -= TeamManager_SetTeamExperience;
            On.RoR2.UI.ExpBar.Update -= ExpBar_Update;
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
            On.RoR2.TeamManager.GetTeamCurrentLevelExperience -= TeamManager_GetTeamCurrentLevelExperience;
            On.RoR2.TeamManager.GetTeamNextLevelExperience -= TeamManager_GetTeamNextLevelExperience;
            On.RoR2.TeamManager.GetTeamExperience -= TeamManager_GetTeamExperience;
            extraHooksSet = false;
        }

        private static void TeamManager_SetTeamExperience(On.RoR2.TeamManager.orig_SetTeamExperience orig, TeamManager self, TeamIndex teamIndex, ulong newExperience)
        {
            orig(self, teamIndex, newExperience);
            foreach(CharacterMaster master in masterToExperienceOffset.Keys)
            {
                if (master && master.hasBody)
                {
                    master.GetBody().MarkAllStatsDirty();
                }
            }
        }

        private static void ExpBar_Update(On.RoR2.UI.ExpBar.orig_Update orig, ExpBar self)
        {
            if (self.source && masterToExperienceOffset.TryGetValue(self.source, out long offset))
            {
                bool hookActiveAbove = activeExperienceOffset != null;
                activeExperienceOffset = offset;
                try { orig(self); }
                finally
                {
                    if (!hookActiveAbove) { activeExperienceOffset = null; };
                }
            }
            else
            {
                orig(self);
            }
        }

        private static void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            if (self.master && masterToExperienceOffset.TryGetValue(self.master, out long offset))
            {
                bool hookActiveAbove = activeExperienceOffset != null;
                activeExperienceOffset = offset;
                try { orig(self); }
                finally
                {
                    if (!hookActiveAbove) { activeExperienceOffset = null; };
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
        public class TransmitExperienceOffset : INetMessage
        {
            private long OffsetAmount;
            private NetworkInstanceId MasterObjectID;

            public TransmitExperienceOffset()
            {
            }

            public TransmitExperienceOffset(long offsetAmount, NetworkInstanceId masterObjectID)
            {
                OffsetAmount = offsetAmount;
                MasterObjectID = masterObjectID;
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(OffsetAmount);
                writer.Write(MasterObjectID);
            }

            public void Deserialize(NetworkReader reader)
            {
                OffsetAmount = reader.ReadInt64();
                MasterObjectID = reader.ReadNetworkId();
            }

            public void OnReceived()
            {
                if (NetworkServer.active)
                {
                    return;
                }
                GameObject gameObject = Util.FindNetworkObject(MasterObjectID);
                if (gameObject)
                {
                    CharacterMaster characterMaster = gameObject.GetComponent<CharacterMaster>();
                    if (characterMaster)
                    {
                        characterMaster.SS2OffsetExperience(OffsetAmount, false);
                    }
                }
            }
        }

        public class TrailingExpBar : MonoBehaviour
        {
            public static GameObject trailingFillBarPrefab;

            public ExpBar expBar;
            private RectTransform baseBarTransform;
            private RectTransform fillRectTransform;
            private float x = 1f;
            private float xVelocity;
            public void Start()
            {
                baseBarTransform = expBar.fillRectTransform;
                fillRectTransform = (RectTransform)Instantiate(trailingFillBarPrefab, baseBarTransform.parent).transform;
            }
            public void Update()
            {
                float baseX = baseBarTransform.anchorMax.x;
                x = Mathf.SmoothDamp(x, baseX, ref xVelocity, 0.2f, 1f, Time.deltaTime); 
                fillRectTransform.anchorMin = new Vector2(Mathf.Min(Mathf.Clamp01(x), baseX), 0f);
                fillRectTransform.anchorMax = new Vector2(baseX, 1f);
            }
            public void OnEnable()
            {
                GlobalEventManager.onCharacterLevelUp += GlobalEventManager_onCharacterLevelUp;
            }
            public void OnDisable()
            {
                GlobalEventManager.onCharacterLevelUp -= GlobalEventManager_onCharacterLevelUp;
            }

            private void GlobalEventManager_onCharacterLevelUp(CharacterBody body)
            {
                if(body && expBar.source && expBar.source.bodyInstanceObject == body.gameObject)
                {
                    x -= 1;
                }
            }
        }
    }
}
