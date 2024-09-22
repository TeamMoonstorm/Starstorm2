using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.NemCommando
{  
    public sealed class NemCommandoBeamAchievement : BaseAchievement
    {
        private BodyIndex bodyIndex;
        public override void OnInstall()
        {
            base.OnInstall();
            RoR2Application.onUpdate += CheckBleedChance;
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
            RoR2Application.onUpdate -= CheckBleedChance;
        }

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            bodyIndex = BodyCatalog.FindBodyIndex("NemCommandoBody");
            return bodyIndex;
        }

        private void CheckBleedChance()
        {
            if (localUser != null && localUser.cachedBody && localUser.cachedBody.bodyIndex == bodyIndex && localUser.cachedBody.bleedChance >= 100f)
            {
                Grant();
            }
        }
    }        
}