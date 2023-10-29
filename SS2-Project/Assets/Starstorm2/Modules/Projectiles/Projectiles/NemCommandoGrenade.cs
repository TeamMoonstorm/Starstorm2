using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.DamageAPI;

namespace Moonstorm.Starstorm2.Projectiles
{
    public sealed class NemCommandoGrenade : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemCommandoGrenadeProjectile", SS2Bundle.NemCommando);

        public override void Initialize()
        {
            var pie = ProjectilePrefab.GetComponent<ProjectileImpactExplosion>();
            pie.impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/OmniExplosionVFXCommandoGrenade.prefab").WaitForCompletion();
        }
    }
}
