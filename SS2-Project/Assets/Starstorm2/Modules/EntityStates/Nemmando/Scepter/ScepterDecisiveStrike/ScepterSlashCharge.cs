using Moonstorm.Starstorm2.Components;
using RoR2;
using UnityEngine;

namespace EntityStates.Nemmando
{
    public class ScepterSlashCharge : BaseSkillState
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
        //private NemmandoController nemmandoController;
        private bool zoomin;
        private Material swordMat;
        private float minEmission;
        private GameObject chargeEffectInstance;

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

        //private GameObject chargeEffectInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            chargeDuration = baseChargeDuration;// / attackSpeedStat;
            childLocator = GetModelChildLocator();
            modelBaseTransform = GetModelBaseTransform();
            animator = GetModelAnimator();
            //nemmandoController = GetComponent<NemmandoController>();
            zoomin = false;
            characterBody.hideCrosshair = true;
            //if (nemmandoController) nemmandoController.chargingDecisiveStrike = true;

            //if (characterBody.skinIndex == 2)
            //{
            //    minEmission = 70f;
            //} else {
            //    minEmission = 0f;
            //}
            //
            //swordVFX = childLocator.FindChild("SwordChargeEffect").GetComponent<ParticleSystem>();
            //
            //var main = swordVFX.main;
            //main.startLifetime = chargeDuration;
            //
            //main = swordVFX.transform.GetChild(0).GetComponent<ParticleSystem>().main;
            //main.startLifetime = chargeDuration;
            //
            //main = swordVFX.transform.GetChild(1).GetComponent<ParticleSystem>().main;
            //main.startDelay = chargeDuration;
            //
            //swordVFX.Play();
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
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterMotor.velocity = Vector3.zero;
            float charge = CalcCharge();

            //matInstance.SetFloat("_EmPower", Util.Remap(charge, 0, 1, minEmission, ScepterSlashAttack.maxEmission));
            matInstance.SetFloat("_EmPower", Util.Remap(charge, 0, 1, 0, 350));
            //swordMat.SetFloat("_EmPower", Util.Remap(charge, 0, 1, minEmission, ScepterSlashAttack.swordEmission));

            if (charge >= 0.6f && !zoomin)
            {
                zoomin = true;
                //if (cameraTargetParams) cameraTargetParams.aimMode = CameraTargetParams.AimType.Aura;
            }

            if (charge >= 1f && !finishedCharge)
            {
                finishedCharge = true;

                AkSoundEngine.StopPlayingID(chargePlayID);
                Util.PlaySound("NemmandoDecisiveStrikeReady", gameObject);

                //if (cameraTargetParams) cameraTargetParams.aimMode = CameraTargetParams.AimType.Aura;
                //if (nemmandoController) nemmandoController.CoverScreen();
            }

            if (isAuthority && (fixedAge >= 1.25f * chargeDuration || !IsKeyDownAuthority() && fixedAge >= 0.1f))
            {
                ScepterSlashEntry nextState = new ScepterSlashEntry();
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

            //swordVFX.gameObject.SetActive(false);
            //swordVFX.gameObject.SetActive(true);
            //if (nemmandoController) nemmandoController.chargingDecisiveStrike = false;

            PlayAnimation("Gesture, Override", "BufferEmpty");

            AkSoundEngine.StopPlayingID(chargePlayID);
            if (chargeEffectInstance)
            {
                Destroy(chargeEffectInstance);
            }

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}