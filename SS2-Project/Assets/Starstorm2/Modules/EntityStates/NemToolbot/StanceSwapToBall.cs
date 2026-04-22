using RoR2;
using SS2.Components;
using UnityEngine;
using UnityEngine.Networking;

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

            if (!gameObject.TryGetComponent(out controller))
            {
                Debug.LogError("NemToolbot StanceSwapToBall: Failed to get NemToolbotController on " + gameObject.name);
            }

            Debug.Log($"[NemToolbot] StanceSwapToBall: Entering ball form (duration={duration:F2}s)");
            if (NetworkServer.active && controller != null)
            {
                controller.SetBallForm(true);
            }

            Util.PlaySound(enterSoundString, gameObject);
            PlayCrossfade("Body", "TransformToBall", "StanceSwap.playbackRate", duration, 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextState(new BallMainState());
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
