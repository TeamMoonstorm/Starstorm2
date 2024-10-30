using RoR2;
using RoR2.Achievements;
namespace SS2.Unlocks.Pickups
{
    public sealed class BlastKnucklesAchievement : BaseAchievement
    {
        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            GlobalEventManager.onCharacterDeathGlobal += CheckDamage;
        }
        public override void OnBodyRequirementBroken()
        {
            base.OnBodyRequirementBroken();
            GlobalEventManager.onCharacterDeathGlobal -= CheckDamage;
        }      

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("LoaderBody");
        }

        private void CheckDamage(DamageReport damageReport)
        {
            CharacterBody body = damageReport.victimBody;
            HealthComponent healthComponent = body.healthComponent;
            DamageSource damageSource = damageReport.damageInfo.damageType.damageSource;
            if (body.isBoss && body.isChampion // is boss
                && healthComponent.serverDamageTakenThisUpdate >= healthComponent.fullCombinedHealth // is 1shot
                && damageReport.attacker == localUser.cachedBodyObject
                && (damageSource == DamageSource.Utility || damageSource == DamageSource.Primary)) // is punch
            {
                 Grant();                   
            }
        }
    }
}