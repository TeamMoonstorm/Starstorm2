using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    [DisabledContent]
    public sealed class Intoxicated : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffIntoxicated", SS2Bundle.Indev);
        public static BuffDef buff;
        public static DotController.DotIndex index;

        public override void Initialize()
        {
            buff = BuffDef;
            index = DotAPI.RegisterDotDef(.5f, 0.5f, DamageColorIndex.Item, BuffDef);
        }

        /*public sealed class Behavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffIntoxicated;
            private AimNoise aimNoise;


            private void Start()
            {
                if (body.hasAuthority)
                {
                    if ((bool)body.master.GetComponent<PlayerStatsComponent>())
                    {
                        foreach (var camera in CameraRigController.readOnlyInstancesList)
                        {
        //camera mode is no longer an enum, this is broken.
                            if (camera.target == gameObject && camera.cameraMode.HasFlag(CameraRigController.CameraMode.PlayerBasic))
                            {
                                aimNoise = camera.gameObject.AddComponent<AimNoise>();
                                aimNoise.cameraRigController = camera;
                                aimNoise.intensity = buffStacks;
                                return;
                            }
                        }
                    }
                }
            }

            private void FixedUpdate()
            {
                aimNoise.intensity = buffStacks;
            }

            private void OnDestroy()
            {
                if (aimNoise)
                    aimNoise.SetToDestroy(Mathf.Log(aimNoise.intensity + 1f) / 3f);
            }
        }*/
    }
}
