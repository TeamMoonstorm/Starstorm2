namespace EntityStates.Nemmando
{
    /*public class ScepterBarrageCharge : BaseSkillState
    {
        public static float baseChargeDuration = 1.5f;

        private float chargeDuration;
        private ChildLocator childLocator;
        private Animator animator;
        private Transform modelBaseTransform;
        private uint chargePlayID;
        private bool hasFinishedCharging;
        private GameObject chargeEffect;

        public override void OnEnter()
        {
            base.OnEnter();
            chargeDuration = baseChargeDuration;// / attackSpeedStat;
            childLocator = GetModelChildLocator();
            modelBaseTransform = GetModelBaseTransform();
            animator = GetModelAnimator();
            hasFinishedCharging = false;

            chargePlayID = Util.PlaySound("NemmandoSubmissionCharge", gameObject);

            chargeEffect = childLocator.FindChild("GunChargeEffect").gameObject;

            if (chargeEffect) chargeEffect.SetActive(true);

            if (cameraTargetParams) cameraTargetParams.aimMode = CameraTargetParams.AimType.Aura;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = CalcCharge();

            if (charge >= 1f)
            {
                if (!hasFinishedCharging)
                {
                    hasFinishedCharging = true;
                    Util.PlaySound("NemmandoSubmissionReady", gameObject);
                }
            }

            if (isAuthority && ((!IsKeyDownAuthority() && fixedAge >= 0.1f)))
            {
                ScepterBarrageFire nextState = new ScepterBarrageFire();
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
            AkSoundEngine.StopPlayingID(chargePlayID);
            base.OnExit();

            if (chargeEffect) chargeEffect.SetActive(false);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }*/
}