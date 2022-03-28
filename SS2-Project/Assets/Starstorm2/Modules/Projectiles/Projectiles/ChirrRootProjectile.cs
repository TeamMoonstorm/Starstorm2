using UnityEngine;
using static R2API.DamageAPI;

namespace Moonstorm.Starstorm2.Projectiles
{
    [DisabledContent]
    public sealed class ChirrRootProjectile : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; } = SS2Assets.LoadAsset<GameObject>("ChirrRootProjectile");

        public override GameObject ProjectileGhost { get; } = null;

        public override void Initialize()
        {
            Debug.LogWarning(ProjectilePrefab);
            Debug.LogWarning(SS2Assets.LoadAsset<GameObject>("ChirrRootProjectile"));
            var damageAPIComponent = ProjectilePrefab.AddComponent<ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(DamageTypes.Root.rootDamageType);
        }
    }
}
