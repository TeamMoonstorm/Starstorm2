using EntityStates.GlobalSkills.LunarNeedle;
using RoR2.Projectile;
using RoR2;
using UnityEngine;
using MSU.Config;
using SS2;

namespace EntityStates.Knight
{
    public class BuffedPrimaryLunar : BaseSkillState
    {
        public static float baseDuration = 0.5f;
        public static int projectileCount = 10;
        public static float maxSpread = 10;
        public static float damageCoefficient = 0.5f;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            if (base.isAuthority)
            {
                Ray aimRay = GetAimRay();
                for (int i = 0; i < projectileCount; i++)
                {
                    FireNeedle(aimRay);
                }
            }

            AddRecoil(-0.4f * FireLunarNeedle.recoilAmplitude, -0.8f * FireLunarNeedle.recoilAmplitude, -0.3f * FireLunarNeedle.recoilAmplitude, 0.3f * FireLunarNeedle.recoilAmplitude);
            base.characterBody.AddSpreadBloom(FireLunarNeedle.spreadBloomValue);
            StartAimMode();
            EffectManager.SimpleMuzzleFlash(FireLunarNeedle.muzzleFlashEffectPrefab, base.gameObject, "Head", transmit: false);
            Util.PlaySound(FireLunarNeedle.fireSound, base.gameObject);

            characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);

            //a custom animation perhaps
            PlayCrossfade("Gesture, Override", "BufferEmpty", 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge > duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
        }

        private void FireNeedle(Ray aimRay)
        {
            aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, maxSpread, 1f, 1f);
            FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
            fireProjectileInfo.position = aimRay.origin;
            fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
            fireProjectileInfo.crit = base.characterBody.RollCrit();
            fireProjectileInfo.damage = base.characterBody.damage * damageCoefficient;
            fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
            fireProjectileInfo.owner = base.gameObject;
            fireProjectileInfo.procChainMask = default(ProcChainMask);
            fireProjectileInfo.force = 0f;
            fireProjectileInfo.useFuseOverride = false;
            fireProjectileInfo.useSpeedOverride = false;
            fireProjectileInfo.target = null;
            fireProjectileInfo.projectilePrefab = FireLunarNeedle.projectilePrefab;
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }

}