using RoR2;
using UnityEngine;

namespace EntityStates.Chirr
{
    public class RootBurstCharge : BaseSkillState
    {
        public static float baseDuration;
        public static float minChargeDuration;
        public static float minBloomRadius;
        public static float maxBloomRadius;
        public static string chargeSoundString;
        public static GameObject chargeEffectPrefab;
        public static GameObject crosshairOverride;

        private float duration;
        private uint loopSoundInstanceId;
        private Animator animator;
        private ChildLocator childLocator;
        private GameObject chargeEffectInstance;
        private GameObject defaultCrosshair;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            animator = base.GetModelAnimator();
            childLocator = base.GetModelChildLocator();

            Transform transform = childLocator.FindChild("MuzzleFace") ?? base.characterBody.coreTransform;
            if (transform && chargeEffectPrefab)
            {
                chargeEffectInstance = UnityEngine.Object.Instantiate<GameObject>(chargeEffectPrefab, transform.position, transform.rotation);
                chargeEffectInstance.transform.parent = transform;
                ScaleParticleSystemDuration particleComp = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                ObjectScaleCurve scaleComp = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
                if (particleComp) particleComp.newDuration = duration;
                if (scaleComp) scaleComp.timeMax = duration;
            }

            PlayChargeAnimation();
            loopSoundInstanceId = Util.PlayAttackSpeedSound(chargeSoundString, base.gameObject, attackSpeedStat);
            defaultCrosshair = base.characterBody.defaultCrosshairPrefab;
            if (!crosshairOverride) crosshairOverride = Resources.Load<GameObject>("prefabs/crosshair/MageCrosshair");
            if (crosshairOverride)
                RoR2.UI.CrosshairUtils.RequestOverrideForBody(characterBody, crosshairOverride, RoR2.UI.CrosshairUtils.OverridePriority.Skill);
            base.StartAimMode(this.duration + 2f, false);
        }

        private void PlayChargeAnimation()
        {
            // an animation for charging the ability goes here
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = CalcCharge();
            if (base.isAuthority && ((!base.IsKeyDownAuthority() && base.fixedAge >= minChargeDuration) || base.fixedAge >= duration))
            {
                RootBurstFire nextState = new RootBurstFire();
                nextState.charge = charge;
                outer.SetNextState(nextState);
            }
        }

        public override void Update()
        {
            base.Update();
            base.characterBody.SetSpreadBloom(Util.Remap(CalcCharge(), 0f, 1f, minBloomRadius, maxBloomRadius), true);
        }

        private float CalcCharge()
        {
            return Mathf.Clamp01(base.fixedAge / duration);
        }

        public override void OnExit()
        {
            if (base.characterBody)
                RoR2.UI.CrosshairUtils.RequestOverrideForBody(characterBody, crosshairOverride, RoR2.UI.CrosshairUtils.OverridePriority.Skill);
            AkSoundEngine.StopPlayingID(loopSoundInstanceId);
            if (!outer.destroying) base.PlayAnimation("Gesture, Additive", "BufferEmpty");
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
