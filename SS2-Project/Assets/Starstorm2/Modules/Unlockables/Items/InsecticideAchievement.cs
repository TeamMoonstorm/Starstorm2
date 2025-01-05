using RoR2;
using RoR2.Achievements;
using RoR2.Stats;
namespace SS2.Unlocks.Pickups
{
	// i dont like this one. too many are just "get x kills"
	public class InsecticideAchievement : BaseAchievement
	{
		public override BodyIndex LookUpRequiredBodyIndex()
		{
			return BodyCatalog.FindBodyIndex("TreebotBody");
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

		public class InsecticideServerAchievement : BaseServerAchievement
		{
			public override void OnInstall()
			{
				base.OnInstall();
				beetleQueenIndex = BodyCatalog.FindBodyIndex("BeetleQueenBody");
				GlobalEventManager.onCharacterDeathGlobal += this.OnCharacterDeath;
			}

			public override void OnUninstall()
			{
				GlobalEventManager.onCharacterDeathGlobal -= this.OnCharacterDeath;
				base.OnUninstall();
			}

			private void OnCharacterDeath(DamageReport damageReport)
			{
				if (damageReport.victimBodyIndex == beetleQueenIndex && damageReport.victimIsElite)
				{
					if(IsCurrentBody(damageReport.attackerBody))
                    {
						Grant();
                    }
				}
			}

			private BodyIndex beetleQueenIndex;
		}
	}
}
