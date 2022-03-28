/*using EntityStates;
using RoR2;
using UnityEngine;

namespace EntityStates.Nucleator
{
    class FireFissionImpulse : BaseSkillState
    {
        public static float minChargeForceCoef = 1F;
        public static float maxChargeForceCoef = 5F;
        public static float maxOverchargeDistanceCoef = 9F;
        private static float damageCoef = 5.5f;

        public float charge;

        private GameObject resource = Resources.Load<GameObject>("prefabs/effects/impacteffects/Hitspark");
        private float duration;
        private float currentDuration;
        private float speedCoef;
        private Vector3 fissionVector;
        private Vector3 previousPosition;
        private OverlapAttack overlapAttack;

        private readonly float minChargeSpeedCoef = 2f;
        private readonly float maxChargeSpeedCoef = 4f;
        private readonly float maxOverchargeSpeedCoef = 8f;


        public HurtBoxGroup hurtboxGroup;
        public CharacterModel characterModel;


        public FireFissionImpulse(HurtBoxGroup hurtboxGroupIn, CharacterModel characterModelIn, float chargeIn) : base()
        {
            fissionVector = Vector3.zero;
            duration = 0.15f;
            charge = chargeIn;
            hurtboxGroup = hurtboxGroupIn;
            characterModel = characterModelIn;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            CalculateSpeed();

            gameObject.layer = LayerIndex.fakeActor.intVal;
            characterMotor.Motor.RebuildCollidableLayers();
            characterMotor.Motor.ForceUnground();
            characterMotor.velocity = Vector3.zero;

            if (characterModel)
            {
                //characterModel.invisibilityCount++;
            }

            var modelTransform = GetModelTransform();

            if (modelTransform)
            {
                overlapAttack = InitMeleeOverlap(damageCoef, null, modelTransform, "Assaulter");
                overlapAttack.damageType = DamageType.Generic;
                overlapAttack.teamIndex = GetTeam();
                //overlapAttack.pushAwayForce 
            }

            PlayAnimation("FullBody, Override", "UtilityRelease", "Utility.playbackRate", duration);

            Util.PlaySound(EntityStates.Croco.BaseLeap.landingSound.eventName, gameObject);
            EffectManager.SpawnEffect(Resources.Load<GameObject>("CrocoLeapExplosion"), new EffectData
            {
                origin = characterBody.footPosition,
                scale = 10f
            }, true);
        }

        public override void OnExit()
        {
            if (characterModel)
            {
                //characterModel.invisibilityCount--;
            }
            if (hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = hurtboxGroup;
                int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
                hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }
            gameObject.layer = LayerIndex.defaultLayer.intVal;
            characterMotor.Motor.RebuildCollidableLayers();
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            currentDuration += Time.fixedDeltaTime;
            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = Vector3.zero;
                characterMotor.rootMotion += inputBank.aimDirection * (moveSpeedStat * speedCoef * Time.fixedDeltaTime);
                characterBody.isSprinting = true;
            }
            bool flag = overlapAttack.Fire(null);
            if (currentDuration >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private void CalculateSpeed()
        {
            float chargeCoef;
            float speedCoef;
            float overchargeThreshold = NucleatorSkillStateBase.overchargeThreshold;

            if (charge < overchargeThreshold)
            {
                chargeCoef = charge / overchargeThreshold;
                speedCoef = chargeCoef * (maxChargeSpeedCoef - minChargeSpeedCoef) + minChargeSpeedCoef;
                speedCoef = moveSpeedStat * speedCoef;
            }
            else
            {
                chargeCoef = (charge - overchargeThreshold) / (1 - overchargeThreshold);
                speedCoef = chargeCoef * (maxOverchargeSpeedCoef - maxChargeSpeedCoef) + maxChargeSpeedCoef;
                speedCoef = moveSpeedStat * speedCoef;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}*/