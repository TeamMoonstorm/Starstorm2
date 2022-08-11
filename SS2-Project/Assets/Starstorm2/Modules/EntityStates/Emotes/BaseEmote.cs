namespace EntityStates.Emotes
{
    /*public class BaseEmote : BaseState
    {
           public class BaseEmote : BaseState
    {
        public string soundString;
        public string animString;
        public float duration;
        public float animDuration;

        private uint activePlayID;
        private Animator animator;
        private ChildLocator childLocator;

        public LocalUser localUser;

        private CharacterCameraParamsData emoteCameraParams = new CharacterCameraParamsData() {
            maxPitch = 70,
            minPitch = -70,
            pivotVerticalOffset = 1f,
            idealLocalCameraPos = emoteCameraPosition,
            wallCushion = 0.1f,
        };

        public static Vector3 emoteCameraPosition = new Vector3(0, 0.0f, -7.9f);

        private CameraParamsOverrideHandle camOverrideHandle;

        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            this.childLocator = base.GetModelChildLocator();
            this.localUser = LocalUserManager.readOnlyLocalUsersList[0];

            base.characterBody.hideCrosshair = true;

            if (base.GetAimAnimator()) base.GetAimAnimator().enabled = false;
            this.animator.SetLayerWeight(animator.GetLayerIndex("AimPitch"), 0);
            this.animator.SetLayerWeight(animator.GetLayerIndex("AimYaw"), 0);

            if (this.animDuration == 0 && this.duration != 0) this.animDuration = this.duration;

            if (this.duration > 0) base.PlayAnimation("FullBody, Override", this.animString, "Emote.playbackRate", this.duration);
            else base.PlayAnimation("FullBody, Override", this.animString, "Emote.playbackRate", this.animDuration);

            this.activePlayID = Util.PlaySound(soundString, base.gameObject);

            CameraParamsOverrideRequest request = new CameraParamsOverrideRequest {
                cameraParamsData = emoteCameraParams,
                priority = 0,
            };

            camOverrideHandle = base.cameraTargetParams.AddParamsOverride(request, 0.5f);

        }

        public override void OnExit()
        {
            base.OnExit();

            base.characterBody.hideCrosshair = false;

            if (base.GetAimAnimator()) base.GetAimAnimator().enabled = true;
            this.animator.SetLayerWeight(animator.GetLayerIndex("AimPitch"), 1);
            this.animator.SetLayerWeight(animator.GetLayerIndex("AimYaw"), 1);

            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            if (this.activePlayID != 0) AkSoundEngine.StopPlayingID(this.activePlayID);

            this.childLocator.FindChild("PickL").localScale = Vector3.one;
            this.childLocator.FindChild("PickR").localScale = Vector3.one;

            base.cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 0.5f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            bool flag = false;

            if (base.characterMotor)
            {
                if (!base.characterMotor.isGrounded) flag = true;
                if (base.characterMotor.velocity != Vector3.zero) flag = true;
            }
             
            if (base.inputBank)
            {
                if (base.inputBank.skill1.down) flag = true;
                if (base.inputBank.skill2.down) flag = true;
                if (base.inputBank.skill3.down) flag = true;
                if (base.inputBank.skill4.down) flag = true;
                if (base.inputBank.jump.down) flag = true;

                if (base.inputBank.moveVector != Vector3.zero) flag = true;
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
    }
}/*
