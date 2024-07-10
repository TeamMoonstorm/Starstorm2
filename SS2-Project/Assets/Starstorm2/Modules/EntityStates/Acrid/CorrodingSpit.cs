using EntityStates;
using RoR2.Projectile;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.Acrid
{
    public class CorrodingSpit : BaseSkillState
    {
        public GameObject effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/MuzzleflashCroco.prefab").WaitForCompletion();

        [SerializeField]
        public float baseDuration = 2f;

        [SerializeField]
        public float damageCoefficient = 1.4f;

        [SerializeField]
        public float force = 20f;

        [SerializeField]
        public string attackString;

        [SerializeField]
        public float recoilAmplitude;

        [SerializeField]
        public float bloom;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            Ray aimRay = GetAimRay();
            duration = baseDuration / attackSpeedStat;
            StartAimMode(duration + 2f);
            PlayAnimation("Gesture, Mouth", "FireSpit", "FireSpit.playbackRate", duration);
            Util.PlaySound(attackString, base.gameObject);
            AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
            base.characterBody.AddSpreadBloom(bloom);
            string muzzleName = "MouthMuzzle";

            if (effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
            }

            if (base.isAuthority)
            {
                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                fireProjectileInfo.projectilePrefab = SS2.Survivors.Acrid.corrodingSpitProjectilePrefab;
                fireProjectileInfo.position = aimRay.origin;
                fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
                fireProjectileInfo.owner = base.gameObject;
                fireProjectileInfo.damage = damageStat * damageCoefficient;
                fireProjectileInfo.force = force;
                fireProjectileInfo.crit = Util.CheckRoll(critStat, base.characterBody.master);

                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
