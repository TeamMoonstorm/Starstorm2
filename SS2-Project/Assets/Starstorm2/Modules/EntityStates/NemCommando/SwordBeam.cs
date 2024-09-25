﻿using SS2;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using MSU;

namespace EntityStates.NemCommando
{
    public class SwordBeam : BaseSkillState
    {
        public float charge;

        public static float maxEmission;
        public static float minEmission;
        [FormatToken("SS2_NEMMANDO_SECONDARY_CONCUSSION_DESCRIPTION", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float maxRecoil;
        public static float minRecoil;
        public static float projectileSpeed;
        public static float baseDuration;
        public static GameObject projectilePrefab;

        private float recoil;
        private float duration;
        private float fireDuration;
        private bool hasFired;
        private string muzzleString;

        private string skinNameToken;
        private GameObject muzzleFlash;

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.SetAimTimer(2f);
            hasFired = false;
            duration = baseDuration / attackSpeedStat;
            recoil = Util.Remap(charge, 0f, 1f, minRecoil, maxRecoil);
            fireDuration = 0.1f * duration;

            PlayCrossfade("Gesture, Override", "SecondaryBeam", "Secondary.playbackRate", duration, 0.05f);

            Util.PlaySound("NemmandoFireBeam2", gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();

        }

        public virtual void FireBeam()
        {
            if (!hasFired)
            {
                hasFired = true;

                skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                //red
                muzzleFlash = SS2Assets.LoadAsset<GameObject>("MuzzleflashNemCommandoRed", SS2Bundle.NemCommando);
                projectilePrefab = SS2Assets.LoadAsset<GameObject>("NemCommandoSwordBeamProjectile", SS2Bundle.NemCommando);


                if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_COMMANDO" || skinNameToken.Contains("BLUE"))
                {
                    muzzleFlash = SS2Assets.LoadAsset<GameObject>("MuzzleflashNemCommandoBlue", SS2Bundle.NemCommando);
                    projectilePrefab = SS2Assets.LoadAsset<GameObject>("NemCommandoSwordBeamProjectileBlue", SS2Bundle.NemCommando);
                }
                //Yellow
                if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_MASTERY" || skinNameToken.Contains("YELLOW"))
                {
                    muzzleFlash = SS2Assets.LoadAsset<GameObject>("MuzzleflashNemCommandoYellow", SS2Bundle.NemCommando);
                    projectilePrefab = SS2Assets.LoadAsset<GameObject>("NemCommandoSwordBeamProjectileYellow", SS2Bundle.NemCommando);
                }


                EffectManager.SimpleMuzzleFlash(muzzleFlash, gameObject, muzzleString, false);

                if (isAuthority)
                {
                    float damage = damageCoefficient * damageStat;
                    AddRecoil(-2f * recoil, -3f * recoil, -1f * recoil, 1f * recoil);
                    characterBody.AddSpreadBloom(0.33f * recoil);
                    Ray aimRay = GetAimRay();

                    ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), gameObject, damage, 0f, RollCrit(), DamageColorIndex.Default, null, projectileSpeed);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireDuration)
            {
                FireBeam();
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}