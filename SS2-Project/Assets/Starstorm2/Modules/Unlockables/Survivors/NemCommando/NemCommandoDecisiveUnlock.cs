using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.NemCommando
{
    public sealed class NemCommandoDecisiveUnlock : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skill.nemcommando.bossattack", SS2Bundle.NemCommando);

        public sealed class NemCommandoDecisiveAchievement : BaseAchievement
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

            private class NemCommandoDecisiveServerAchievement : BaseServerAchievement
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
                    On.RoR2.CharacterBody.AddBuff_BuffIndex += Check;
                }

                private void Check(On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
                {
                    if (self)
                    {
                        if (self.GetBuffCount(SS2Content.Buffs.BuffGouge) >= 8f && networkUser.GetCurrentBody().bodyIndex == nemCommandoBodyIndex)
                        {
                            Grant();
                        }
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
}