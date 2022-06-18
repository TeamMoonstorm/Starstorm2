using BepInEx;
using Moonstorm.Starstorm2.API;
using R2API.Utils;

namespace Moonstorm.Starstorm2
{
    [BepInDependency("com.TeamMoonstorm.MoonstormSharedUtils", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(
        nameof(R2API.DotAPI),
        nameof(R2API.PrefabAPI))]
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
            instance = this;
            pluginInfo = Info;
            SS2Log.logger = Logger;
            if (DEBUG)
            {
                base.gameObject.AddComponent<SS2DebugUtil>();
            }
            new SS2Config().Init();
            new SS2Assets().Init();
            new SS2Content().Init();
            new SS2Language().Init();
            ConfigurableFieldManager.AddMod(this);
            TokenModifierManager.AddToManager();
        }


        private void Start()
        {
            SoundBankManager.Init();
        }
    }
}