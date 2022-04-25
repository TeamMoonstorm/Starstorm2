using BepInEx;
using Moonstorm.Starstorm2.API;
using R2API.Utils;

namespace Moonstorm.Starstorm2
{
    [BepInDependency("com.TeamMoonstorm.MoonstormSharedUtils", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(
        nameof(R2API.DotAPI))]
    [BepInPlugin(guid, modName, version)]
    public class Starstorm : BaseUnityPlugin
    {
        internal const string guid = "com.TeamMoonstorm.Starstorm2-Nightly";
        internal const string modName = "Starstorm 2 Nightly";
        internal const string version = "0.4.0";

        public static Starstorm instance;
        public static PluginInfo pluginInfo;
        public static bool DEBUG = true;

        public void Awake()
        {
            Logger.LogInfo(1);
            instance = this;
            Logger.LogInfo(2);
            pluginInfo = Info;
            Logger.LogInfo(3);
            SS2Log.logger = Logger;
            Logger.LogInfo(4);
            if (DEBUG)
            {
                Logger.LogInfo(5);
                base.gameObject.AddComponent<SS2DebugUtil>();
                Logger.LogInfo(6);
            }
            Logger.LogInfo(7);
            new SS2Config().Init();
            Logger.LogInfo(8);
            new SS2Assets().Init();
            Logger.LogInfo(9);
            new SS2Content().Init();
            Logger.LogInfo(10);
            new SS2Language().Init();
            Logger.LogInfo(11);
            ConfigurableFieldManager.AddMod(this);
            Logger.LogInfo(12);
            TokenModifierManager.AddToManager();
            Logger.LogInfo(13);
        }


        private void Start()
        {
            SoundBankManager.Init();
        }
    }
}