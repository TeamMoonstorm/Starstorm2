using RoR2;
using RoR2.Achievements;
using System.Collections.Generic;
namespace SS2.Unlocks.Pickups
{
    public sealed class ErraticGadgetAchievement : BaseAchievement
    {
        private Inventory currentInventory;
        private PlayerCharacterMasterController currentMasterController;
        public static List<ItemDef> LightningItems = new List<ItemDef>();
        public override void OnInstall()
        {
            base.OnInstall();
            localUser.onMasterChanged += OnMasterChanged;
            SetMasterController(localUser.cachedMasterController);

            LightningItems.Add(RoR2Content.Items.ChainLightning);
            LightningItems.Add(RoR2Content.Items.LightningStrikeOnHit);
            LightningItems.Add(RoR2Content.Items.ShockNearby);
            LightningItems.Add(DLC1Content.Items.ChainLightningVoid);
            if (SS2Content.Items.CrypticSource)
                LightningItems.Add(SS2Content.Items.CrypticSource);
            if (SS2Content.Items.LightningOnKill)
                LightningItems.Add(SS2Content.Items.LightningOnKill);
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
                foreach (ItemDef lightningItem in LightningItems)
                {
                    var currentItemCount = currentInventory.GetItemCount(lightningItem);
                    if (currentItemCount >= 1)
                    {
                        neededItemsObtained += currentItemCount;
                    }
                }
                if (currentInventory.currentEquipmentIndex == RoR2Content.Equipment.Lightning.equipmentIndex || currentInventory.currentEquipmentIndex == RoR2Content.Equipment.AffixBlue.equipmentIndex)
                {
                    neededItemsObtained += 1;
                }
                if (neededItemsObtained >= 5)
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