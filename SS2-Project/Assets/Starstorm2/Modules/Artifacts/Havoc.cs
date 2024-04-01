using RoR2;
using R2API.ScriptableObjects;
namespace SS2.Artifacts
{
    //[DisabledContent]
    public class Havoc : ArtifactBase
    {
        public override ArtifactDef ArtifactDef { get; } = SS2Assets.LoadAsset<ArtifactDef>("Havoc", SS2Bundle.Artifacts);

        public override ArtifactCode ArtifactCode { get; } = SS2Assets.LoadAsset<ArtifactCode>("HavocCode", SS2Bundle.Artifacts);

        public override void OnArtifactDisabled()
        { }

        public override void OnArtifactEnabled()
        { }
    }
}
