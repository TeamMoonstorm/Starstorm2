/*using RoR2.ContentManagement;
using UnityEngine;

namespace Moonstorm.Starstorm2.Modules
{
    public class ItemDisplays : ItemDisplayModuleBase
    {
        public static ItemDisplays Instance { get; set; }
        public override AssetBundle AssetBundle { get; set; } = SS2Assets.Instance.MainAssetBundle;
        public override SerializableContentPack ContentPack { get; set; } = Starstorm2Content.Instance.SerializableContentPack;

        public override void Init()
        {
            Instance = this;
            base.Init();
            PopulateKeyAssetsAndDisplaysFromAssetbundle();
            PopulateMSIDRSFromAssetBundle();
            PopulateSingleItemDisplayRuleFromAssetBundle();
        }
    }
}*/