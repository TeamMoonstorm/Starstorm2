using EntityStates;
using EntityStates.Bandit2.Weapon;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Starstorm2.Modules.EntityStates.Bandit
{
    public class TranquilizerGun : Bandit2FirePrimaryBase
    {
        public override void ModifyBullet(BulletAttack bulletAttack)
        {
            base.ModifyBullet(bulletAttack);
            bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
            bulletAttack.damageType = DamageType.Stun1s;
        }
    }
}