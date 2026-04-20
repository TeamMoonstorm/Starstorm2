using EntityStates.Knight;
using RoR2;
using RoR2.Projectile;
using System;
using UnityEngine;

namespace EntityStates.NemMage
{
    public class FireNeedle : BaseKnightMeleeAttack
    {
        public static float throwNeedleTime;
        public static GameObject needlePrefab;

        private bool _hasThrown;

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge > duration * throwNeedleTime && !_hasThrown)
            {
                _hasThrown = true;

                if (base.isAuthority)
                {
                    var aimRay = GetAimRay();
                    FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                    fireProjectileInfo.projectilePrefab = needlePrefab;
                    fireProjectileInfo.position = aimRay.origin;
                    fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
                    fireProjectileInfo.owner = base.gameObject;
                    fireProjectileInfo.damage = damageStat * damageCoefficient;
                    fireProjectileInfo.force = 0;
                    fireProjectileInfo.crit = Util.CheckRoll(critStat, base.characterBody.master);

                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
            }
        }
    }
}