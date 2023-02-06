using RoR2;
using RoR2.Achievements.Artifacts;

namespace Moonstorm.Starstorm2.Unlocks.Artifacts
{
    public sealed class CognationUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.artifact.cognation", SS2Bundle.Artifacts);

        public override void Initialize()
        {
            AddRequiredType<Starstorm2.Artifacts.Cognation>();
        }

        public sealed class CognationAchievement : BaseObtainArtifactAchievement
        {
            public override ArtifactDef artifactDef => SS2Content.Artifacts.Cognation;
        }
    }
}