using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.NemCommando
{
    public sealed class NemCommandoBeamUnlock : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skill.nemcommando.swordbeam", SS2Bundle.NemCommando);

        public sealed class NemCommandoBeamAchievement : BaseAchievement
        {
            public override BodyIndex LookUpRequiredBodyIndex()
            {
                return BodyCatalog.FindBodyIndex("NemCommandoBody");
            }

            public override void OnBodyRequirementMet()
            {
                base.SetServerTracked(true);
                base.OnBodyRequirementMet();
            }
            public override void OnBodyRequirementBroken()
            {
                base.SetServerTracked(false);
                base.OnBodyRequirementBroken();
            }

            private class NemCommandoBeamUnlockServerAchievement : BaseServerAchievement
            {
                public override void OnInstall()
                {
                    base.OnInstall();
                    RoR2Application.onFixedUpdate += FixedUpdate;
                }

                public override void OnUninstall()
                {
                    base.OnUninstall();
                    RoR2Application.onFixedUpdate -= FixedUpdate;                    
                }

                private void FixedUpdate()
                {
                    if (base.GetCurrentBody().bleedChance >= 100f)
                    {
                        Grant();
                    }
                }
            }
        }
    }
}