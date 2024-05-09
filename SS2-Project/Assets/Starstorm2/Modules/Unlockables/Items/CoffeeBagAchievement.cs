using RoR2;
using RoR2.Achievements;
namespace SS2.Unlocks.Pickups
{
    public sealed class CoffeeBagAchievement : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            RoR2Application.onUpdate += CheckMovementAttackSpeed;
        }

        public override void OnUninstall()
        {
            RoR2Application.onUpdate -= CheckMovementAttackSpeed;
            base.OnUninstall();
        }

        private void CheckMovementAttackSpeed()
        {
            if (localUser != null && (bool)localUser.cachedBody)
            {
                if (localUser.cachedBody.moveSpeed / localUser.cachedBody.baseMoveSpeed >= 2.5f)
                {
                    if (localUser.cachedBody.attackSpeed / localUser.cachedBody.baseAttackSpeed >= 2.5f)
                    {
                        Grant();
                    }
                }
            }
        }
    }
}
