using BepInEx;
using SS2.API;
using R2API;
using R2API.Utils;
using R2API.Networking;
using UnityEngine;
using MSU;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Unity.Burst;

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
        public const string VERSION = "0.6.10";

        internal static SS2Main Instance { get; private set; }

        public static bool ScepterInstalled { get; private set; }
        public static bool RiskyModInstalled { get; private set; }
        public static bool GOTCEInstalled { get; private set; }
        public static bool StageAestheticInstalled { get; private set; }
        internal static bool ChristmasTime { get; private set; }
        internal static bool ChileanIndependenceWeek { get; private set; }
        internal static event Action onFixedUpdate;
        public void Awake()
        {
            Instance = this;
#if DEBUG
            base.gameObject.AddComponent<SS2DebugUtil>();
#endif
            new SS2Log(Logger);
            new SS2Config(this);
            new SS2Content();

            LoadBurstAssembly();
            LanguageFileLoader.AddLanguageFilesFromMod(this, "languages");
            LoadingScreenSpriteUtility.AddSpriteAnimations(SS2Assets.GetLoadingScreenBundle());

            TMProEffects.Init();
            BodyNames.Hook();
            HideUnlocks.Hook();
            SetSpecialEventBooleans();            
        }

        private void SetSpecialEventBooleans()
        {
            //N: Funny method i wrote that makes both Runshroom's Santa Hat and Clay Monger's Lucky Pup events last an entire week, said week is the week where the "special day" lands. so even if christmas lands on a sunday, all previous days will count as the Christmas time event.
            ChristmasTime = SS2Util.DoesTodayLandWithinASpecificDaysWeek(25, 12);
            ChileanIndependenceWeek = SS2Util.DoesTodayLandWithinASpecificDaysWeek(18, 9);
        }

        private void FixedUpdate()
        {
            onFixedUpdate?.Invoke();
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

        //We need to explicitly tell the Burst system to load our assembly containing our bursted jobs.
        private void LoadBurstAssembly()
        {
            SS2Log.Info("Loading Starstorm2_Burst.dll...");
            var modLocation = Info.Location;
            var directoryName = Path.GetDirectoryName(modLocation);
            var assemblyPath = Path.Combine(directoryName, "Starstorm2_Burst.dll");
            if(!File.Exists(assemblyPath))
            {
                SS2Log.Error($"Failed to load Starstorm2's Burst dll! file does not exist.");
                return;
            }

            BurstRuntime.LoadAdditionalLibrary(assemblyPath);
            SS2Log.Message("Starstorm2_Burst.dll loadded succesfully!");
        }
    }
}