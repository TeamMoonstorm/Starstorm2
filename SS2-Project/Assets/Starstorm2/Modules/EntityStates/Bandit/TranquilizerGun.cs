using EntityStates.Bandit2.Weapon;
using R2API;
using RoR2;
using UnityEngine;

namespace EntityStates.Bandit
{
    public class TranquilizerGun : Bandit2FirePrimaryBase
    {
        public override void ModifyBullet(BulletAttack bulletAttack)
        {
            bulletAttack.tracerEffectPrefab = SS2.Survivors.Bandit.tranqMuzzleFlashPrefab;
            bulletAttack.hitEffectPrefab = SS2.Survivors.Bandit.tranqMuzzleFlashPrefab;

            Debug.Log("DEBUGGER Is tranq null?");
            Debug.Log(SS2.Survivors.Bandit.TranqDamageType);

            bulletAttack.AddModdedDamageType(SS2.Survivors.Bandit.TranqDamageType);
            base.ModifyBullet(bulletAttack);
        }
    }
}