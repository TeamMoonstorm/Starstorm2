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
            public override void OnInstall()
            {
                base.OnInstall();
                SetServerTracked(true);
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
            }

            private class NemCommandoBeamUnlockServerAchievement : BaseServerAchievement
            {
                public BodyIndex nemCommandoBodyIndex
                {
                    get
                    {
                        var nemCommandoBodyPrefab = SS2Assets.LoadAsset<GameObject>("NemCommandoBody", SS2Bundle.NemCommando);
                        if (nemCommandoBodyPrefab)
                        {
                            return nemCommandoBodyPrefab.GetComponent<CharacterBody>().bodyIndex;
                        }
                        return BodyIndex.None;
                    }
                }

                public override void OnInstall()
                {
                    base.OnInstall();
                    RoR2Application.onUpdate += CheckBleedChance;
                }

                public override void OnUninstall()
                {
                    RoR2Application.onUpdate -= CheckBleedChance;
                    base.OnUninstall();
                }

                private void CheckBleedChance()
                {
                    if (networkUser != null && networkUser.GetCurrentBody() != null)
                    {
                        if (networkUser.GetCurrentBody().bodyIndex == nemCommandoBodyIndex)
                        {
                            if (networkUser.GetCurrentBody().bleedChance >= 100f)
                            {
                                Grant();
                            }
                        }
                    }
                }
            }
        }
    }
}