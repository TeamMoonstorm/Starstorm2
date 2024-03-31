using UnityEngine;
using static R2API.DamageAPI;

using Moonstorm;
namespace SS2.Projectiles
{
    public sealed class KnightShieldBeam : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; } = SS2Assets.LoadAsset<GameObject>("KnightShieldBeamProjectile", SS2Bundle.Indev);
    }
}
