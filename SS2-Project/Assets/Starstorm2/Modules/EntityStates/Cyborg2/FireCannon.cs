using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
using Moonstorm.Starstorm2;

namespace EntityStates.Cyborg2
{
    public class FireCannon : BaseSkillState
    {
        public static float baseFireDuration = 0.4f;
        public static float baseExtraDuration = 0.25f;
        public static float bulletMaxDistance = 256;
        public static float bulletRadius = 1;
        public static float extraDurationPerShot = 0.05f;

        [NonSerialized]
        private static GameObject TRACERTEMP = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/TracerHuntressSnipe.prefab").WaitForCompletion();

        public static float maxDamageCoefficient = 6f;
        public static float minDamageCoefficient = 3f;
        public static int numShotsForMaxDamage = 5;

        public static float procCoefficient = 1f;
        public static float force = 150f;
        public static GameObject tracerPrefab;
        public static GameObject hitEffectPrefab;
        public static GameObject muzzleFlashPrefab;
        public static string fireSoundString = "Play_MULT_m1_snipe_shoot"; //"Play_MULT_m1_snipe_shoot"
        public static float recoil = 1;
        public static float bloom = .4f;
        public static float fireSoundPitch = 1;

        private float fireTime;
        private float fireStopwatch;
        private int numShots;
        private int shotsFired;
        private float duration;
        private bool hasFiredSecondShot;
        public override void OnEnter()
        {
            base.OnEnter();

            tracerPrefab = TRACERTEMP;

            numShots = 1 + base.characterBody.GetBuffCount(SS2Content.Buffs.BuffCyborgPrimary);
            float baseFireDuration = FireCannon.baseFireDuration + numShots * FireCannon.extraDurationPerShot;      
            float baseDuration = baseFireDuration + FireCannon.baseExtraDuration;
            duration = baseDuration / base.attackSpeedStat;
            fireTime = baseFireDuration / numShots / base.attackSpeedStat;
            base.StartAimMode();

        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            fireStopwatch -= Time.fixedDeltaTime;
            if(fireStopwatch <= 0 && shotsFired < numShots)
            {
                fireStopwatch = fireTime;
                Fire(shotsFired % 2 == 1 ? "CannonR" : "CannonL");

            }

            if (base.fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            if (shotsFired < numShots)
            {
                Fire(shotsFired % 2 == 1 ? "CannonR" : "CannonL");
            }

            base.OnExit();
        }
        private void Fire(string muzzleName)
        {
            shotsFired++;

            if(shotsFired > 1)
            {
                base.characterBody.ClearTimedBuffs(SS2Content.Buffs.BuffCyborgPrimary);
            }

            
            EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, base.gameObject, muzzleName, false);
            //anim
            base.characterBody.AddSpreadBloom(bloom);
            base.AddRecoil(-1f * recoil, -1.5f * recoil, -1f * recoil, 1f * recoil);


            float t = Mathf.Clamp01((shotsFired - 1) / (numShotsForMaxDamage - 1));
            float damage = Mathf.Lerp(minDamageCoefficient, maxDamageCoefficient, t);
            Util.PlayAttackSpeedSound(fireSoundString, base.gameObject, fireSoundPitch * (t + 1));
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                new BulletAttack
                {
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    owner = base.gameObject,
                    damage = damageStat * damage,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    force = force,
                    HitEffectNormal = false,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    maxDistance = FireCannon.bulletMaxDistance,
                    radius = FireCannon.bulletRadius,
                    isCrit = base.RollCrit(),
                    muzzleName = muzzleName,
                    minSpread = 0,
                    maxSpread = 0,
                    hitEffectPrefab = hitEffectPrefab,
                    smartCollision = true,
                    tracerEffectPrefab = tracerPrefab
                }.Fire();
            }
            
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
