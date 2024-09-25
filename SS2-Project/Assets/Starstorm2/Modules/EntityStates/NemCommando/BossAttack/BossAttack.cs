﻿using SS2;

using R2API;
using RoR2;
using UnityEngine;
using MSU;

namespace EntityStates.Nemmando
{
    public class BossAttack : BaseSkillState
    {
        public float charge;

        public static float baseDuration;
        [FormatToken("SS2_NEMMANDO_SPECIAL_BOSS_DESCRIPTION",   0)]
        public static int maxHits;
        public static int minHits;
        [FormatToken("SS2_NEMMANDO_SPECIAL_BOSS_DESCRIPTION", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float maxDamageCoefficient;
        public static float minDamageCoefficient;
        public static float maxRadius;
        public static float minRadius;
        public static float maxEmission;
        public static float minEmission;
        public static GameObject effectPrefab;
        public static GameObject hitEffectPrefab;

        private float hitStopwatch;
        private float duration;
        private int hitCount;
        private float damageCoefficient;
        private float radius;
        private float emission;
        private BlastAttack blastAttack;
        private EffectData attackEffect;
        public Material swordMat;
        public Material matInstance;
        private float minimumEmission;
        private string skinNameToken;

        public CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.isSprinting = false;
            duration = baseDuration / attackSpeedStat;
            hitCount = Mathf.RoundToInt(Util.Remap(charge, 0f, 1f, minHits, maxHits));

            int hitCountModified = (int)(hitCount * ((.5f * characterBody.attackSpeed) + .5f));
            hitCount = Mathf.Max(hitCount, hitCountModified);

            damageCoefficient = maxDamageCoefficient;
            radius = maxRadius;
            emission = Util.Remap(charge, 0f, 1f, minEmission, maxEmission);
            characterBody.hideCrosshair = false;

            characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

            if (skinNameToken != "SS2_SKIN_NEMCOMMANDO_DEFAULT" && skinNameToken != "SS2_SKIN_NEMCOMMANDO_GRANDMASTERY")
            {
                //Yellow
                if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_MASTERY" || skinNameToken.Contains("YELLOW"))
                {
                    effectPrefab = SS2Assets.LoadAsset<GameObject>("DecisiveStrikeSlashYellow", SS2Bundle.NemCommando);
                    hitEffectPrefab = SS2Assets.LoadAsset<GameObject>("NemCommandoImpactSlashEffectYellow", SS2Bundle.NemCommando);
                }
                //Blue
                if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_COMMANDO" || skinNameToken.Contains("BLUE"))
                {
                    effectPrefab = SS2Assets.LoadAsset<GameObject>("DecisiveStrikeSlashBlue", SS2Bundle.NemCommando);
                    hitEffectPrefab = SS2Assets.LoadAsset<GameObject>("NemCommandoImpactSlashEffectBlue", SS2Bundle.NemCommando);
                }
            }
            //Red
            else
            {
                effectPrefab = SS2Assets.LoadAsset<GameObject>("DecisiveStrikeSlash", SS2Bundle.NemCommando);
            }


            blastAttack = new BlastAttack()
            {
                attacker = gameObject,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                baseDamage = damageCoefficient * damageStat,
                baseForce = -500f,
                bonusForce = Vector3.zero,
                crit = RollCrit(),
                damageColorIndex = DamageColorIndex.Default,
                damageType = RoR2.DamageType.Stun1s,
                falloffModel = BlastAttack.FalloffModel.None,
                inflictor = gameObject,
                losType = BlastAttack.LoSType.None,
                position = characterBody.corePosition,
                procChainMask = default,
                procCoefficient = 1f,
                radius = radius,
                impactEffect = EffectCatalog.FindEffectIndexFromPrefab(hitEffectPrefab),
                teamIndex = GetTeam()
            };
            DamageAPI.AddModdedDamageType(blastAttack, SS2.Survivors.NemCommando.GougeDamageType);

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
            }
        }

        private void FireAttack()
        {
            hitStopwatch = duration / hitCount;

            if (isAuthority)
            {
                blastAttack.position = characterBody.corePosition;

                int hitcount = blastAttack.Fire().hitCount;
                if (hitcount > 0) Util.PlaySound(EntityStates.Merc.GroundLight.hitSoundString, gameObject);

                EffectData data = new EffectData();
                data.scale = characterBody.bestFitRadius * 10;
                data.origin = characterBody.corePosition;
                EffectManager.SpawnEffect(effectPrefab, data, true);

            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            hitStopwatch -= Time.fixedDeltaTime;
            emission -= 2f * Time.fixedDeltaTime;
            if (emission < 0f) emission = 0f;


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

            emission = 0f;

            characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;

            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, duration/1.5f);
            }
            ref CharacterModel.RendererInfo renderInfo = ref GetModelTransform().GetComponent<CharacterModel>().baseRendererInfos[1];
            /*if (matInstance && swordMat != null)
            {
                renderInfo.defaultMaterial = swordMat;
                swordMat.SetFloat("_EmPower", 1f);
                renderInfo.defaultMaterial.SetFloat("_EmPower", 1f);
                Object.Destroy(matInstance);
            }*/
            
            renderInfo.defaultMaterial.SetFloat("_EmPower", 1f);
        }
    }
}