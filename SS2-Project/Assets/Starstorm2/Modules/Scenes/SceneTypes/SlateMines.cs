using RoR2;

using Moonstorm;
namespace SS2.Scenes
{
    [DisabledContent]
    public sealed class SlateMines : SceneBase
    {
        public override SceneDef SceneDef { get; } = SS2Assets.LoadAsset<SceneDef>("slatemines", SS2Bundle.Indev);
    }
}
