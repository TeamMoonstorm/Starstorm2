namespace EntityStates.Emotes
{
    /*public class BaseEmote : BaseState
    {
        public string soundString;
        public string animString;
        public float duration;
        public float animDuration;
        public bool normalizeModel;

        private uint activePlayID;
        private Animator animator;
        private ChildLocator childLocator;
        private CharacterCameraParams defaultParams;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            childLocator = GetModelChildLocator();

            characterBody.hideCrosshair = true;

            if (GetAimAnimator()) GetAimAnimator().enabled = false;
            animator.SetLayerWeight(animator.GetLayerIndex("AimPitch"), 0);
            animator.SetLayerWeight(animator.GetLayerIndex("AimYaw"), 0);

            if (animDuration == 0 && duration != 0) animDuration = duration;

            PlayAnimation("FullBody, Override", animString, "Emote.playbackRate", animDuration);

            activePlayID = Util.PlaySound(soundString, gameObject);

            if (normalizeModel)
            {
                if (modelLocator)
                {
                    modelLocator.normalizeToFloor = true;
                }
            }

            defaultParams = cameraTargetParams.cameraParams;
            cameraTargetParams.cameraParams = CameraParams.emoteCameraParams;
        }

        public override void OnExit()
        {
            base.OnExit();

            characterBody.hideCrosshair = false;

            if (GetAimAnimator()) GetAimAnimator().enabled = true;
            if (animator)
            {
                animator.SetLayerWeight(animator.GetLayerIndex("AimPitch"), 1);
                animator.SetLayerWeight(animator.GetLayerIndex("AimYaw"), 1);
            }

            if (normalizeModel)
            {
                if (modelLocator)
                {
                    modelLocator.normalizeToFloor = false;
                }
            }

            PlayAnimation("FullBody, Override", "BufferEmpty");
            if (activePlayID != 0) AkSoundEngine.StopPlayingID(activePlayID);

            cameraTargetParams.cameraParams = defaultParams;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            bool flag = false;

            if (characterMotor)
            {
                if (!characterMotor.isGrounded) flag = true;
                if (characterMotor.velocity != Vector3.zero) flag = true;
            }

            if (inputBank)
            {
                if (inputBank.skill1.down) flag = true;
                if (inputBank.skill2.down) flag = true;
                if (inputBank.skill3.down) flag = true;
                if (inputBank.skill4.down) flag = true;
                if (inputBank.jump.down) flag = true;

                if (inputBank.moveVector != Vector3.zero) flag = true;
            }

            //emote cancels
            if (isAuthority && characterMotor.isGrounded)
            {
                if (Input.GetKeyDown(Config.restKeybind))
                {
                    outer.SetInterruptState(new RestEmote(), InterruptPriority.Any);
                    return;
                }
                else if (Input.GetKeyDown(Config.tauntKeybind))
                {
                    outer.SetInterruptState(new TauntEmote(), InterruptPriority.Any);
                    return;
                }
            }

            if (duration > 0 && fixedAge >= duration) flag = true;

            if (flag)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }*/
}
