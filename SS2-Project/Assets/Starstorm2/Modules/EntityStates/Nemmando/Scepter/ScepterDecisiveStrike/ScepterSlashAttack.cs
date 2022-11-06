using Moonstorm;
using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.Components;
using Moonstorm.Starstorm2.DamageTypes;
using R2API;
using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace EntityStates.Nemmando
{
    public class ScepterSlashAttack : BaseSkillState
    {
        public static float baseDuration;
        //[TokenModifier("SS2_NEMMANDO_SPECIAL_BOSS_DESCRIPTION", StatTypes.Default, 0)]
        [TokenModifier("SS2_NEMMANDO_SPECIAL_SCEPBOSS_DESCRIPTION", StatTypes.Default, 0)]
        public static int maxHits;
        public static int minHits;
        [TokenModifier("SS2_NEMMANDO_SPECIAL_SCEPBOSS_DESCRIPTION", StatTypes.MultiplyByN, 1, "100")]
        public static float maxDamageCoefficient;
        public static float minDamageCoefficient;
        public static float maxRadius;
        public static float minRadius;
        public static float maxEmission;
        public static float minEmission;
        public static GameObject preSlashEffectPrefab; //NemmandoPreImpactScepterStrikeEffect
        public static GameObject slashEffectPrefab; //NemmandoImpactScepterStrikeEffect
        public static GameObject sheatheEffectPrefab; //NemmandoSheatheEffect
        public static GameObject stunEffectPrefab; //NemmandoImpactSlashEffect

        public float charge;
        //public static float baseDuration = 2.5f;
        //public static int baseHitCount = 12;
        //public static float damageCoefficient = 3.8f;
        //public static float radius = 64f;
        //public static float swordEmission = 350f;

        private float hitStopwatch;
        private int hitCount;
        private int hitsFired;
        private float duration;
        private float emission;
        private float radius;
        private float damageCoefficient;
        private EffectData attackEffect;

        private CharacterModel characterModel;
        private float minimumEmission;
        private bool isCrit;
        private bool hidden;

        public CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        public Material matInstance;
        public Material swordMat;

        private List<HurtBox> targetList;
        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.isSprinting = false;
            hitsFired = 0;
            hitCount = Mathf.RoundToInt(Util.Remap(charge, 0f, 1f, minHits, maxHits)); //(int)(baseHitCount * attackSpeedStat);

            SS2Log.Debug("hit count inital: " + hitCount + "| " + characterBody.attackSpeed);
            int hitCountModified = (int)(hitCount * characterBody.attackSpeed);
            hitCount = Mathf.Max(hitCount, hitCountModified);
            SS2Log.Debug("hit count after: " + hitCount);

            duration = baseDuration;// attackSpeedStat;
            radius = Util.Remap(charge, 0f, 1f, minRadius, maxRadius);
            damageCoefficient = Util.Remap(charge, 0f, 1f, minDamageCoefficient, maxDamageCoefficient);
            emission = Util.Remap(charge, 0f, 1f, minEmission, maxEmission); //swordEmission;
            isCrit = RollCrit();
            hidden = true;

            characterModel = modelLocator.modelTransform.GetComponent<CharacterModel>();
            characterBody.hideCrosshair = true;

            if (characterBody.skinIndex == 2) minimumEmission = 70f;
            else minimumEmission = 0f;

            attackEffect = new EffectData()
            {
                scale = 0.5f * radius,
                origin = characterBody.corePosition
            };
            EffectManager.SpawnEffect(preSlashEffectPrefab, attackEffect, true);

            characterMotor.rootMotion = Vector3.zero;
            characterMotor.velocity = Vector3.zero;

            
            PlayAnimation("FullBody, Override", "DecisiveStrike", "DecisiveStrike.playbackRate", duration);
            Util.PlaySound("NemmandoDecisiveStrikeFire", gameObject);
            
            if (NetworkServer.active) characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);

            GetTargets();

            if (characterModel) characterModel.invisibilityCount++;
        }
        private void FireAttack()
        {
            hitsFired++;

            if (targetList == null) return;

            hitStopwatch = 0.05f;
            foreach (HurtBox i in targetList)
            {
                if (i)
                {
                    HurtBoxGroup hurtboxGroup = i.hurtBoxGroup;
                    HurtBox hurtbox = hurtboxGroup.hurtBoxes[Random.Range(0, hurtboxGroup.hurtBoxes.Length - 1)];
                    if (hurtbox && hurtbox.healthComponent.alive)
                    {
                        EffectData effect = new EffectData
                        {
                            scale = 4f,
                            origin = hurtbox.transform.position,
                            rotation = hurtbox.transform.rotation
                        };
                        EffectManager.SpawnEffect(slashEffectPrefab, effect, true);

                        //Util.PlaySound(effectComponent.impactSoundDef.eventName, i.gameObject);
                        Util.PlaySound(EntityStates.Merc.GroundLight.hitSoundString, gameObject);

                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.damage = damageCoefficient * damageStat;
                        damageInfo.attacker = gameObject;
                        damageInfo.procCoefficient = 1f;
                        damageInfo.position = hurtbox.transform.position; 
                        damageInfo.crit = isCrit;
                        //damageInfo.damageType = (DamageType)Gouge.gougeDamageType;
                        DamageAPI.AddModdedDamageType(damageInfo, Gouge.gougeDamageType);

                        hurtbox.healthComponent.TakeDamage(damageInfo);
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, hurtbox.healthComponent.gameObject);
                        GlobalEventManager.instance.OnHitAll(damageInfo, hurtbox.healthComponent.gameObject);
                        
                    }
                }
            }
        }
        private void GetTargets()
        {
            targetList = new List<HurtBox>();
            Ray aimRay = GetAimRay();
            SphereSearch search = new SphereSearch();
            search.mask = LayerIndex.entityPrecise.mask;
            search.ClearCandidates();
            search.origin = transform.position;
            search.radius = radius;
            search.RefreshCandidates();
            search.FilterCandidatesByDistinctHurtBoxEntities();
            search.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(teamComponent.teamIndex));
            search.GetHurtBoxes(targetList);
            if (NetworkServer.active)
            {
                foreach (HurtBox i in targetList)
                {
                    if (i.healthComponent != healthComponent)
                    {
                        if (i.healthComponent.alive)
                        {
                            attackEffect = new EffectData()
                            {
                                scale = 0.5f * radius,
                                origin = i.healthComponent.body.corePosition
                            };
                            EffectManager.SpawnEffect(stunEffectPrefab, attackEffect, true);

                            Moonstorm.Starstorm2.Orbs.NemmandoDashOrb dashOrb = new Moonstorm.Starstorm2.Orbs.NemmandoDashOrb();
                            dashOrb.origin = transform.position;
                            dashOrb.target = i;
                            OrbManager.instance.AddOrb(dashOrb);
                        }
                    }
                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();  
            hitStopwatch -= Time.fixedDeltaTime;
            emission -= 10f * Time.fixedDeltaTime;
            if (emission < 0f) emission = 0f;

            characterMotor.rootMotion = Vector3.zero;
            characterMotor.velocity = Vector3.zero;
            if (fixedAge >= 0.4f * duration && hidden)
            {
                hidden = false;
                if (characterModel) characterModel.invisibilityCount--;
                PlayAnimation("FullBody, Override", "ScepterSpecial", "DecisiveStrike.playbackRate", 1f);

                EffectData effect = new EffectData
                {
                    scale = 4f,
                    origin = characterModel.transform.position,
                    rotation = characterModel.transform.rotation
                };
                EffectManager.SpawnEffect(preSlashEffectPrefab, effect, true);
            }
            if (fixedAge >= duration && hitsFired < hitCount && hitStopwatch <= 0f)
            {

                if (hitsFired == 0)
                {
                    PlayAnimation("FullBody, Override", "ScepterSpecialEnd", "DecisiveStrike.playbackRate", 1.4f);
                    EffectData effect = new EffectData
                    {
                        scale = 4f,
                        origin = characterModel.transform.position,
                        rotation = characterModel.transform.rotation
                    };
                    EffectManager.SpawnEffect(sheatheEffectPrefab, effect, true);
                }

                FireAttack();
            }
            if (isAuthority && fixedAge >= duration && hitsFired >= hitCount)
            {
                outer.SetNextStateToMain();
                return;
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, duration / 1.5f);
            }
            if (matInstance)
            {

                ref CharacterModel.RendererInfo renderInfo = ref GetModelTransform().GetComponent<CharacterModel>().baseRendererInfos[1];
                renderInfo.defaultMaterial = swordMat;
                Object.Destroy(matInstance);
            }
            if (NetworkServer.active) characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
        }
    }
}
