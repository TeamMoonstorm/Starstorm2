using BepInEx;
using SS2.API;
using R2API;
using R2API.Utils;
using R2API.Networking;
using UnityEngine;
using MSU;
using System;
namespace SS2
{
    #region R2API
    [BepInDependency("com.bepis.r2api.dot")]
    [BepInDependency("com.bepis.r2api.networking")]
    [BepInDependency("com.bepis.r2api.prefab")]
    [BepInDependency("com.bepis.r2api.difficulty")]
    [BepInDependency("com.bepis.r2api.tempvisualeffect")]
    #endregion
    [BepInDependency(MSU.MSUMain.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.RiskyLives.RiskyMod", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class SS2Main : BaseUnityPlugin
    {
        public const string GUID = "com.TeamMoonstorm";
        public const string MODNAME = "Starstorm 2";
        public const string VERSION = "0.6.18";

        internal static SS2Main Instance { get; private set; }

        public static bool ScepterInstalled { get; private set; }
        public static bool RiskyModInstalled { get; private set; }
        public static bool GOTCEInstalled { get; private set; }
        public static bool StageAestheticInstalled { get; private set; }
        internal static bool ChristmasTime { get; private set; }
        public void Awake()
        {
            Instance = this;
#if DEBUG
            base.gameObject.AddComponent<SS2DebugUtil>();
#endif
            new SS2Log(Logger);
            new SS2Config(this);
            new SS2Content();
            
            LanguageFileLoader.AddLanguageFilesFromMod(this, "languages");
            LoadingScreenSpriteUtility.AddSpriteAnimations(SS2Assets.GetLoadingScreenBundle());

            TMProEffects.Init();
            BodyNames.Hook();
            HideUnlocks.Hook();
            //N: Not gonna lie, i love the idea of seasonal effects, but having the same date time check is silly, so there's that internal static bool now.
            DateTime today = DateTime.Today;
            if (today.Month == 12 && ((today.Day == 31) || (today.Day == 30) || (today.Day == 29) || (today.Day == 28) || (today.Day == 27) || (today.Day == 26) || (today.Day == 25) || (today.Day == 24) || (today.Day == 23)))
                ChristmasTime = true;

            //N: i have no idea if SystemInitializer would be too late for this, so it stays here for now.
            //R2API.Networking.NetworkingAPI.RegisterMessageType<ScriptableObjects.NemesisSpawnCard.SyncBaseStats>();
            
        }



        private void Start()
        {
            SoundBankManager.Init();
            SetupModCompat();
        }

        private void SetupModCompat()
        {
            ScepterInstalled = MSUtil.IsModInstalled(AncientScepter.AncientScepterMain.ModGuid);
            RiskyModInstalled = MSUtil.IsModInstalled("com.RiskyLives.RiskyMod");
            GOTCEInstalled = MSUtil.IsModInstalled("com.TheBestAssociatedLargelyLudicrousSillyheadGroup.GOTCE");
            StageAestheticInstalled = MSUtil.IsModInstalled("com.HIFU.StageAesthetic");
        }
    }
}