using UnityEngine;
using static R2API.DamageAPI;

namespace Moonstorm.Starstorm2.Projectiles
{
    public sealed class NemmandoDistantGashClassic : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemmandoSwordBeamProjectile", SS2Bundle.Nemmando);

        public override void Initialize()
        {
            var damageAPIComponent = ProjectilePrefab.AddComponent<ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(DamageTypes.Gouge.gougeDamageType);
        }
    }
}
