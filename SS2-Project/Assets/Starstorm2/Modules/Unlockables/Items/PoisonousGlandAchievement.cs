using RoR2;
using RoR2.Achievements;
using RoR2.Stats;
using SS2.Stats;
namespace SS2.Unlocks.Pickups
{
    public sealed class PoisonousGlandAchievement : BaseAchievement
    {
        
        public override void OnInstall()
        {
            base.OnInstall();          
            base.userProfile.onStatsReceived += Check;
            base.SetServerTracked(true);
        }       
        private void Check()
        {
            if (base.userProfile.statSheet.GetStatValueULong(SS2StatDefs.crocoPoisonedEnemies) >= 500)
            {
                base.Grant();
            }
        }
        public override float ProgressForAchievement()
        {
            return base.userProfile.statSheet.GetStatValueULong(SS2StatDefs.crocoPoisonedEnemies) / 500f;
        }
        public override void OnUninstall()
        {                   
            base.userProfile.onStatsReceived -= Check;
            base.OnUninstall();
        }

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("CrocoBody");
        }

		private class PoisonousGlandServerAchievement : BaseServerAchievement
		{
			public override void OnInstall()
			{
				base.OnInstall();
				this.crocoBodyIndex = BodyCatalog.FindBodyIndex("CrocoBody");
                DotController.onDotInflictedServerGlobal += CheckCrocoPoison;
            }

			public override void OnUninstall()
			{
                DotController.onDotInflictedServerGlobal -= CheckCrocoPoison;
                base.OnUninstall();
			}
            private void CheckCrocoPoison(DotController dotController, ref InflictDotInfo inflictDotInfo)
            {
                if (inflictDotInfo.dotIndex != DotController.DotIndex.Poison) return;
                CharacterBody body = inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
                if(base.IsCurrentBody(body) && body.bodyIndex == crocoBodyIndex)
                {
                    PlayerStatsComponent masterPlayerStatsComponent = base.networkUser.masterPlayerStatsComponent;
                    if (masterPlayerStatsComponent)
                    {
                        masterPlayerStatsComponent.currentStats.PushStatValue(SS2StatDefs.crocoPoisonedEnemies, 1);
                    }
                }
            }
            private BodyIndex crocoBodyIndex;
		}
	}
}