using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.NemCommando
{
    /*
    public sealed class NemCommandoBeamUnlock : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skill.nemcommando.swordbeam", SS2Bundle.NemCommando);

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

            private void CheckBleedChance()
            {
                if (localUser != null && localUser.cachedBody && localUser.cachedBody.bleedChance >= 100f)
                {
                    Grant();
                }
            }
        }
    }
    */
}