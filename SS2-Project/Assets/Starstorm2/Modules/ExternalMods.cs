namespace Moonstorm.Starstorm2
{
    /*
    public static class ExternalMods
    {
        internal static bool hasEliteVariety;
        internal static bool hasMysticItems;
        internal static bool hasAetherium;


        [SystemInitializer]
        private static void Init()
        {
            if (MSUtil.IsModInstalled("com.themysticsword.elitevariety"))
            {
                LogCore.LogI("Elite Variety Installed");
                LoadEliteVariety();
            }
        }
        private static void LoadEliteVariety()
        {
            if (ItemDisplayModuleBase.log)
                LogCore.LogD("-------------------\nElite Variety Equipment Key Asset Names");
            typeof(EliteVariety.EliteVarietyContent.Equipment).GetFields()
                .ToList()
                .ForEach(Field =>
                {
                    if (ItemDisplayModuleBase.log)
                        LogCore.LogD(Field.Name);
                    ItemDisplayModuleBase.equipKeyAssets.Add(Field.Name.ToLowerInvariant(), Field.GetValue(typeof(EquipmentDef)) as Object);
                });
            if (ItemDisplayModuleBase.log)
                LogCore.LogD("--------------------\nElite Variety Item Display Prefab Names");
            List<string> followerPrefabNames = new List<string>() { "Armored", "Buffing", "Pillaging", "Sandstorm", "Tinkerer", "ImpPlane" };
            foreach (string prefabName in followerPrefabNames)
            {
                var followerPrefab = EliteVariety.Items.AllItemDisplaysItem.GetEliteFollowerPrefab(prefabName);
                var constructedString = prefabName + "FollowerModel";
                if (ItemDisplayModuleBase.log)
                    LogCore.LogD(constructedString);
                ItemDisplayModuleBase.moonstormItemDisplayPrefabs.Add(constructedString.ToLowerInvariant(), followerPrefab);
            }
        }

        private static void LoadAetherium()
        {
            if (ItemDisplayModuleBase.log)
                LogCore.LogD("-------------------\nAetherium Item Display Prefab Names");
        }
    }*/
}