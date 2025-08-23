using RoR2;
using RoR2.Achievements;
using RoR2.Stats;
using SS2.Stats;
namespace SS2.Unlocks.Pickups
{	
	public class UniversalChargerAchievement : BaseAchievement
	{
		
		public override BodyIndex LookUpRequiredBodyIndex()
		{
			return BodyCatalog.FindBodyIndex("EngiBody");
		}

		public override void OnBodyRequirementMet()
		{
			base.OnBodyRequirementMet();
			base.userProfile.onStatsReceived += Check;
			base.SetServerTracked(true);
		}
		public override float ProgressForAchievement()
		{
			return base.userProfile.statSheet.GetStatValueULong(SS2StatDefs.engiTurretKills) / 400f;
		}
		public override void OnBodyRequirementBroken()
		{
			base.SetServerTracked(false);
			base.userProfile.onStatsReceived -= Check;
			base.OnBodyRequirementBroken();
		}
		private void Check()
		{
			if (base.userProfile.statSheet.GetStatValueULong(SS2StatDefs.engiTurretKills) >= 400)
			{
				base.Grant();
			}
		}

		public class UniversalChargerServerAchievement : BaseServerAchievement
		{
			public override void OnInstall()
			{
				base.OnInstall();
				turret1BodyIndex = BodyCatalog.FindBodyIndex("EngiTurretBody");
				turret2BodyIndex = BodyCatalog.FindBodyIndex("EngiWalkerTurretBody");
				GlobalEventManager.onCharacterDeathGlobal += this.OnCharacterDeath;
			}

			public override void OnUninstall()
			{
				GlobalEventManager.onCharacterDeathGlobal -= this.OnCharacterDeath;
				base.OnUninstall();
			}

			private void OnCharacterDeath(DamageReport damageReport)
			{
				if (damageReport.attackerBodyIndex == turret1BodyIndex || damageReport.attackerBodyIndex == turret2BodyIndex)
				{
					CharacterMaster master = damageReport.attackerOwnerMaster;
					if(master && IsCurrentBody(master.GetBody()))
                    {
						PlayerStatsComponent masterPlayerStatsComponent = base.networkUser.masterPlayerStatsComponent;
						if (masterPlayerStatsComponent)
						{
							masterPlayerStatsComponent.currentStats.PushStatValue(SS2StatDefs.engiTurretKills, 1);
						}
					}					
				}
			}

			private BodyIndex turret1BodyIndex;
			private BodyIndex turret2BodyIndex;
		}
	}
}
