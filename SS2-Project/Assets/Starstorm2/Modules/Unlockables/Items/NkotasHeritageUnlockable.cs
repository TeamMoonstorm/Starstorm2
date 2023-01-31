using RoR2;
using RoR2.Achievements;
using System.Linq;

namespace Moonstorm.Starstorm2.Unlocks.Pickups
{
    public sealed class NkotasHeritageUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.item.nkotasheritage", SS2Bundle.Items);

        public override void Initialize()
        {
            AddRequiredType<Items.NkotasHeritage>();
        }
        public sealed class NkotasHeritageAchievement : BaseAchievement
        {
            private PlayerCharacterMasterController currentMasterController;
            private Inventory currentInventory;

            public override void OnInstall()
            {
                base.OnInstall();
                localUser.onMasterChanged += OnMasterChanged;
                SetMasterController(localUser.cachedMasterController);
            }

            public override void OnUninstall()
            {
                SetMasterController(null);
                localUser.onMasterChanged -= OnMasterChanged;
                base.OnUninstall();
            }
            private void SetMasterController(PlayerCharacterMasterController newMasterController)
            {
                if ((object)currentMasterController != newMasterController)
                {
                    if ((object)currentInventory != null)
                    {
                        currentInventory.onInventoryChanged -= OnInventoryChanged;
                    }
                    currentMasterController = newMasterController;
                    currentInventory = currentMasterController?.master?.inventory;
                    if ((object)currentInventory != null)
                    {
                        currentInventory.onInventoryChanged += OnInventoryChanged;
                    }
                }
            }
            private void OnInventoryChanged()
            {
                if ((bool)currentInventory)
                {
                    int num = 25; //required unique items for unlock
                    //SS2Log.Debug("aghhhhhh: " + currentInventory.itemAcquisitionOrder.Count);
                    if(currentInventory.itemAcquisitionOrder.Count >= num) {
                        int temp = 0;
                        for(int i = 0; i < currentInventory.itemAcquisitionOrder.Count; i++)
                        {
                            var currentItemDef = ItemCatalog.GetItemDef(currentInventory.itemAcquisitionOrder[i]);
                            if(currentItemDef.tier != ItemTier.NoTier && !currentItemDef.hidden)
                            {
                                temp++;
                            }
                        }
                        if(temp >= num)
                        {
                            Grant();
                        }
                    }
                }
            }
            private void OnMasterChanged()
            {
                SetMasterController(localUser.cachedMasterController);
            }
        }
    }
}