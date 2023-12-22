using RoR2;
using RoR2.Achievements.Artifacts;

namespace Moonstorm.Starstorm2.Unlocks.Artifacts
{

    [DisabledContent]

    public sealed class DeviationUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.artifact.deviation", SS2Bundle.Artifacts);

        public override void Initialize()
        {
            AddRequiredType<Starstorm2.Artifacts.Deviation>();
        }

        public sealed class DeviationAchievement : BaseObtainArtifactAchievement
        {
            public override ArtifactDef artifactDef => SS2Content.Artifacts.Deviation;
        }
    }
}