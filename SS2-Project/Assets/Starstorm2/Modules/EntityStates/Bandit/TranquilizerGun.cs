using EntityStates.Bandit2.Weapon;
using R2API;
using RoR2;

namespace EntityStates.Bandit
{
    public class TranquilizerGun : Bandit2FirePrimaryBase
    {
        public override void ModifyBullet(BulletAttack bulletAttack)
        {
            base.ModifyBullet(bulletAttack);
            bulletAttack.falloffModel = BulletAttack.FalloffModel.None;

            bulletAttack.AddModdedDamageType(SS2.Survivors.Bandit.TranqDamageType);
        }
    }
}