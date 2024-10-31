using SS2;
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
        private string skinNameToken;

        public CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private CharacterCameraParamsData decisiveCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 70f,
            minPitch = -70f,
            pivotVerticalOffset = 1f, //how far up should the camera go?
            idealLocalCameraPos = zoomCameraPosition,
            wallCushion = 0.1f
        };
        public static Vector3 zoomCameraPosition = new Vector3(0f, 0f, -5.3f); // how far back should the camera go?
        Material matInstance;

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

                skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                if (skinNameToken != "SS2_SKIN_NEMCOMMANDO_DEFAULT" && skinNameToken != "SS2_SKIN_NEMCOMMANDO_GRANDMASTERY")
                {
                    //Yellow
                    if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_MASTERY" || skinNameToken.Contains("YELLOW"))
                    {
                        chargeEffect = SS2Assets.LoadAsset<GameObject>("DecisiveStrikeChargeYellow", SS2Bundle.NemCommando);
                    }
                    //Blue
                    if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_COMMANDO" || skinNameToken.Contains("BLUE"))
                    {
                        chargeEffect = SS2Assets.LoadAsset<GameObject>("DecisiveStrikeChargeBlue", SS2Bundle.NemCommando);
                    }
                }
                //Red
                else
                {
                    chargeEffect = SS2Assets.LoadAsset<GameObject>("DecisiveStrikeCharge", SS2Bundle.NemCommando);
                }

                chargeEffectInstance = Object.Instantiate(chargeEffect, chargeTransform);
                //I hate this code but it just works
                if ((bool)chargeEffectInstance)
                {
                    //Haha child locator in effect prefab go BRRRR
                    ChildLocator effectChildLocator = chargeEffectInstance.GetComponent<ChildLocator>();
                    var main = effectChildLocator.FindChild("Charge").GetComponent<ParticleSystem>().main;
                    main.startLifetime = chargeDuration;
                    //main = effectChildLocator.FindChild("Shadow").GetComponent<ParticleSystem>().main;
                    //main.startLifetime = chargeDuration;
                    Transform fullCharge = effectChildLocator.FindChild("FullCharge");
                    foreach (ParticleSystem particleSystem in fullCharge.GetComponentsInChildren<ParticleSystem>()) // bleh sry
                    {
                        var main2 = particleSystem.main;
                        main2.startDelay = chargeDuration;
                    }
                    foreach (ParticleSystem particleSystem in chargeEffectInstance.GetComponentsInChildren<ParticleSystem>())
                    {
                        particleSystem.Play();
                    }
                }
            }
            chargePlayID = Util.PlayAttackSpeedSound("NemmandoDecisiveStrikeCharge", gameObject, attackSpeedStat);
            PlayAnimation("FullBody, Override", "DecisiveStrikeCharge", "DecisiveStrike.playbackRate", chargeDuration);

            //
            CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
            {
                cameraParamsData = decisiveCameraParams,
                priority = 0f
            };
            camOverrideHandle = cameraTargetParams.AddParamsOverride(request, chargeDuration);

            //if (cameraTargetParams)
            //    cameraTargetParams.RequestAimType(CameraTargetParams.AimType.OverTheShoulder);
            ref CharacterModel.RendererInfo renderInfo = ref GetModelTransform().GetComponent<CharacterModel>().baseRendererInfos[1];

            swordMat = renderInfo.defaultMaterial;
            matInstance = Object.Instantiate(swordMat);
            renderInfo.defaultMaterial = matInstance;



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

            matInstance.SetFloat("_EmPower", Util.Remap(charge, 0, 1, minEmission, BossAttack.maxEmission));
            //swordMat.SetFloat("_EmPower", Util.Remap(charge, 0, 1, minEmission, BossAttack.maxEmission));

            if (areaIndicator) areaIndicator.localScale = Vector3.one * Util.Remap(charge, 0f, 1f, BossAttack.minRadius, BossAttack.maxRadius);

            if (charge >= 0.6f && !zoomin)
            {
                zoomin = true;
                //if (cameraTargetParams) cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
                //CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
                //{
                //    cameraParamsData = decisiveCameraParams,
                //    priority = 0f
                //};
                //camOverrideHandle = cameraTargetParams.AddParamsOverride(request, chargeDuration);
            }

            if (charge >= 1f && !finishedCharge)
            {
                finishedCharge = true;

                AkSoundEngine.StopPlayingID(chargePlayID);
                Util.PlaySound("NemmandoDecisiveStrikeReady", gameObject);

                //if (cameraTargetParams)
                //    cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);

                //CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
                //{
                //    cameraParamsData = decisiveCameraParams,
                //    priority = 0f
                //};
                //camOverrideHandle = cameraTargetParams.AddParamsOverride(request, 0.15f);
            }

            bool keyDown = IsKeyDownAuthority();
            if (GetTeam() == TeamIndex.Monster) keyDown = true;

            if (isAuthority && (fixedAge >= 1.25f * chargeDuration || !keyDown && fixedAge >= 0.1f))
            {
                BossAttackEntry nextState = new BossAttackEntry();
                nextState.charge = charge;
                nextState.camOverrideHandle = camOverrideHandle;
                nextState.matInstance = matInstance;
                nextState.swordMat = swordMat;
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

            PlayAnimation("FullBody, Override", "BufferEmpty");
            
            AkSoundEngine.StopPlayingID(chargePlayID);

            //if (cameraTargetParams) cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}