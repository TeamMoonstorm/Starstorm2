using RoR2;
using RoR2.Achievements.Artifacts;
namespace SS2.Unlocks.Artifacts
{
    public sealed class DeviationAchievement : BaseObtainArtifactAchievement
    {
        public override ArtifactDef artifactDef => SS2Content.Artifacts.Deviation;
    }
}