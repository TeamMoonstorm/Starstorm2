using Moonstorm;
using Moonstorm.Starstorm2;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.NemCommando
{
    public class ShootGun2 : BaseSkillState
    {
        [TokenModifier("SS2_NEMMANDO_SECONDARY_SHOOT_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float baseDuration;
        public static float minimumDuration;
        public static float force;
        public static float recoil;
        public static float range;
        public static string muzzleString;
        public static string soundString;
        private string skinNameToken;


        [HideInInspector]
        public static GameObject tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/GoldGat/TracerGoldGat.prefab").WaitForCompletion();
        

        private float fireTime;
        private bool hasFired;
        private Animator animator;
        
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

            if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_GM")
            {
                muzzleString = "Suppressor";
            }
            else
            {
                muzzleString = "Muzzle";
            }

            animator.SetBool("shouldAdditiveReload", false);

            if (animator.GetFloat("primaryPlaying") > 0.05)
            {
                PlayCrossfade("Gesture, Override, LowerLeftArm", "FireGun", "FireGun.playbackRate", baseDuration, 0.005f);
                animator.SetBool("shouldAdditiveReload", true);
            }

            PlayCrossfade("Gesture, Override, LeftArm", "FireGun", "FireGun.playbackRate", baseDuration, 0.005f);
        }

        public override void OnExit()
        {
            base.OnExit();
            //SS2Log.Info("Ahhhh");
            //if(skillLocator.secondary.stock != skillLocator.secondary.maxStock)
            //{
            //    ReloadGun nextState = new ReloadGun();
            //    outer.SetNextState(nextState);
            //}
            //ReloadGun nextState = new ReloadGun();
            //outer.SetNextState(nextState);
            //float rechargeTime = Mathf.Clamp(skillLocator.secondary.finalRechargeInterval, 0.25f, Mathf.Infinity);
        }

        

        private void Fire()
        {
            if (hasFired)
                return;
            hasFired = true;
            bool isCrit = RollCrit();
            EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
            

            if (soundString != string.Empty)
                Util.PlaySound(soundString, gameObject);
            
            if (isAuthority)
            {
                Ray aimRay = GetAimRay();
                AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);

                float minSpread = 0f;
                float maxSpread = characterBody.spreadBloomAngle;
                if (teamComponent.teamIndex != TeamIndex.Player)
                {
                    minSpread += 2f;
                    maxSpread += 4f;
                }

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
                    minSpread = minSpread,
                    maxSpread = maxSpread,
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
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = Commando.CommandoWeapon.FirePistol2.hitEffectPrefab
                };

                bulletAttack.Fire();

                FindModelChild("casingParticle").GetComponent<ParticleSystem>().Emit(1);

                characterBody.AddSpreadBloom(3f);
            }
        }

       
        public override void FixedUpdate()
        {
            
            base.FixedUpdate();

            //SS2Log.Info("Aaahh");

            if (fixedAge >= fireTime * duration)
            {
                Fire();
            }

            if (fixedAge >= duration && isAuthority)
            {
                if (inputBank.skill2.down & skillLocator.secondary.stock >= 1)
                {
                    outer.SetNextState(new ShootGun2());
                    skillLocator.secondary.stock -= 1;
                    return;
                }
                if(fixedAge >= 1.5f * duration)
                {
                    outer.SetNextState(new ReloadGun());
                    return;
                }
                //outer.SetNextStateToMain();
                //return;
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
