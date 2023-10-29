using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.Projectile;

namespace Moonstorm.Starstorm2.Survivors
{
	[DisabledContent]
    public sealed class MULE : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("MULEBody", SS2Bundle.Indev);
        public override GameObject MasterPrefab { get; } = null;
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("survivorMULE", SS2Bundle.Indev);

        private GameObject projectilePrefab;
        

        public override void ModifyPrefab()
        {
            base.ModifyPrefab();
            //ModifyPrimaryProjectile();

            var cb = BodyPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/RoboCratePod.prefab").WaitForCompletion(); ;
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }

        public void ModifyPrimaryProjectile()
        {
            
        }
    }
}
