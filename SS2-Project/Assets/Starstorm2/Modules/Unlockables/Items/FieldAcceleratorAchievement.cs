using RoR2;
using RoR2.Achievements;
using RoR2.Stats;
namespace SS2.Unlocks.Pickups
{
	public class FieldAcceleratorAchievement : BaseAchievement
	{
		public override BodyIndex LookUpRequiredBodyIndex()
		{
			return BodyCatalog.FindBodyIndex("MageBody");
		}

		public override void OnBodyRequirementMet()
		{
			base.OnBodyRequirementMet();
			base.SetServerTracked(true);
		}
		public override void OnBodyRequirementBroken()
		{
			base.SetServerTracked(false);
			base.OnBodyRequirementBroken();
		}

		public class FieldAcceleratorServerAchievement : BaseServerAchievement
		{
			public override void OnInstall()
			{
				base.OnInstall();
				GlobalEventManager.onCharacterDeathGlobal += this.OnCharacterDeath;
			}

			public override void OnUninstall()
			{
				GlobalEventManager.onCharacterDeathGlobal -= this.OnCharacterDeath;
				base.OnUninstall();
			}

			private void OnCharacterDeath(DamageReport damageReport)
			{
				if (damageReport.victimIsBoss && damageReport.victimIsChampion && damageReport.attackerBody && (damageReport.damageInfo.position - damageReport.attacker.transform.position).sqrMagnitude <= 10000 && IsCurrentBody(damageReport.attackerBody))
				{
					Grant();					
				}
			}
		}
	}
}
