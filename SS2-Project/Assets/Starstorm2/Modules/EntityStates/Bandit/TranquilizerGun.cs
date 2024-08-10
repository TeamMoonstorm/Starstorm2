using EntityStates.Bandit2.Weapon;
using R2API;
using RoR2;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace EntityStates.Bandit
{
    public class TranquilizerGun : Bandit2FirePrimaryBase
    {
        public new GameObject muzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/MuzzleflashBandit2.prefab").WaitForCompletion();
        public new GameObject tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/TracerBandit2Rifle.prefab").WaitForCompletion();
        public new GameObject hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/HitsparkBandit.prefab").WaitForCompletion();

        public override void ModifyBullet(BulletAttack bulletAttack)
        {
            base.ModifyBullet(bulletAttack);
            bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
            bulletAttack.tracerEffectPrefab = muzzleFlashPrefab;
            bulletAttack.hitEffectPrefab = muzzleFlashPrefab;

            bulletAttack.AddModdedDamageType(SS2.Survivors.Bandit.TranqDamageType);
        }
    }
}