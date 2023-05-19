using Moonstorm;
using Moonstorm.Starstorm2;
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
        [TokenModifier("SS2_NEMMANDO_SPECIAL_BOSS_DESCRIPTION", StatTypes.MultiplyByN, 1, "100")]
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
        public Material swordMat;
        public Material matInstance;
        //private NemmandoController nemmandoController;
        private float minimumEmission;
        private string skinNameToken;

        public CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.isSprinting = false;
            duration = baseDuration / attackSpeedStat;
            hitCount = Mathf.RoundToInt(Util.Remap(charge, 0f, 1f, minHits, maxHits));

            SS2Log.Debug("hit count inital: " + hitCount + "| " + characterBody.attackSpeed);
            int hitCountModified = (int)(hitCount * ((.5f * characterBody.attackSpeed) + .5f));
            hitCount = Mathf.Max(hitCount, hitCountModified);
            SS2Log.Debug("hit count after: " + hitCount);

            damageCoefficient = maxDamageCoefficient;
            radius = maxRadius;
            emission = Util.Remap(charge, 0f, 1f, minEmission, maxEmission);
            //nemmandoController = GetComponent<NemmandoController>();
            characterBody.hideCrosshair = false;

            skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

            if (skinNameToken != "SS2_SKIN_NEMCOMMANDO_DEFAULT" && skinNameToken != "SS2_SKIN_NEMCOMMANDO_GRANDMASTERY")
            {
                //Yellow
                if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_MASTERY")
                {
                    effectPrefab = SS2Assets.LoadAsset<GameObject>("DecisiveStrikeSlashYellow", SS2Bundle.NemCommando);
                }
                //Blue
                if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_COMMANDO")
                {
                    effectPrefab = SS2Assets.LoadAsset<GameObject>("DecisiveStrikeSlashBlue", SS2Bundle.NemCommando);
                }
            }
            //Red
            else
            {
                effectPrefab = SS2Assets.LoadAsset<GameObject>("DecisiveStrikeSlash", SS2Bundle.NemCommando);
            }

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
            
            //swordMat = GetModelTransform().GetComponent<CharacterModel>().baseRendererInfos[1].defaultMaterial;
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

            //if (swordMat)
            //    swordMat.SetFloat("_EmPower", Util.Remap(fixedAge, 0, duration, emission, minimumEmission));

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

            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, duration/1.5f);
                //cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Standard);
            }
            ref CharacterModel.RendererInfo renderInfo = ref GetModelTransform().GetComponent<CharacterModel>().baseRendererInfos[1];
            if (matInstance)
            {
                //Object.Destroy(matInstance);
                renderInfo.defaultMaterial = swordMat;
                swordMat.SetFloat("_EmPower", 1f);
                renderInfo.defaultMaterial.SetFloat("_EmPower", 1f);
                Object.Destroy(matInstance);
            }
            
            renderInfo.defaultMaterial.SetFloat("_EmPower", 1f);
            //swordMat.SetFloat("_EmPower", minimumEmission);
            //if (nemmandoController)
            //  nemmandoController.UncoverScreen();
        }
    }
}