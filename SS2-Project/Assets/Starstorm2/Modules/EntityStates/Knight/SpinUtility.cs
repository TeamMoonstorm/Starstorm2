using MSU;
using MSU.Config;
using RoR2;
using RoR2.Skills;
using SS2;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace EntityStates.Knight
{
    public class SpinUtility : BaseKnightDashMelee
    {
        public static float swingTimeCoefficient = 1.63f;
        [FormatToken("SS2_KNIGHT_SPECIAL_SPIN_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float TokenModifier_dmgCoefficient => new SpinUtility().damageCoefficient;
        private Transform swordPivot;                       //^ew

        // Multihit info
        [SerializeField]
        //public for hot compile
        public float baseFireFrequency = 2f;
        public float fireFrequency;
        public float fireAge;
        public float fireInterval;
        public float intervalUntilPlaySpin;
        public float intervalUntilPlaySpinFinisher;
        public Transform modelTransform;
        #region test
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testbaseDuration = 0.69f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testminSpeedCoefficient = 2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testmaxSpeedCoefficient = 4f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testinterruptSpeedCoefficient = 0.4f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testFireFrequency = 5f;
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testFireFrequencyBoosted = 10f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testDamage = 2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testDamageBoosted = 4f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testEffectSpin = 0.115f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testEffectSpinFinisher = 0.456f;
        #endregion test

        enum SpinEffectState
        {
            Null,
            Start,
            Spin,
            Finisher
        }

        private SpinEffectState currentEffectState;
        private float effectTimer;

        public override void OnEnter()
        {
            #region test
            baseDuration = testbaseDuration;
            minSpeedCoefficient = testminSpeedCoefficient;
            maxSpeedCoefficient = testmaxSpeedCoefficient;
            interruptSpeedCoefficient = testinterruptSpeedCoefficient;
            baseFireFrequency = testFireFrequency;
            damageCoefficient = testDamage;

            intervalUntilPlaySpin = testEffectSpin;
            intervalUntilPlaySpinFinisher = testEffectSpinFinisher;
            #endregion test

            base.OnEnter();
            this.attack.damageType.damageSource = DamageSource.Utility;
            fireFrequency = baseFireFrequency * attackSpeedStat;
            fireInterval = 1 / fireFrequency;

            modelTransform = GetModelTransform();

            swordPivot = FindModelChild("HitboxAnchor");
            swordPivot.rotation = Util.QuaternionSafeLookRotation(GetAimRay().direction);

            //currentEffectState = SpinEffectState.Null;
        }

        /*public override void Update()
        {
            base.Update();

            if(!(currentEffectState == SpinEffectState.Null || currentEffectState == SpinEffectState.Finisher))
            {
                effectTimer -= Time.deltaTime;
                if(effectTimer <= 0.0)
                {
                    switch (currentEffectState)
                    {
                        case SpinEffectState.Start: //transition to spin
                            currentEffectState = SpinEffectState.Spin;
                            swingEffectPrefab = SS2.Survivors.Knight.KnightSpinEffect;
                            effectTimer = intervalUntilPlaySpinFinisher * duration;
                            PlaySwingEffect();
                            break;
                        case SpinEffectState.Spin: //transition to finisher
                            currentEffectState = SpinEffectState.Finisher;
                            swingEffectPrefab = SS2.Survivors.Knight.KnightSpinFinisherEffect;
                            PlaySwingEffect();
                            break;
                    }
                }
            }
        }*/

        protected override void ModifyMelee()
        {
            swingEffectPrefab = SS2.Survivors.Knight.KnightSpinEffect;
            hitEffectPrefab = SS2.Survivors.Knight.KnightHitEffect;
        }

        protected override void FireAttack()
        {
            if (!inHitPause)
            {
                fireAge += Time.fixedDeltaTime;
            }

            List<HurtBox> hurtBoxes = new List<HurtBox>();


            while ((fireAge >= fireInterval))
            {
                fireAge -= fireInterval;
                attack.ResetIgnoredHealthComponents();
                hurtBoxes.Clear();
                PlaySwingEffect();

                if (isAuthority && attack.Fire(hurtBoxes))
                {
                    for (int i = 0; i < hurtBoxes.Count; i++)
                    {
                        PushVictim(hurtBoxes[i]);
                    }
                    OnHitEnemyAuthority();
                }
            }
        }

        //public for hot compile
        public void PushVictim(HurtBox hurtBox)
        {
            if (hurtBox.healthComponent.body == null)
                return;

            float sizeMultiplier = 1;
            switch (hurtBox.healthComponent.body.hullClassification)
            {
                default:
                case HullClassification.Human:
                    break;
                case HullClassification.Golem:
                    sizeMultiplier = 0.5f;
                    break;
                case HullClassification.BeetleQueen:
                    sizeMultiplier = 0.2f;
                    break;
            }

            Vector3 hurtboxPos = hurtBox.healthComponent.transform.position;
            float z = modelTransform.InverseTransformPoint(hurtboxPos).z;

            //0 to 1 based on how far the enemy is in front of or behind, with 0 being furthest in front
            float positionMultiplier = Mathf.InverseLerp(8, -8, z);
            PhysForceInfo physInfo = new PhysForceInfo()
            {
                massIsOne = true,
                disableAirControlUntilCollision = true,
                ignoreGroundStick = true,                               //presto!
                force = forwardDirection * ((rollSpeed * fireInterval * 6f) + (positionMultiplier * 3)) * sizeMultiplier,
            };

            if (hurtBox.healthComponent.body.characterMotor)
            {
                hurtBox.healthComponent.body.characterMotor.velocity = Vector3.zero;
                hurtBox.healthComponent.body.characterMotor.ApplyForceImpulse(physInfo);
                //should probably just set velocity instead of setting to zero and applying impulse, but the disableaircontrol and ignoregroundstick are useful
            }
            if(hurtBox.healthComponent.TryGetComponent(out RigidbodyMotor motor))
            {
                physInfo.disableAirControlUntilCollision = false;
                motor.rigid.velocity = Vector3.zero;
                motor.ApplyForceImpulse(physInfo);
            }
        }

        public override void PlayAttackAnimation()
        {
            PlayCrossfade("FullBody, Override", "Utility", "Utility.playbackRate", duration * swingTimeCoefficient, duration * 0.15f);   
        }

        public override void OnExit()
        {
            base.OnExit();

            swordPivot.transform.localRotation = Quaternion.identity;
        }
    }
}