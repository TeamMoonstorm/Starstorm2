using UnityEngine;
using static R2API.DamageAPI;
namespace SS2.Projectiles
{
    public sealed class NemmandoDistantGashBlue : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemCommandoSwordBeamProjectileBlue", SS2Bundle.NemCommando);

        public override void Initialize()
        {
            var damageAPIComponent = ProjectilePrefab.AddComponent<ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(DamageTypes.Gouge.gougeDamageType);
        }
    }
}
