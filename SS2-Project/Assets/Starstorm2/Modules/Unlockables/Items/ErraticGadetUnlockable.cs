using RoR2;
using RoR2.Achievements;
using System.Collections.Generic;

namespace Moonstorm.Starstorm2.Unlocks.Pickups
{
    [DisabledContent]
    public sealed class ErraticGadgetUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.item.erraticgadget", SS2Bundle.Indev);

        public override void Initialize()
        {
            AddRequiredType<Items.ErraticGadget>();
        }
        public sealed class ErraticGadgetAchievement : BaseAchievement
        {
            private Inventory currentInventory;
            private PlayerCharacterMasterController currentMasterController;
            public static List<ItemDef> CritItems = new List<ItemDef>();
            public override void OnInstall()
            {
                base.OnInstall();
                localUser.onMasterChanged += OnMasterChanged;
                SetMasterController(localUser.cachedMasterController);

                CritItems.Add(RoR2Content.Items.CritGlasses);
                CritItems.Add(RoR2Content.Items.AttackSpeedOnCrit);
                CritItems.Add(RoR2Content.Items.HealOnCrit);
                CritItems.Add(RoR2Content.Items.BleedOnHitAndExplode);
                if (SS2Content.Items.Needles)
                    CritItems.Add(SS2Content.Items.Needles);
                if (SS2Content.Items.GreenChocolate)
                    CritItems.Add(SS2Content.Items.GreenChocolate);
                if (SS2Content.Items.HuntersSigil)
                    CritItems.Add(SS2Content.Items.HuntersSigil);
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
                    int neededItemsObtained = 0;
                    foreach (ItemDef critItem in CritItems)
                    {
                        var currentItemCount = currentInventory.GetItemCount(critItem);
                        if (currentItemCount >= 1)
                        {
                            neededItemsObtained += currentItemCount;
                        }
                    }
                    if (neededItemsObtained >= 10)
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