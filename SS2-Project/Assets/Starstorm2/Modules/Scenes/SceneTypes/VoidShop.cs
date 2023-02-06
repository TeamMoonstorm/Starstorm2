using RoR2;

namespace Moonstorm.Starstorm2.Scenes
{
    [DisabledContent]
    public sealed class VoidShop : SceneBase
    {
        public override SceneDef SceneDef { get; } = SS2Assets.LoadAsset<SceneDef>("voidshop", SS2Bundle.Indev);
    }
}
