using EntityStates;
using MSU;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Starstorm2.Modules.EntityStates.Executioner2.Templar
{
    public class HolyWeapon : BaseSkillState
    {
        [FormatToken("SS2_EXECUTIONER_HOLY_WEAPON_DESCRIPTION", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float baseDuration;
        public static float recoil;
        public static float spreadBloom;
        public static float force;

        [HideInInspector]
        public static GameObject muzzleEffectPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/Muzzleflash1");
        [HideInInspector]
        public static GameObject tracerPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/tracers/tracercommandodefault");
        [HideInInspector]
        public static GameObject hitPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/HitsparkCommando");

        private float duration;
        private float fireDuration;
        private string muzzleString;
        private bool hasFired;
        private Animator animator;

        public static float range = 55f; 
        public static float minSpread = 6f; 
        public static float maxSpread = 10f;
        public static uint bulletCount = 3;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            duration = baseDuration / attackSpeedStat;
            fireDuration = 0.1f * duration;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";
            hasFired = false;
            PlayCrossfade("Gesture, Override", "Primary", "Primary.playbackRate", duration * 2.5f, 0.05f);
            Shoot();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge < duration || !isAuthority)
                return;
            outer.SetNextStateToMain();
        }

        private void Shoot()
        {
            if (!hasFired)
            {
                hasFired = true;
                bool isCrit = RollCrit();

                string soundString = "ExecutionerPrimary";
                if (isCrit) soundString += "Crit";
                Util.PlaySound(soundString, gameObject);
                AddRecoil(-0.4f * recoil, -0.8f * recoil, -0.3f * recoil, 0.3f * recoil);

                if (muzzleEffectPrefab)
                    EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, muzzleString, false);

                if (isAuthority)
                {
                    float dmg = damageCoefficient * damageStat;
                    Ray aimRay = GetAimRay();


                    var bulletAttack = new BulletAttack
                    {
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = dmg,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.ClayGoo,
                        falloffModel = BulletAttack.FalloffModel.Buckshot,
                        maxDistance = 55f,
                        force = force,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        isCrit = base.RollCrit(),
                        owner = base.gameObject,
                        muzzleName = muzzleString,
                        smartCollision = false,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = procCoefficient,
                        radius = 2f,
                        sniper = false,
                        stopperMask = LayerIndex.CommonMasks.bullet,
                        weapon = null,
                        spreadPitchScale = 0.7f, // prev: 1 0f
                        spreadYawScale = 0.7f, //prev 1 0f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        tracerEffectPrefab = tracerPrefab,
                        hitEffectPrefab = hitPrefab
                    };

                    bulletAttack.minSpread = minSpread;
                    bulletAttack.maxSpread = maxSpread; 
                    bulletAttack.bulletCount = 1;
                    bulletAttack.Fire();

                    uint secondShot = (uint)Mathf.CeilToInt(bulletCount / 2f) - 1;
                    bulletAttack.minSpread = minSpread; 
                    bulletAttack.maxSpread = maxSpread * 1.45f; 
                    bulletAttack.bulletCount = secondShot;
                    bulletAttack.Fire();

                    bulletAttack.minSpread = minSpread * 1.45f;
                    bulletAttack.maxSpread = maxSpread * 2f; 
                    bulletAttack.bulletCount = (uint)Mathf.FloorToInt(bulletCount / 2f);
                    bulletAttack.Fire();

                    bulletAttack.minSpread = minSpread * 2f; 
                    bulletAttack.maxSpread = maxSpread * 4f;
                    bulletAttack.bulletCount = (uint)Mathf.FloorToInt(bulletCount / 2f);
                    bulletAttack.Fire();
                }
                characterBody.AddSpreadBloom(spreadBloom);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
