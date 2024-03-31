using UnityEngine;
using static R2API.DamageAPI;

using Moonstorm;
namespace SS2.Projectiles
{
    public sealed class SauceBottle : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; } = SS2Assets.LoadAsset<GameObject>("SauceProjectile", SS2Bundle.Items);
    }
}
