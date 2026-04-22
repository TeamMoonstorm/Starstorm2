using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Lancer
{
    public class RecallSpear : BaseSkillState
    {
        public static float baseDuration = 0.3f;
        public static string recallSoundString = "";

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;

            PlayCrossfade("Gesture, Override", "RecallSpear", "RecallSpear.playbackRate", duration, 0.1f);
            Util.PlaySound(recallSoundString, gameObject);

            if (gameObject.TryGetComponent(out SS2.Components.LancerController lancerController))
            {
                if (NetworkServer.active)
                {
                    GameObject spearProjectile = lancerController.GetSpearProjectile();
                    if (spearProjectile)
                    {
                        if (spearProjectile.TryGetComponent(out SS2.Components.LancerSpearProjectile spearComponent))
                        {
                            spearComponent.BeginReturn();
                        }
                        else
                        {
                            Debug.LogError("RecallSpear: Spear projectile missing LancerSpearProjectile component.");
                        }
                    }

                    lancerController.SetSpearState(SS2.Components.LancerController.SpearState.Returning);
                }
            }
            else
            {
                Debug.LogError("RecallSpear: Failed to get LancerController component.");
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
