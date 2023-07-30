using RoR2;
using RoR2.Projectile;
using UnityEngine;
using static R2API.DamageAPI;

namespace Moonstorm.Starstorm2.Projectiles
{
    public sealed class NemHuntressExplosiveArrow : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemHuntressExplosiveArrowProjectile", SS2Bundle.Indev);

        public override void Initialize()
        {
            var damageAPIComponent = ProjectilePrefab.AddComponent<ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(DamageTypes.WeakPointProjectile.weakPointProjectile);
        }
    }
}
