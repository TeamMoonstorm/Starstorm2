using EntityStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;

namespace EntityStates.LampBoss
{
    public class BuffNearby : BaseState
    {
        public static float baseDuration;
        private float duration;

        public static GameObject buffWard;
        public static GameObject blueBuffWard;
        private GameObject wardInstance;

        public static GameObject chargingVFX;
        public static GameObject chargingVFXBlue;

        public static GameObject explosionVFX;
        public static GameObject explosionBlueVFX;
        public static float baseDamageCoefficient = 2f;
        public static float forceCoefficient = 400f;
        public static float procCoefficient = 1.0f;
        public static float radius = 10f;

        private bool hasBuffed;
        private Animator animator;
        public static string mecanimParameter;
        private float timer;

        private bool isBlue;

        public override void OnEnter()
        {
            base.OnEnter();
            hasBuffed = false;
            animator = GetModelAnimator();
            duration = baseDuration / attackSpeedStat;

            hasBuffed = false;

            isBlue = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken == "SS2_SKIN_LAMP_BLUE";

            GameObject vfx = isBlue ? chargingVFXBlue : chargingVFX;

            if (vfx)
            {
                EffectData effectData = new EffectData
                {
                    origin = characterBody.corePosition,
                    rootObject = GetModelChildLocator().FindChild("Chest").gameObject
                };
                //EffectManager.SpawnEffect(vfx, effectData, true);
            }

            PlayCrossfade("FullBody, Override", "SecondaryBuff", "Secondary.playbackRate", duration, 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (animator.GetFloat(mecanimParameter) >= 0.5f && !hasBuffed)
            {
                hasBuffed = true;
                
                GameObject ward = isBlue ? blueBuffWard : buffWard;
                GameObject vfx = isBlue ? explosionBlueVFX : explosionVFX;

                if (isAuthority)
                {
                    wardInstance = Object.Instantiate(ward);
                    wardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                    wardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject);
                }

                bool crit = RollCrit();
                BlastAttack blast = new BlastAttack()
                {
                    radius = radius,
                    procCoefficient = procCoefficient,
                    position = characterBody.corePosition,
                    attacker = gameObject,
                    teamIndex = teamComponent.teamIndex,
                    baseForce = forceCoefficient,
                    crit = crit,
                    baseDamage = characterBody.damage * baseDamageCoefficient,
                    damageColorIndex = DamageColorIndex.Default,
                    falloffModel = BlastAttack.FalloffModel.None,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                };
                blast.Fire();

                if (vfx)
                    EffectManager.SimpleEffect(vfx, characterBody.corePosition, Quaternion.identity, true);

                Util.PlayAttackSpeedSound("LampBullet", gameObject, characterBody.attackSpeed);

                //Util.PlaySound();
            }

            if (fixedAge >= duration)
                outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
