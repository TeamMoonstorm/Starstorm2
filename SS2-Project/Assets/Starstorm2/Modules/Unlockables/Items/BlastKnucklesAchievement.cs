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
        // todo: check loader (nem loader?) util damagesource
        private void CheckDamage(DamageReport damageReport)
        {
            CharacterBody body = damageReport.victimBody;
            HealthComponent healthComponent = body.healthComponent;
            if (body.isBoss && body.isChampion
                && healthComponent.serverDamageTakenThisUpdate >= healthComponent.fullCombinedHealth
                && damageReport.attacker == localUser.cachedBodyObject) // && damage source
            {
                 Grant();                   
            }
        }
    }
}