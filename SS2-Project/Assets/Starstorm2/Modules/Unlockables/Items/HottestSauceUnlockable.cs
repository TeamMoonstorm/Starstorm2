using RoR2;
using RoR2.Achievements;
using System.Collections.Generic;

namespace Moonstorm.Starstorm2.Unlocks.Pickups
{
    [DisabledContent]
    public sealed class HottestSauceUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.item.hottestsauce", SS2Bundle.Indev);

        public override void Initialize()
        {
            AddRequiredType<Items.HottestSauce>();
        }
        public sealed class HottestSauceAchievement : BaseAchievement
        {
            private Inventory currentInventory;
            private PlayerCharacterMasterController currentMasterController;

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
                    List<ItemDef> neededItems = new List<ItemDef>() { RoR2Content.Items.FireRing, RoR2Content.Items.ExplodeOnDeath, RoR2Content.Items.IgniteOnKill };
                    int neededItemsObtained = 0;
                    foreach (ItemDef itemDef in neededItems)
                    {
                        var currentItemsItemCount = currentInventory.GetItemCount(itemDef);
                        if (currentItemsItemCount >= 1)
                        {
                            neededItemsObtained++;
                        }
                    }
                    if (neededItemsObtained >= 3)
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