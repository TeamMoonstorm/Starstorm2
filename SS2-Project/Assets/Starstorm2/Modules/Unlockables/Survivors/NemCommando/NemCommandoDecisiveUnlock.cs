using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.NemCommando
{
    public sealed class NemCommandoDecisiveAchievement : BaseAchievement
    {
        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            SetServerTracked(true);
        }
        public override void OnBodyRequirementBroken()
        {
            base.OnBodyRequirementBroken();
            SetServerTracked(false);
        }
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("NemCommandoBody");
        }

        private class NemCommandoDecisiveServerAchievement : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                On.RoR2.CharacterBody.AddBuff_BuffIndex += Check;
            }

            private void Check(On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
            {
                if (self.GetBuffCount(SS2Content.Buffs.BuffGouge) >= 8f)
                {
                    Grant();
                }             
                orig(self, buffType);
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
                On.RoR2.CharacterBody.AddBuff_BuffIndex -= Check;
            }
        }
    }
    
}