﻿using RoR2;
using RoR2.Achievements;
namespace SS2.Unlocks.Chirr
{
    public sealed class ChirrAchievement : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            base.SetServerTracked(true);
        }
        private class ChirrServerAchievement : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                GlobalEventManager.onCharacterDeathGlobal += CheckEmpyrean;
            }

            private void CheckEmpyrean(DamageReport damageReport)
            {
                if (damageReport.attackerTeamIndex == TeamIndex.Player && damageReport.victimIsElite && damageReport.victimBody.HasBuff(SS2Content.Buffs.bdEmpyrean))
                {
                    this.Grant();
                }
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
                GlobalEventManager.onCharacterDeathGlobal -= CheckEmpyrean;
            }
        }
    }            
}