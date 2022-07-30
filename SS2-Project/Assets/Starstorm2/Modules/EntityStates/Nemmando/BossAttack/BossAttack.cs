﻿using Moonstorm;
using Moonstorm.Starstorm2.DamageTypes;
using R2API;
using RoR2;
using UnityEngine;

namespace EntityStates.Nemmando
{
    public class BossAttack : BaseSkillState
    {
        public float charge;

        public static float baseDuration;
        [TokenModifier("SS2_NEMMANDO_SPECIAL_BOSS_DESCRIPTION", StatTypes.Default, 0)]
        public static int maxHits;
        public static int minHits;
        [TokenModifier("SS2_NEMMANDO_SPECIAL_BOSS_DESCRIPTION", StatTypes.Percentage, 1)]
        public static float maxDamageCoefficient;
        public static float minDamageCoefficient;
        public static float maxRadius;
        public static float minRadius;
        public static float maxEmission;
        public static float minEmission;
        public static GameObject effectPrefab;

        private float hitStopwatch;
        private float duration;
        private int hitCount;
        private float damageCoefficient;
        private float radius;
        private float emission;
        private BlastAttack blastAttack;
        private EffectData attackEffect;
        private Material swordMat;
        //private NemmandoController nemmandoController;
        private float minimumEmission;

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.isSprinting = false;
            duration = baseDuration / attackSpeedStat;
            hitCount = Mathf.RoundToInt(Util.Remap(charge, 0f, 1f, minHits, maxHits));
            damageCoefficient = Util.Remap(charge, 0f, 1f, minDamageCoefficient, maxDamageCoefficient);
            radius = Util.Remap(charge, 0f, 1f, minRadius, maxRadius);
            emission = Util.Remap(charge, 1f, 2f, minEmission, maxEmission);
            //nemmandoController = GetComponent<NemmandoController>();
            characterBody.hideCrosshair = false;

            //minimumEmission = effectComponent.defaultSwordEmission;

            blastAttack = new BlastAttack()
            {
                attacker = gameObject,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                baseDamage = damageCoefficient * damageStat,
                baseForce = -500f,
                bonusForce = Vector3.zero,
                crit = RollCrit(),
                damageColorIndex = DamageColorIndex.Default,
                damageType = RoR2.DamageType.Generic,
                falloffModel = BlastAttack.FalloffModel.None,
                inflictor = gameObject,
                losType = BlastAttack.LoSType.None,
                position = characterBody.corePosition,
                procChainMask = default,
                procCoefficient = 1f,
                radius = radius,
                teamIndex = GetTeam()
            };
            DamageAPI.AddModdedDamageType(blastAttack, Gouge.gougeDamageType);

            characterMotor.rootMotion = Vector3.zero;
            characterMotor.velocity = Vector3.zero;

            if (charge >= 0.4f)
            {

                EffectData data = new EffectData();
                data.scale = characterBody.bestFitRadius * 10;
                data.origin = characterBody.corePosition;
                EffectManager.SpawnEffect(effectPrefab, data, true);

            }
            FireAttack();

            if (charge >= 0.6f)
            {
                PlayAnimation("FullBody, Override", "DecisiveStrikeMax", "DecisiveStrike.playbackRate", duration);
                Util.PlaySound("NemmandoDecisiveStrikeFire", gameObject);
            }
            else
            {
                PlayAnimation("FullBody, Override", "DecisiveStrike", "DecisiveStrike.playbackRate", duration);
                //Util.PlaySound(effectComponent.swingSound, gameObject);
            }

            swordMat = GetModelTransform().GetComponent<CharacterModel>().baseRendererInfos[1].defaultMaterial;
        }

        private void FireAttack()
        {
            hitStopwatch = duration / hitCount;

            if (isAuthority)
            {
                blastAttack.position = characterBody.corePosition;

                int hitcount = blastAttack.Fire().hitCount;
                if (hitCount > 0) Util.PlaySound(EntityStates.Merc.GroundLight.hitSoundString, gameObject);

                EffectData data = new EffectData();
                data.scale = characterBody.bestFitRadius * 10;
                data.origin = characterBody.corePosition;
                EffectManager.SpawnEffect(effectPrefab, data, true);

                //Util.PlayAttackSpeedSound(effectComponent.swingSound, gameObject, 2f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            hitStopwatch -= Time.fixedDeltaTime;
            emission -= 2f * Time.fixedDeltaTime;
            if (emission < 0f) emission = 0f;

            if (swordMat)
                swordMat.SetFloat("_EmPower", Util.Remap(fixedAge, 0, duration, emission, minimumEmission));

            characterMotor.rootMotion = Vector3.zero;
            characterMotor.velocity = Vector3.zero;

            for (int i = 0; hitStopwatch <= 0f && i < hitCount; i++)
            {
                FireAttack();
            }

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (cameraTargetParams)
                cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Standard);
            swordMat.SetFloat("_EmPower", minimumEmission);
            //if (nemmandoController)
            //  nemmandoController.UncoverScreen();
        }
    }
}