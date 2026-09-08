using RoR2;
using RoR2.ContentManagement;
using UnityEngine;

namespace SS2.Items
{
    public sealed class BirthrightChimeraHelper : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acBirthrightChimeraHelper", SS2Bundle.Items);

        public override void Initialize()
        {
            On.RoR2.UI.HealthBar.UpdateBarInfos += HealthBarOnUpdateBarInfos;
        }

        private void HealthBarOnUpdateBarInfos(On.RoR2.UI.HealthBar.orig_UpdateBarInfos orig, RoR2.UI.HealthBar self)
        {
            //stolen from neb neb ,.., 
            orig(self);

            HealthComponent healthComponent = self._source;
            if (!healthComponent) return;

            if (healthComponent.body?.inventory?.GetItemCountEffective(ItemDef) > 0)
            {
                self.barInfoCollection.trailingOverHealthbarInfo.color = new Color32(100, 200, 255, 255);
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}