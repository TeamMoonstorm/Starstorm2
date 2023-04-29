using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using Moonstorm.Starstorm2;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;

namespace EntityStates.MULE
{
    public class MULESuperSlam : BaseSkillState
    {
        public float charge;

        public static float damageCoefficient;
        public static float minRadius;
        public static float maxRadius;
        public static float baseDuration;
        public static string muzzleString;

        public static GameObject slamEffectVFX;

        public static GameObject projectilePrefab;
        public static float waveProjectileArc;
        public static int waveProjectileCount;
        public static float waveProjectileDamageCoefficient;
        public static float waveProjectileForce;

        private Animator animator;
        private float duration;
        private float radius;
        private float fireDuration;
        private bool hasFired;
        private ChildLocator childLocator;
        private ProjectileController pc;
        private ProjectileOverlapAttack poa;

        public override void OnEnter()
        {
            Debug.Log("Super Slamming " + charge);
            base.OnEnter();
            characterBody.SetAimTimer(2f);
            animator = GetModelAnimator();
            childLocator = GetModelChildLocator();
            hasFired = false;
            duration = baseDuration / attackSpeedStat;
            characterBody.SetBuffCount(SS2Content.Buffs.bdHiddenSlow20.buffIndex, 0);
            radius = Util.Remap(charge, 0f, 1f, minRadius, maxRadius);
            fireDuration = 0.25f * duration;

            PlayCrossfade("FullBody, Override", "SlamComedic", "Primary.playbackRate", duration, 0.05f);
            PlayCrossfade("Gesture, Override", "BufferEmpty", "Primary.playbackRate", duration, 0.05f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void GroundSlam()
        {
            bool crit = RollCrit();
            BlastAttack blast = new BlastAttack()
            {
                radius = radius,
                procCoefficient = 1f,
                position = childLocator.FindChild(muzzleString).position,
                attacker = gameObject,
                teamIndex = teamComponent.teamIndex,
                crit = crit,
                baseDamage = damageCoefficient * damageStat,
                damageColorIndex = DamageColorIndex.Default,
                falloffModel = BlastAttack.FalloffModel.None,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                damageType = DamageType.Generic,
                impactEffect = EntityStates.Bison.Headbutt.hitEffectPrefab.GetComponent<EffectIndex>()
                
        };

            blast.Fire();

            EffectManager.SimpleMuzzleFlash(slamEffectVFX, gameObject, muzzleString, true);

            FireShockwaves();
            //AddRecoil();
        }

        private void FireShockwaves()
        {
            pc = projectilePrefab.GetComponent<ProjectileController>();
            poa = projectilePrefab.GetComponent<ProjectileOverlapAttack>();
            GameObject brotherProjectilePrefab = EntityStates.BrotherMonster.WeaponSlam.waveProjectilePrefab;
            ProjectileController brotherPC = brotherProjectilePrefab.GetComponent<ProjectileController>();
            ProjectileOverlapAttack brotherPOA = brotherProjectilePrefab.GetComponent<ProjectileOverlapAttack>();
            pc.flightSoundLoop = brotherPC.flightSoundLoop;
            poa.impactEffect = brotherPOA.impactEffect;

            Transform muzzle = FindModelChild(muzzleString);
            float num = waveProjectileArc / (float)waveProjectileCount;
            Vector3 point = Vector3.ProjectOnPlane(characterDirection.forward, Vector3.up);
            Vector3 position = characterBody.footPosition;
            if (muzzle)
            {
                position = muzzle.position;
            }
            for (int i = 0; i < waveProjectileCount; i++)
            {
                Vector3 forward = Quaternion.AngleAxis(num * ((float)i - (float)waveProjectileCount / 2f), Vector3.up) * point;
                ProjectileManager.instance.FireProjectile(
                    projectilePrefab,
                    position,
                    Util.QuaternionSafeLookRotation(forward),
                    gameObject,
                    damageStat * waveProjectileDamageCoefficient,
                    waveProjectileForce,
                    Util.CheckRoll(characterBody.crit, characterBody.master),
                    DamageColorIndex.Default,
                    null,
                    -1f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterMotor.velocity = Vector3.zero;

            if (fixedAge >= fireDuration && !hasFired)
            {
                hasFired = true;
                GroundSlam();
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
