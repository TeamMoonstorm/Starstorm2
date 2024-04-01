using RoR2;
namespace SS2.Scenes
{
    [DisabledContent]
    public sealed class TorridOutlands : SceneBase
    {
        public override SceneDef SceneDef { get; } = SS2Assets.LoadAsset<SceneDef>("torridoutlands", SS2Bundle.Indev);
    }
}
