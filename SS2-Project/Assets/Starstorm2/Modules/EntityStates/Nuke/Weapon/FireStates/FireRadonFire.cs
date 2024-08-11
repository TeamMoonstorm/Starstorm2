using R2API;
using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.Nuke.Weapon
{
    public class FireRadonFire : BaseNukeFireState
    {
        public static float duration;
        public static SerializableDamageColor damageColor;
        public static string rightMuzzleName;
        public static string leftMuzzleName;
        public static float baseRadius;
        public static float baseDistance;
        public static float procCoefficientPerTick;
        public static float tickFrequency;
        public static float ignitePercentChange;

        private float _tickDamageCoefficient;
        private float _flamethrowerStopwatch;
        private float _duration;
        private bool _isCrit;
        private Transform _leftMuzzleTransform;
        private Transform _rightMuzzleTransform;
        private BulletAttack _leftFlamethrowerAttack;
        private BulletAttack _rightFlamethrowerAtack;
        public override void OnEnter()
        {
            base.OnEnter();
            _duration = duration;
            var childLocator = GetModelChildLocator();
            if (childLocator)
            {
                _leftMuzzleTransform = childLocator.FindChild(leftMuzzleName);
                _rightMuzzleTransform = childLocator.FindChild(rightMuzzleName);
            }
            int num = Mathf.CeilToInt(_duration * tickFrequency);
            _tickDamageCoefficient = charge / num;
            if (isAuthority)
            {
                _isCrit = RollCrit();
            }

            _leftFlamethrowerAttack = new BulletAttack
            {
                radius = baseRadius * charge,
                maxDistance = baseDistance * charge,
                bulletCount = 1,
                damage = damageStat * _tickDamageCoefficient,
                damageColorIndex = damageColor.DamageColorIndex,
                falloffModel = BulletAttack.FalloffModel.Buckshot,
                isCrit = _isCrit,
                muzzleName = leftMuzzleName,
                owner = gameObject,
                procCoefficient = procCoefficientPerTick,
                weapon = gameObject,
                stopperMask = LayerIndex.world.mask,
                smartCollision = true,
            };
            _leftFlamethrowerAttack.AddModdedDamageType(SS2.Survivors.Nuke.NuclearDamageType);

            _rightFlamethrowerAtack = new BulletAttack
            {
                radius = baseRadius * charge,
                maxDistance = baseDistance * charge,
                bulletCount = 1,
                damage = damageStat * _tickDamageCoefficient,
                damageColorIndex = damageColor.DamageColorIndex,
                falloffModel = BulletAttack.FalloffModel.Buckshot,
                isCrit = _isCrit,
                muzzleName = rightMuzzleName,
                owner = gameObject,
                procCoefficient = procCoefficientPerTick,
                weapon = gameObject,
                stopperMask = LayerIndex.world.mask,
                smartCollision = true,
            };
            _rightFlamethrowerAtack.AddModdedDamageType(SS2.Survivors.Nuke.NuclearDamageType);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            _flamethrowerStopwatch += Time.fixedDeltaTime;
            float num = 1 / tickFrequency / attackSpeedStat;
            if (_flamethrowerStopwatch > num)
            {
                _flamethrowerStopwatch -= num;
                FireGauntlet();
            }

            if (fixedAge > duration && isAuthority)
                outer.SetNextStateToMain();
        }

        private void FireGauntlet()
        {
            if (!isAuthority)
                return;

            Ray aimRay = GetAimRay();
            _leftFlamethrowerAttack.origin = aimRay.origin;
            _leftFlamethrowerAttack.aimVector = aimRay.direction;
            _leftFlamethrowerAttack.damageType = (Util.CheckRoll(ignitePercentChange, characterBody.master) ? DamageType.IgniteOnHit : DamageType.Generic);

            _rightFlamethrowerAtack.origin = aimRay.origin;
            _rightFlamethrowerAtack.aimVector = aimRay.direction;
            _rightFlamethrowerAtack.damageType = (Util.CheckRoll(ignitePercentChange, characterBody.master) ? DamageType.IgniteOnHit : DamageType.Generic);

            _leftFlamethrowerAttack.Fire();
            _rightFlamethrowerAtack.Fire();
        }
    }
}