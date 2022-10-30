using RoR2;
using UnityEngine;

namespace EntityStates.Nemmando
{
    public class ChargeSwordBeam : BaseSkillState
    {
        public static float baseChargeDuration = 1.25f;
        public static float maxEmission;
        public static float minEmission;
        public static GameObject lightningEffect;

        private Material swordMat;
        private float chargeDuration;
        private ChildLocator childLocator;
        private Animator animator;
        private Transform modelBaseTransform;
        private GameObject effectInstance;
        private GameObject defaultCrosshair;
        private uint chargePlayID;

        public override void OnEnter()
        {
            base.OnEnter();
            chargeDuration = baseChargeDuration / attackSpeedStat;
            childLocator = GetModelChildLocator();
            modelBaseTransform = GetModelBaseTransform();
            animator = GetModelAnimator();
            defaultCrosshair = characterBody.defaultCrosshairPrefab;

            //RoR2.UI.CrosshairUtils.RequestOverrideForBody(characterBody, Resources.Load<GameObject>("Prefabs/Crosshair/MageCrosshair"), RoR2.UI.CrosshairUtils.OverridePriority.Skill);

            if (lightningEffect)
            {
                effectInstance = Object.Instantiate(lightningEffect, childLocator.FindChild("LightingEffect"));
                effectInstance.GetComponentInChildren<ParticleSystem>().Play();
            }
            PlayCrossfade("Gesture, Override", "Secondary1", "Secondary.playbackRate", chargeDuration, 0.05f);

            swordMat = GetModelTransform().GetComponent<CharacterModel>().baseRendererInfos[1].defaultMaterial;

            chargePlayID = Util.PlaySound("NemmandoChargeBeam2", gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = CalcCharge();

            characterBody.SetSpreadBloom(Util.Remap(charge, 0f, 1f, 0f, 3f), true);

            StartAimMode();

            if (swordMat)
            {
                swordMat.SetFloat("_EmPower", Util.Remap(charge, 0, 1, minEmission, maxEmission));
            }

            if (isAuthority && (charge >= 1f || (!IsKeyDownAuthority() && fixedAge >= 0.1f)))
            {
                FireSwordBeam nextState = new FireSwordBeam();
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
            swordMat.SetFloat("_EmPower", minEmission);
            swordMat.SetColor("_EmColor", Color.red);
            if (effectInstance)
                Destroy(effectInstance);
            //characterBody.crosshairPrefab = null;
            AkSoundEngine.StopPlayingID(chargePlayID);
            //swordVFX.Stop();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}