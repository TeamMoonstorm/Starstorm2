using RoR2;
using RoR2.Achievements;

namespace Moonstorm.Starstorm2.Unlocks.Nemmando
{
    public sealed class NemmandoBossAttackUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skill.nemmando.bossattack", SS2Bundle.Nemmando);

        public override void Initialize()
        {
            AddRequiredType<Survivors.Nemmando>();
            
        }
        public sealed class NemmandoBossAttackAchievement : BaseAchievement
        {
            public BodyIndex LookUpRequiredBodyIndex()
            {
                return BodyCatalog.FindBodyIndex("NemmandoBody");
            }

            public override void OnInstall()
            {
                //On.RoR2.CharacterBody.AddBuff_BuffIndex += Check;
            }

            private void Check(On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
            {
                if (self)
                {
                    if (self.GetBuffCount(SS2Content.Buffs.BuffGouge) >= 10)
                    {
                        //Grant();
                    }
                }
                orig(self, buffType);
            }

            public override void OnUninstall()
            {
                //On.RoR2.CharacterBody.AddBuff_BuffIndex -= Check;
            }
        }
    }
}