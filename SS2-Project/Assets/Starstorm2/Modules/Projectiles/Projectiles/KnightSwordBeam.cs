using UnityEngine;
using static R2API.DamageAPI;

namespace Moonstorm.Starstorm2.Projectiles
{
    public sealed class KnightSwordBeam : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; } = SS2Assets.LoadAsset<GameObject>("KnightSwordBeamProjectile", SS2Bundle.Indev);
    }
}
