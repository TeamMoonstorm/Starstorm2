using RoR2;
using UnityEngine;

namespace EntityStates.Nemmando
{
    public class BossAttackCharge : BaseSkillState
    {
        public static float baseChargeDuration;
        public static GameObject chargeEffect;
        public static string childName;

        private float chargeDuration;
        private bool finishedCharge;
        private ChildLocator childLocator;
        private Animator animator;
        private Transform modelBaseTransform;
        private uint chargePlayID;
        private ParticleSystem swordVFX;
        private bool zoomin;
        private Material swordMat;
        private float minEmission;
        private GameObject chargeEffectInstance;
        private Transform areaIndicator;

        public override void OnEnter()
        {
            base.OnEnter();
            chargeDuration = baseChargeDuration / attackSpeedStat;
            childLocator = GetModelChildLocator();
            modelBaseTransform = GetModelBaseTransform();
            animator = GetModelAnimator();
            zoomin = false;
            characterBody.hideCrosshair = true;
            if (chargeEffect)
            {
                Transform chargeTransform = childLocator.FindChild(childName);
                chargeEffectInstance = Object.Instantiate(chargeEffect, chargeTransform);
                //I hate this code but it just works
                if ((bool)chargeEffectInstance)
                {
                    //Haha child locator in effect prefab go BRRRR
                    ChildLocator effectChildLocator = chargeEffectInstance.GetComponent<ChildLocator>();
                    var main = effectChildLocator.FindChild("Charge").GetComponent<ParticleSystem>().main;
                    main.startLifetime = chargeDuration;
                    main = effectChildLocator.FindChild("Shadow").GetComponent<ParticleSystem>().main;
                    main.startLifetime = chargeDuration;
                    main = effectChildLocator.FindChild("FullCharge").GetComponent<ParticleSystem>().main;
                    main.startDelay = chargeDuration;
                    foreach (ParticleSystem particleSystem in chargeEffectInstance.GetComponentsInChildren<ParticleSystem>())
                    {
                        particleSystem.Play();
                    }
                }
            }
            chargePlayID = Util.PlayAttackSpeedSound("NemmandoDecisiveStrikeCharge", gameObject, attackSpeedStat);
            PlayAnimation("FullBody, Override", "DecisiveStrikeCharge", "DecisiveStrike.playbackRate", chargeDuration);

            if (cameraTargetParams)
                cameraTargetParams.RequestAimType(CameraTargetParams.AimType.OverTheShoulder);

            swordMat = GetModelTransform().GetComponent<CharacterModel>().baseRendererInfos[1].defaultMaterial;

            if (GetTeam() == TeamIndex.Monster)
            {
                chargeEffectInstance = Object.Instantiate(new EntityStates.ImpBossMonster.BlinkState().blinkDestinationPrefab, gameObject.transform);
                chargeEffectInstance.transform.position = characterBody.corePosition;
                chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = chargeDuration;
                areaIndicator = chargeEffectInstance.transform.Find("Particles").Find("AreaIndicator");

                chargeEffectInstance.GetComponentInChildren<PostProcessDuration>().maxDuration = chargeDuration;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterMotor.velocity = Vector3.zero;
            float charge = CalcCharge();

            swordMat.SetFloat("_EmPower", Util.Remap(charge, 0, 1, minEmission, BossAttack.maxEmission));

            if (areaIndicator) areaIndicator.localScale = Vector3.one * Util.Remap(charge, 0f, 1f, BossAttack.minRadius, BossAttack.maxRadius);

            if (charge >= 0.6f && !zoomin)
            {
                zoomin = true;
                if (cameraTargetParams) cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }

            if (charge >= 1f && !finishedCharge)
            {
                finishedCharge = true;

                AkSoundEngine.StopPlayingID(chargePlayID);
                Util.PlaySound("NemmandoDecisiveStrikeReady", gameObject);

                if (cameraTargetParams)
                    cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }

            bool keyDown = IsKeyDownAuthority();
            if (GetTeam() == TeamIndex.Monster) keyDown = true;

            if (isAuthority && (fixedAge >= 1.25f * chargeDuration || !keyDown && fixedAge >= 0.1f))
            {
                BossAttackEntry nextState = new BossAttackEntry();
                nextState.charge = charge;
                outer.SetNextState(nextState);
            }
        }

        protected float CalcCharge()
        {
            return Mathf.Clamp01(fixedAge / chargeDuration);
        }

        public override void OnExit()
        {
            base.OnExit();
            if (chargeEffectInstance)
            {
                Destroy(chargeEffectInstance);
            }

            PlayAnimation("Gesture, Override", "BufferEmpty");

            AkSoundEngine.StopPlayingID(chargePlayID);

            if (cameraTargetParams) cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}