using RoR2;
using SS2;
using SS2.Components;
using UnityEngine;

namespace EntityStates.NemToolbot
{
    /// <summary>
    /// Deployed form special. Plays a transition animation then sets the body state
    /// to BallMainState. Follows the ToolbotStanceSwap timing pattern.
    /// </summary>
    public class StanceSwapToBall : BaseState
    {
        [SerializeField]
        public float baseDuration = 0.5f;

        public static string enterSoundString = "";

        private float duration;
        private NemToolbotController controller;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;

            if (!gameObject.TryGetComponent(out controller) || !controller.SetBallForm(true))
            {
                SS2Log.Error("NemToolbot StanceSwapToBall: Missing or unconfigured NemToolbotController.");
                if (isAuthority)
                    outer.SetNextStateToMain();
                return;
            }

            // Util.PlaySound(enterSoundString, gameObject);
            // PlayCrossfade("Body", "TransformToBall", "StanceSwap.playbackRate", duration, 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextState(new BallMainState());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
