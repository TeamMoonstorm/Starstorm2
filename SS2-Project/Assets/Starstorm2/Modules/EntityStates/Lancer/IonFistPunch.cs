using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Lancer
{
    public class IonFistPunch : BaseState, SteppedSkillDef.IStepSetter
    {
        public enum Gauntlet
        {
            Left,
            Right
        }

        public static GameObject projectilePrefab;
        public static GameObject muzzleflashEffectPrefab;
        public static float baseDuration = 0.4f;
        public static float damageCoefficient = 1.5f;
        public static float force = 20f;
        public static float procCoefficient = 1f;
        public static string attackSoundString = "";

        private float duration;
        private string muzzleString;
        private Gauntlet gauntlet;

        public void SetStep(int i)
        {
            gauntlet = (Gauntlet)i;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            StartAimMode(2f);

            Util.PlaySound(attackSoundString, gameObject);

            switch (gauntlet)
            {
                case Gauntlet.Left:
                    muzzleString = "FistLeft";
                    PlayCrossfade("Gesture, Override", "PunchLeft", "Punch.playbackRate", duration, 0.1f);
                    break;
                case Gauntlet.Right:
                    muzzleString = "FistRight";
                    PlayCrossfade("Gesture, Override", "PunchRight", "Punch.playbackRate", duration, 0.1f);
                    break;
            }

            if (isAuthority && projectilePrefab)
            {
                Ray aimRay = GetAimRay();
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = projectilePrefab,
                    position = FindModelChild(muzzleString)?.position ?? aimRay.origin,
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    owner = gameObject,
                    damage = damageStat * damageCoefficient,
                    force = force,
                    crit = RollCrit(),
                    damageTypeOverride = new DamageTypeCombo(DamageType.Generic, DamageTypeExtended.Generic, DamageSource.Primary)
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }

            if (muzzleflashEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, gameObject, muzzleString, transmit: false);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write((byte)gauntlet);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            gauntlet = (Gauntlet)reader.ReadByte();
        }
    }
}
