using RoR2;
using SS2.Components;
using UnityEngine;
using UnityEngine.Networking;

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

            if (!gameObject.TryGetComponent(out controller))
            {
                Debug.LogError("NemToolbot StanceSwapToDeployed: Failed to get NemToolbotController on " + gameObject.name);
            }

            Debug.Log($"[NemToolbot] StanceSwapToDeployed: Exiting ball form (duration={duration:F2}s)");
            if (NetworkServer.active && controller != null)
            {
                controller.SetBallForm(false);
            }

            Util.PlaySound(enterSoundString, gameObject);
            PlayCrossfade("Body", "TransformToDeployed", "StanceSwap.playbackRate", duration, 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(fixedAge);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            fixedAge = reader.ReadSingle();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
