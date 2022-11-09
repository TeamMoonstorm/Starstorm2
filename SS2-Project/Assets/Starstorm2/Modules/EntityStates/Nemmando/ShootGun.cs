using Moonstorm;
using RoR2;
using UnityEngine;

//Dont fix what isnt broken or something
namespace EntityStates.Nemmando
{
    public class ShootGun : RendHandler
    {
        [TokenModifier("SS2_NEMMANDO_SECONDARY_SHOOT_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float baseDuration;
        public static float force;
        public static float recoil;
        public static float range;
        public static string muzzleString;
        public static string soundString;
        
        
        [HideInInspector]
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");
        

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
            fireTime = 0.1f * this.duration;
            characterBody.SetAimTimer(2f);
            animator = GetModelAnimator();
            muzzleString = "Muzzle";

            FindModelChildGameObject("GunSpinEffect").SetActive(false);
            PlayCrossfade("RightArm, Override", "ShootGunShort", "ShootGun.playbackRate", this.duration, 0.05f);
        }

        public override void OnExit()
        {
            base.OnExit();

            float rechargeTime = Mathf.Clamp(this.skillLocator.secondary.finalRechargeInterval, 0.25f, Mathf.Infinity);
            PlayAnimation("Gesture, Override", "ReloadGun", "Reload.playbackRate", 0.5f * (rechargeTime - this.duration - 0.3f));
        }

        

        private void Fire()
        {
            if (hasFired)
                return;
            hasFired = true;
            bool isCrit = RollCrit();
            EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);

            PlayCrossfade("Gesture, Additive", "FireGun", "FireGun.playbackRate", duration, 0.075f);

            //TODO: Add other sounds per skin
            //if (isCrit) soundString += "Crit";
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
                    minSpread = 0f,
                    maxSpread = 0f,
                    isCrit = isCrit,
                    owner = gameObject,
                    muzzleName = muzzleString,
                    smartCollision = false,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    radius = 0.75f,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = tracerEffectPrefab,
                    spreadPitchScale = 0f,
                    spreadYawScale = 0f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab
                };

                RendMultiplier(bulletAttack);

                bulletAttack.Fire();

                characterBody.AddSpreadBloom(1.5f);
            }
        }

       
        public override void FixedUpdate()
        {
            
            base.FixedUpdate();

            if (fixedAge >= this.fireTime)
            {
                this.Fire();
            }

            if (fixedAge >= this.duration && isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
