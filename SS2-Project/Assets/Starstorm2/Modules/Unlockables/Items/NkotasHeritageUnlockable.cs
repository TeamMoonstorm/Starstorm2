using RoR2;
using RoR2.Achievements;
using System.Linq;

namespace Moonstorm.Starstorm2.Unlocks.Pickups
{
    [DisabledContent]
    public sealed class NkotasHeritageUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.item.nkotasheritage");

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
                    int num = 25;
                    int uniqueItems = currentInventory.itemAcquisitionOrder.Count(x => currentInventory.GetItemCount(x) > 0 && ItemTierCatalog.GetItemTierDef(ItemCatalog.GetItemDef(x).tier).isDroppable);
                    if (uniqueItems >= num)
                    {
                        Grant();
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