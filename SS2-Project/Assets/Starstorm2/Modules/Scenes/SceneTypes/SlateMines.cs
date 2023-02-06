using RoR2;

namespace Moonstorm.Starstorm2.Scenes
{
    [DisabledContent]
    public sealed class SlateMines : SceneBase
    {
        public override SceneDef SceneDef { get; } = SS2Assets.LoadAsset<SceneDef>("slatemines", SS2Bundle.Indev);
    }
}
