using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.NemCommando
{  
    public sealed class NemCommandoBeamAchievement : BaseAchievement
    {
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
            return BodyCatalog.FindBodyIndex("NemCommandoBody");
        }

        private void CheckBleedChance()
        {
            if (localUser != null && localUser.cachedBody && localUser.cachedBody.bleedChance >= 100f)
            {
                Grant();
            }
        }
    }        
}