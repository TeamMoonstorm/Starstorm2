using R2API.ScriptableObjects;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UObject = UnityEngine.Object;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Unlockables : UnlockablesModuleBase
    {
        public static Unlockables Instance { get; private set; }

        public override R2APISerializableContentPack SerializableContentPack { get; } = SS2Content.Instance.SerializableContentPack;

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            SS2Log.Info($"Initializing Unlockables.");
            GetUnlockableBases();
        }

        protected override IEnumerable<UnlockableBase> GetUnlockableBases()
        {
            var allUnlocks = base.GetUnlockableBases();
            if (SS2Config.UnlockAll)
            {
                RemoveAllNonSkinUnlocks();
                allUnlocks = allUnlocks.Where(unlock => unlock.UnlockableDef.cachedName.Contains("skin"));
            }

            allUnlocks.ToList().ForEach(unlock => AddUnlockable(unlock));

            return null;
        }

        private void RemoveAllNonSkinUnlocks()
        {
#if DEBUG
            SS2Log.Info("Unlock all is enabled, removing unlocks for everything except skins");
#endif
            //This should load all the assets we have that:
            //Are not skin defs
            //Have a field that has an unlockableDef field.
            var allAssets = SS2Assets.LoadAllAssetsOfType<UnityEngine.Object>(SS2Bundle.All)
                .Where(asset => !(asset is SkinDef))
                .Where(asset => asset.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(fInfo =>
                {
                    bool isNotStatic = !fInfo.IsStatic;
                    var fieldType = fInfo.FieldType;
                    bool isUnlockableDefOrDerived = fieldType.IsSameOrSubclassOf<UnlockableDef>();
                    return isNotStatic && isUnlockableDefOrDerived;
                }).Count() > 0);

            foreach (UObject asset in allAssets)
            {
                var fieldsInAsset = asset.GetType()
                                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                    .Where(fieldInfo => fieldInfo.FieldType.IsSameOrSubclassOf<UnlockableDef>()).ToArray();

                foreach (FieldInfo field in fieldsInAsset)
                {
                    field.SetValue(asset, null);
                }

#if DEBUG
                SS2Log.Info($"Removed {fieldsInAsset.Length} unlockableDef references from {asset}");
#endif
            }
        }
    }
}
