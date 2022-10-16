using RoR2;
using UnityEngine;

namespace EntityStates.Beastmaster.Weapon
{
    public class FireHitscanDart : GenericBulletBaseState
    {
        [SerializeField]
        public bool isPiercing;

        public override void ModifyBullet(BulletAttack bulletAttack)
        {
            if (this.isPiercing)
            {
                bulletAttack.stopperMask = LayerIndex.world.mask;
            }
            bulletAttack.modifyOutgoingDamageCallback = delegate (BulletAttack _bulletAttack, ref BulletAttack.BulletHit hitInfo, DamageInfo damageInfo)
            {
                HealthComponent healthComponent = null;
                if (hitInfo.hitHurtBox)
                {
                    healthComponent = hitInfo.hitHurtBox.healthComponent;
                }
                if (healthComponent && !FriendlyFireManager.ShouldDirectHitProceed(healthComponent, characterBody.teamComponent.teamIndex))
                {
                    healthComponent.TakeDamage(damageInfo);
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, hitInfo.entityObject);
                }
                GlobalEventManager.instance.OnHitAll(damageInfo, hitInfo.entityObject);
            };
        }
    }
}