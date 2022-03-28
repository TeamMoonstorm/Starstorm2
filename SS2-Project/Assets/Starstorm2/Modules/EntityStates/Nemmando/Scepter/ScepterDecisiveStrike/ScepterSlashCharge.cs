/*using Moonstorm.Starstorm2.Components;
using RoR2;
using UnityEngine;

namespace EntityStates.Nemmando
{
    public class ScepterSlashCharge : BaseSkillState
    {
        public static float baseChargeDuration = 1.75f;

        private float chargeDuration;
        private bool finishedCharge;
        private ChildLocator childLocator;
        private Animator animator;
        private Transform modelBaseTransform;
        private uint chargePlayID;
        private ParticleSystem swordVFX;
        private NemmandoController nemmandoController;
        private bool zoomin;
        private Material swordMat;
        private float minEmission;

        public override void OnEnter()
        {
            base.OnEnter();
            chargeDuration = baseChargeDuration;// / attackSpeedStat;
            childLocator = GetModelChildLocator();
            modelBaseTransform = GetModelBaseTransform();
            animator = GetModelAnimator();
            nemmandoController = GetComponent<NemmandoController>();
            zoomin = false;
            characterBody.hideCrosshair = true;
            if (nemmandoController) nemmandoController.chargingDecisiveStrike = true;

            if (characterBody.skinIndex == 2) minEmission = 70f;
            else minEmission = 0f;

            swordVFX = childLocator.FindChild("SwordChargeEffect").GetComponent<ParticleSystem>();

            var main = swordVFX.main;
            main.startLifetime = chargeDuration;

            main = swordVFX.transform.GetChild(0).GetComponent<ParticleSystem>().main;
            main.startLifetime = chargeDuration;

            main = swordVFX.transform.GetChild(1).GetComponent<ParticleSystem>().main;
            main.startDelay = chargeDuration;

            swordVFX.Play();
            chargePlayID = Util.PlayAttackSpeedSound("NemmandoDecisiveStrikeCharge", gameObject, attackSpeedStat);
            PlayAnimation("FullBody, Override", "DecisiveStrikeCharge", "DecisiveStrike.playbackRate", chargeDuration);

            if (cameraTargetParams) cameraTargetParams.aimMode = CameraTargetParams.AimType.OverTheShoulder;

            swordMat = GetModelTransform().GetComponent<ModelSkinController>().skins[characterBody.skinIndex].rendererInfos[1].defaultMaterial;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterMotor.velocity = Vector3.zero;
            float charge = CalcCharge();

            swordMat.SetFloat("_EmPower", Util.Remap(charge, 0, 1, minEmission, ScepterSlashAttack.swordEmission));

            if (charge >= 0.6f && !zoomin)
            {
                zoomin = true;
                if (cameraTargetParams) cameraTargetParams.aimMode = CameraTargetParams.AimType.Aura;
            }

            if (charge >= 1f && !finishedCharge)
            {
                finishedCharge = true;

                AkSoundEngine.StopPlayingID(chargePlayID);
                Util.PlaySound("NemmandoDecisiveStrikeReady", gameObject);

                if (cameraTargetParams) cameraTargetParams.aimMode = CameraTargetParams.AimType.Aura;
                if (nemmandoController) nemmandoController.CoverScreen();
            }

            if (isAuthority && (fixedAge >= 1.25f * chargeDuration || !IsKeyDownAuthority() && fixedAge >= 0.1f))
            {
                ScepterSlashEntry nextState = new ScepterSlashEntry();
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
            swordVFX.gameObject.SetActive(false);
            swordVFX.gameObject.SetActive(true);
            if (nemmandoController) nemmandoController.chargingDecisiveStrike = false;

            PlayAnimation("Gesture, Override", "BufferEmpty");

            AkSoundEngine.StopPlayingID(chargePlayID);

            if (cameraTargetParams)
            {
                cameraTargetParams.aimMode = CameraTargetParams.AimType.Aura;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}*/