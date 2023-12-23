using RoR2;
using RoR2.Achievements;
using UnityEngine.SceneManagement;
using UnityEngine;
namespace Moonstorm.Starstorm2.Unlocks.Chirr
{

    public sealed class ChirrUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.survivor.chirr", SS2Bundle.Chirr);

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
                    throw new System.NotImplementedException();
                }

                public override void OnUninstall()
                {
                    base.OnUninstall();
                }

                private void OnNemMercenaryDefeated(CharacterBody obj)
                {
                    if (obj.bodyIndex == nemMercBodyIndex)
                    {
                        Grant();
                    }
                }
            }
        }
        
    }
}