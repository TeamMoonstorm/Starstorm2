using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;

namespace EntityStates.Cyborg2
{
    public class DoubleCannon : BaseSkillState
    {
        public static float baseDuration = 1.33f;
        public static float bulletMaxDistance = 256;
        public static float bulletRadius = 1;
        public static float secondShotTime = 0.125f;

        [NonSerialized]
        private static GameObject TRACERTEMP = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/TracerHuntressSnipe.prefab").WaitForCompletion();

        [SerializeField]
        public float damageCoefficient = 2f;
        [SerializeField]
        public float procCoefficient = 1f;
        [SerializeField]
        public float force = 150f;
        [SerializeField]
        public GameObject tracerPrefab;
        [SerializeField]
        public GameObject hitEffectPrefab;
        [SerializeField]
        public GameObject muzzleFlashPrefab;
        [SerializeField]
        public string fireSoundString = "Play_lunar_golem_attack1_launch"; //"Play_MULT_m1_snipe_shoot"
        [SerializeField]
        public float recoil = 1;
        [SerializeField]
        public float bloom = .4f;
        [SerializeField]
        public float fireSoundPitch = 1;

        private float duration;
        private bool hasFiredSecondShot;
        public override void OnEnter()
        {
            base.OnEnter();

            this.tracerPrefab = TRACERTEMP;

            this.duration = DoubleCannon.baseDuration / base.attackSpeedStat;
            base.StartAimMode();
            Fire("CannonR");
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.fixedAge >= this.duration * secondShotTime && !this.hasFiredSecondShot)
            {
                this.hasFiredSecondShot = true;
                Fire("CannonL");
            }

            if(base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if(!this.hasFiredSecondShot)
            {
                Fire("CannonL");
            }
        }
        private void Fire(string muzzleName)
        {           
            Util.PlayAttackSpeedSound(this.fireSoundString, base.gameObject, this.fireSoundPitch);
            EffectManager.SimpleMuzzleFlash(this.muzzleFlashPrefab, base.gameObject, muzzleName, false);
            //anim
            base.characterBody.AddSpreadBloom(this.bloom);
            base.AddRecoil(-1f * this.recoil, -1.5f * this.recoil, -1f * this.recoil, 1f * this.recoil);

            Ray aimRay = base.GetAimRay();
            float spreadAngle = 0f;
            new BulletAttack
            {
                aimVector = aimRay.direction,
                origin = aimRay.origin,
                owner = base.gameObject,
                damage = this.damageStat * this.damageCoefficient,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Generic,
                falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                force = this.force,
                HitEffectNormal = false,
                procChainMask = default(ProcChainMask),
                procCoefficient = this.procCoefficient,
                maxDistance = DoubleCannon.bulletMaxDistance,
                radius = DoubleCannon.bulletRadius,
                isCrit = base.RollCrit(),
                muzzleName = muzzleName,
                minSpread = 0,
                maxSpread = 0,
                hitEffectPrefab = this.hitEffectPrefab,
                smartCollision = true,
                tracerEffectPrefab = this.tracerPrefab
            }.Fire();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
