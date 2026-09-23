using RoR2;
using SS2;
using SS2.Components;
using UnityEngine;

namespace EntityStates.NemToolbot
{
    /// <summary>
    /// Ball form special. Plays a transition animation then returns the body state
    /// to main (GenericCharacterMain / deployed main state). Follows the ToolbotStanceSwap
    /// timing pattern.
    /// </summary>
    public class StanceSwapToDeployed : BaseState
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

            if (!gameObject.TryGetComponent(out controller) || !controller.SetBallForm(false))
            {
                SS2Log.Error("NemToolbot StanceSwapToDeployed: Missing or unconfigured NemToolbotController.");
                if (isAuthority)
                    outer.SetNextStateToMain();
                return;
            }

            // Util.PlaySound(enterSoundString, gameObject);
            // PlayCrossfade("Body", "TransformToDeployed", "StanceSwap.playbackRate", duration, 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
