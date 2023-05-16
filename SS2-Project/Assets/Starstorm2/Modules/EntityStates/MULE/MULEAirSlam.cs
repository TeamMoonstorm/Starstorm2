using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using Moonstorm.Starstorm2;
using UnityEngine.AddressableAssets;
using RoR2.Projectile;

namespace EntityStates.MULE
{
    public class MULEAirSlam : BaseSkillState
    {
        public float charge;

        public static float minDamageCoefficient;
        public static float maxDamageCoefficient;
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
        private float damageCoefficient;
        private ChildLocator childLocator;
        private ProjectileController pc;
        private ProjectileOverlapAttack poa;
        private float ogmass;

        public override void OnEnter()
        {
            Debug.Log("Air Slamming " + charge);
            base.OnEnter();
            characterBody.SetAimTimer(2f);
            animator = GetModelAnimator();
            childLocator = GetModelChildLocator();
            hasFired = false;
            duration = baseDuration / attackSpeedStat;
            characterBody.SetBuffCount(SS2Content.Buffs.bdHiddenSlow20.buffIndex, 0);
            damageCoefficient = Util.Remap(charge, 0f, 1f, minDamageCoefficient, maxDamageCoefficient);
            radius = Util.Remap(charge, 0f, 1f, minRadius, maxRadius);
            fireDuration = 0.25f * duration;

            if (!isGrounded)
            {
                ogmass = characterMotor.mass;
                characterMotor.mass *= 30f; //lol fatass
                PlayCrossfade("FullBody, Override", "SlamAir", "Primary.playbackRate", duration, 0.05f);
                PlayCrossfade("Gesture, Override", "BufferEmpty", "Primary.playbackRate", duration, 0.05f);

                characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                characterMotor.onHitGroundAuthority += GroundSlam;
            }
        }

        public override void OnExit()
        {

            base.OnExit();
            //GroundSlam();
            PlayCrossfade("FullBody, Override", "SlamLand", "Primary.playbackRate", duration, 0.05f);
            characterMotor.onHitGroundAuthority -= GroundSlam;
            characterBody.bodyFlags -= CharacterBody.BodyFlags.IgnoreFallDamage;
            characterMotor.mass = ogmass;
        }

        private void GroundSlam(ref CharacterMotor.HitGroundInfo hitGroundInfo)
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
                baseDamage = damageCoefficient * damageStat + 2f,
                damageColorIndex = DamageColorIndex.Default,
                falloffModel = BlastAttack.FalloffModel.None,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                damageType = DamageType.Generic,
                impactEffect = Bison.Headbutt.hitEffectPrefab.GetComponent<EffectIndex>()
            };

            EffectManager.SimpleMuzzleFlash(slamEffectVFX, gameObject, muzzleString, true);

            blast.Fire();

            if (charge >= 1f)
                FireShockwaves();

            outer.SetNextStateToMain();
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
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
