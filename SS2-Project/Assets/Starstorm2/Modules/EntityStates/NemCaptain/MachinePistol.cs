using Moonstorm;
using Moonstorm.Starstorm2.Components;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.NemCaptain
{
    public class MachinePistol : BaseSkillState
    {
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float baseDuration;
        public static float minimumDuration;
        public static float force;
        public static float recoil;
        public static float range;
        public static string muzzleString;
        public static string soundString;

        [HideInInspector]
        public static GameObject tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/GoldGat/TracerGoldGat.prefab").WaitForCompletion();

        private float fireTime;
        private bool hasFired;
        private Animator animator;

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
            characterBody.SetAimTimer(2f);
            animator = GetModelAnimator();

            ncc = characterBody.GetComponent<NemCaptainController>();

            //PlayCrossfade("Gesture, Override, LeftArm", "FireGun", "FireGun.playbackRate", baseDuration, 0.005f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        

        private void Fire()
        {
            if (hasFired)
                return;
            hasFired = true;
            bool isCrit = RollCrit();
            //EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
            

            if (soundString != string.Empty)
                Util.PlaySound(soundString, gameObject);
            
            if (isAuthority)
            {
                Ray aimRay = GetAimRay();
                AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);

                BulletAttack bulletAttack = new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    damage = damageCoefficient * damageStat,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    maxDistance = range,
                    force = force,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = 0.2f * ncc.stressFraction,
                    maxSpread = characterBody.spreadBloomAngle + ncc.stressFraction,
                    isCrit = isCrit,
                    owner = gameObject,
                    muzzleName = muzzleString,
                    smartCollision = true,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    radius = 0.75f,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = tracerEffectPrefab,
                    spreadPitchScale = 2f * ncc.stressFraction,
                    spreadYawScale = 2f * ncc.stressFraction,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = Commando.CommandoWeapon.FirePistol2.hitEffectPrefab
                };

                bulletAttack.Fire();

                //FindModelChild("casingParticle").GetComponent<ParticleSystem>().Emit(1);

                characterBody.AddSpreadBloom(0.2f + (0.3f * ncc.stressFraction));
            }
        }

       
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireTime)
            {
                Fire();
            }

            if (fixedAge >= duration && isAuthority)
            {
                if (inputBank.skill1.down & skillLocator.primary.stock >= 1)
                {
                    outer.SetNextState(new MachinePistol());
                    skillLocator.primary.stock -= 1;
                    return;
                }
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (fixedAge <= minimumDuration)
            {
                return InterruptPriority.PrioritySkill;
            }

            return InterruptPriority.Any;
        }
    }
}
