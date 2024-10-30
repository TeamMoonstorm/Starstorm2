using RoR2;
using RoR2.Achievements;
using RoR2.Stats;
namespace SS2.Unlocks.Pickups
{
    public sealed class PoisonousGlandAchievement : BaseAchievement
    {
        public static readonly StatDef crocoPoisonedEnemies = StatDef.Register("crocoPoisonEnemeisAchievementProgress", StatRecordType.Sum, StatDataType.ULong, 0.0, null);
        public override void OnInstall()
        {
            base.OnInstall();          
            base.userProfile.onStatsReceived += Check;
            base.SetServerTracked(true);
        }       
        private void Check()
        {
            if (base.userProfile.statSheet.GetStatValueULong(crocoPoisonedEnemies) >= 500)
            {
                base.Grant();
            }
        }
        public override float ProgressForAchievement()
        {
            return base.userProfile.statSheet.GetStatValueULong(crocoPoisonedEnemies) / 500;
        }
        public override void OnUninstall()
        {
            base.OnUninstall();           
            base.userProfile.onStatsReceived -= Check;
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
                CharacterBody body = base.networkUser.GetCurrentBody();
                if(base.IsCurrentBody(body) && body.bodyIndex == crocoBodyIndex)
                {
                    PlayerStatsComponent masterPlayerStatsComponent = base.networkUser.masterPlayerStatsComponent;
                    if (masterPlayerStatsComponent)
                    {
                        masterPlayerStatsComponent.currentStats.PushStatValue(crocoPoisonedEnemies, 1);
                    }
                }
            }
            private BodyIndex crocoBodyIndex;
		}
	}
}