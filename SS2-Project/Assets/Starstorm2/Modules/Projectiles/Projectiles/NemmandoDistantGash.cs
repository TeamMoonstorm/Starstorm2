using UnityEngine;
using static R2API.DamageAPI;

namespace Moonstorm.Starstorm2.Projectiles
{
    public sealed class NemmandoDistantGash : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemCommandoSwordBeamProjectile", SS2Bundle.NemCommando);

        public override void Initialize()
        {
            var damageAPIComponent = ProjectilePrefab.AddComponent<ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(DamageTypes.Gouge.gougeDamageType);

            //i was gonna add the yellow projectile here but i think it's just more effort than it's worth.
            //it's a very small optimization just to change the hit effect to also be yellow.
            //ugh.
            //i'm also adding an additional line to this comment because dotflare was complaining.
            //shut up dot ily - N
        }
    }
}
