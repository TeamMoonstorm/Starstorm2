using RoR2;
using RoR2.Achievements;
using System.Collections.Generic;
namespace SS2.Unlocks.Pickups
{
	public class X4Achievement : BaseAchievement
	{
		public override BodyIndex LookUpRequiredBodyIndex()
		{
			return BodyCatalog.FindBodyIndex("HuntressBody");
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

		public class X4ServerAchievement : BaseServerAchievement
		{
			private List<BodyIndex> uniqueBossKills;
			public override void OnInstall()
			{
				base.OnInstall();
                Run.onRunDestroyGlobal += Reset;
				GlobalEventManager.onCharacterDeathGlobal += this.OnCharacterDeath;
			}

            private void Reset(Run obj)
            {
				uniqueBossKills.Clear();
            }

            public override void OnUninstall()
			{
				GlobalEventManager.onCharacterDeathGlobal -= this.OnCharacterDeath;
				Run.onRunDestroyGlobal -= Reset;
				base.OnUninstall();
			}

			private void OnCharacterDeath(DamageReport damageReport)
			{
				if (damageReport.victimIsChampion && IsCurrentBody(damageReport.attackerBody) && !uniqueBossKills.Contains(damageReport.victimBodyIndex))
				{
					uniqueBossKills.Add(damageReport.victimBodyIndex);
					if (uniqueBossKills.Count >= 7) Grant();
				}
			}
		}
	}
}
