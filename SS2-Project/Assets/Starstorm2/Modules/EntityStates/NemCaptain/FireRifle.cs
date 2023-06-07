using Moonstorm.Starstorm2.Components;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.NemCaptain
{
    public class FireRifle : BaseSkillState
    {
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float baseDuration;
        public static float force;
        public static float recoil;
        public static float range;
        public static string soundString;
        public static string muzzleString;

        [HideInInspector]
        public static GameObject tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgun.prefab").WaitForCompletion();

        private float fireTime;
        private bool hasFired;

        private NemCaptainController ncc;

        private float duration
        {
            get
            {
                return baseDuration / attackSpeedStat;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.outOfCombatStopwatch = 0f;
            fireTime = 0.1f * duration;

            hasFired = false;

            ncc = characterBody.GetComponent<NemCaptainController>();

            //play animation
        }

        private void Fire()
        {
            if (hasFired)
                return;
            hasFired = true;
            bool isCrit = RollCrit();
            //muzzleflash

            if (soundString != string.Empty)
                Util.PlaySound(soundString, gameObject);

            if (isAuthority)
            {
                Ray aimRay = GetAimRay();
                AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);

                float minSpread = 0f + (4f * ncc.stressFraction);
                float maxSpread = characterBody.spreadBloomAngle + (8f * ncc.stressFraction);
                if (teamComponent.teamIndex != TeamIndex.Player)
                {
                    minSpread += 2f * (2f * ncc.stressFraction);
                    maxSpread += 2f + (4f * ncc.stressFraction);
                }

                BulletAttack bulletAttack = new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    damage = damageCoefficient * damageStat,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Stun1s,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    maxDistance = range,
                    force = force,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = minSpread,
                    maxSpread = maxSpread,
                    isCrit = isCrit,
                    owner = gameObject,
                    //muzzleName = muzzleString,
                    smartCollision = true,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    radius = 1.5f,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = tracerEffectPrefab,
                    spreadPitchScale = 1f + (4f * ncc.stressFraction),
                    spreadYawScale = 1f + (4f * ncc.stressFraction),
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = Commando.CommandoWeapon.FirePistol2.hitEffectPrefab
                };

                bulletAttack.Fire();

                //FindModelChild("casingParticle").GetComponent<ParticleSystem>().Emit(1);

                characterBody.AddSpreadBloom(3f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireTime * duration && !hasFired)
                Fire();

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
