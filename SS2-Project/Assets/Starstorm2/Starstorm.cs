using BepInEx;
using Moonstorm.Starstorm2.API;
using R2API.Utils;

namespace Moonstorm.Starstorm2
{
#if DEBUG
    [BepInDependency("iHarbHD.DebugToolkit", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.KingEnderBrine.ItemDisplayPlacementHelper", BepInDependency.DependencyFlags.SoftDependency)]
#endif
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.TeamMoonstorm.MoonstormSharedUtils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.niwith.DropInMultiplayer", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(guid, modName, version)]
    [R2APISubmoduleDependency(new string[]
    {
        "PrefabAPI",
        "LoadoutAPI",
        "DirectorAPI",
        "NetworkingAPI",
        "SoundAPI",
        "CommandHelper",
        "DotAPI",
        "DamageAPI",
        "ArtifactCodeAPI",
        "SoundAPI",
        "ProjectileAPI"
    })]

    public class Starstorm : BaseUnityPlugin
    {
        internal const string guid = "com.TeamMoonstorm.Starstorm2-Nightly";
        internal const string modName = "Starstorm 2 Nightly";
        internal const string version = "0.3.37";

        public static Starstorm instance;
        public static PluginInfo pluginInfo;
        public static bool DEBUG = true;

        public void Awake()
        {
            instance = this;
            pluginInfo = Info;
            SS2Log.logger = Logger;

            if (DEBUG)
                base.gameObject.AddComponent<SS2DebugUtil>();

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