using RoR2;
using RoR2.Achievements;
using UnityEngine.SceneManagement;

namespace Moonstorm.Starstorm2.Unlocks.Chirr
{

    [DisabledContent]

    public sealed class ChirrUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.survivor.beastmaster", SS2Bundle.Indev);

        public sealed class ChirrAchievement : BaseAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                On.RoR2.ChestBehavior.Open += TryUnlock;
            }

            public override void OnUninstall()
            {
                On.RoR2.ChestBehavior.Open -= TryUnlock;
                base.OnUninstall();
            }
            private void TryUnlock(On.RoR2.ChestBehavior.orig_Open orig, ChestBehavior chest)
            {
                orig(chest);

                if (SceneManager.GetActiveScene().name == "rootJungle")
                {
                    if (chest.gameObject.transform.parent?.parent?.name == "GROUP: Large Treasure Chests")
                    {
                        Grant();
                    }
                }
            }
        }
    }
}