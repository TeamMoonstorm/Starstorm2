using RoR2;

using Moonstorm;
namespace SS2.Scenes
{
    [DisabledContent]
    public sealed class TorridOutlands : SceneBase
    {
        public override SceneDef SceneDef { get; } = SS2Assets.LoadAsset<SceneDef>("torridoutlands", SS2Bundle.Indev);
    }
}
