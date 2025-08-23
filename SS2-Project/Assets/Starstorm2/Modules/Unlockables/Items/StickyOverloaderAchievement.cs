using RoR2;
using RoR2.Achievements;
namespace SS2.Unlocks.Pickups
{
    public sealed class StickyOverloaderAchievement : BaseAchievement
    {
        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            Inventory.onInventoryChangedGlobal += OnInventoryChangedGlobal;
        }
        public override void OnBodyRequirementBroken()
        {
            base.OnBodyRequirementBroken();
            Inventory.onInventoryChangedGlobal -= OnInventoryChangedGlobal;
        }
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("ToolbotBody");
        }
        private void OnInventoryChangedGlobal(Inventory inventory)
        {
           CharacterMaster master = inventory.GetComponent<CharacterMaster>();
           if(master && localUser.cachedMaster == master)
           {
                if(inventory.HasAtLeastXTotalItemsOfTier(ItemTier.Tier2, 20))
                {
                    Grant();
                }
           }
        }
    }
}