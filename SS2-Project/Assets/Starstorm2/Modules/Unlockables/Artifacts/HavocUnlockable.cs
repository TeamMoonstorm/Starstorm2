using RoR2;
using RoR2.Achievements.Artifacts;

namespace Moonstorm.Starstorm2.Unlocks.Artifacts
{
    public sealed class HavocUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.artifact.havoc", SS2Bundle.Artifacts);

        public override void Initialize()
        {
            AddRequiredType<Starstorm2.Artifacts.Havoc>();
        }

        public sealed class HavocAchievement : BaseObtainArtifactAchievement
        {
            public override ArtifactDef artifactDef => SS2Content.Artifacts.Havoc;
        }
    }
}