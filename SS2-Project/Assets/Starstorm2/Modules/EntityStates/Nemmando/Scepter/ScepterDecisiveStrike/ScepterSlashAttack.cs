/*using Moonstorm.Starstorm2.Components;
using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Nemmando
{
    public class ScepterSlashAttack : BaseCustomSkillState
    {
        public float charge;

        public static float baseDuration = 2.5f;
        public static int baseHitCount = 12;
        public static float damageCoefficient = 3.8f;
        public static float radius = 64f;
        public static float swordEmission = 350f;

        private float hitStopwatch;
        private int hitCount;
        private int hitsFired;
        private float duration;
        private float emission;
        private EffectData attackEffect;
        private Material swordMat;
        private NemmandoController nemmandoController;
        private CharacterModel characterModel;
        private float minimumEmission;
        private bool isCrit;
        private bool hidden;

        private List<HurtBox> targetList;

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.isSprinting = false;
            hitsFired = 0;
            hitCount = (int)(baseHitCount * attackSpeedStat);
            duration = baseDuration;// attackSpeedStat;
            nemmandoController = GetComponent<NemmandoController>();
            emission = swordEmission;
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

            characterMotor.rootMotion = Vector3.zero;
            characterMotor.velocity = Vector3.zero;

            //EffectManager.SpawnEffect(Assets.nemChargedSlashStartFX, attackEffect, true);

            PlayAnimation("FullBody, Override", "DecisiveStrike", "DecisiveStrike.playbackRate", duration);
            Util.PlaySound("NemmandoDecisiveStrikeFire", gameObject);

            if (NetworkServer.active) characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);

            swordMat = GetModelTransform().GetComponent<ModelSkinController>().skins[characterBody.skinIndex].rendererInfos[1].defaultMaterial;

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
                        Util.PlaySound(effectComponent.impactSoundDef.eventName, i.gameObject);


                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.damage = damageCoefficient * damageStat;
                        damageInfo.attacker = gameObject;
                        damageInfo.procCoefficient = 1f;
                        damageInfo.position = hurtbox.transform.position;
                        damageInfo.crit = isCrit;
                        damageInfo.damageType = DamageType.BlightOnHit;

                        hurtbox.healthComponent.TakeDamage(damageInfo);

                        GlobalEventManager.instance.OnHitEnemy(damageInfo, hurtbox.healthComponent.gameObject);
                        GlobalEventManager.instance.OnHitAll(damageInfo, hurtbox.healthComponent.gameObject);

                        EffectData effect = new EffectData
                        {
                            scale = 4f,
                            origin = hurtbox.transform.position,
                            rotation = hurtbox.transform.rotation
                        };

                        //EffectManager.SpawnEffect(Assets.nemScepterImpactFX, effect, false);
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

            if (swordMat) swordMat.SetFloat("_EmPower", Util.Remap(fixedAge, 0, duration, emission, minimumEmission));

            characterMotor.rootMotion = Vector3.zero;
            characterMotor.velocity = Vector3.zero;

            if (fixedAge >= 0.4f * duration && hidden)
            {
                hidden = false;
                if (characterModel) characterModel.invisibilityCount--;
                PlayAnimation("FullBody, Override", "ScepterSpecial", "DecisiveStrike.playbackRate", 1f);
            }

            if (fixedAge >= 0.4f * duration && fixedAge <= duration)
            {
                CameraTargetParams ctp = cameraTargetParams;
                ctp.idealLocalCameraPos = new Vector3(0f, -1.4f, -6f);
            }

            if (fixedAge >= duration && hitsFired < hitCount && hitStopwatch <= 0f)
            {
                if (nemmandoController) nemmandoController.UncoverScreen();

                if (hitsFired == 0) PlayAnimation("FullBody, Override", "ScepterSpecialEnd", "DecisiveStrike.playbackRate", 1.4f);

                CameraTargetParams ctp = cameraTargetParams;
                ctp.idealLocalCameraPos = new Vector3(0f, 1.8f, -16f);

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

            if (cameraTargetParams) cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
            if (swordMat) swordMat.SetFloat("_EmPower", minimumEmission);
            if (nemmandoController) nemmandoController.UncoverScreen();
            if (NetworkServer.active) characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);

            characterBody.hideCrosshair = false;
        }
    }
}*/