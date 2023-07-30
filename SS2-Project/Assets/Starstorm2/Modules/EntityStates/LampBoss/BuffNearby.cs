using EntityStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;

namespace EntityStates.LampBoss
{
    public class BuffNearby : BaseState
    {
        public static float baseDuration;
        private float duration;
        public static GameObject buffWard;
        private GameObject wardInstance;
        private bool hasBuffed;
        private Animator animator;
        public static string mecanimParameter;
        private float timer;

        public override void OnEnter()
        {
            base.OnEnter();
            hasBuffed = false;
            animator = GetModelAnimator();
            duration = baseDuration / attackSpeedStat;

            PlayCrossfade("FullBody, Override", "SecondaryBuff", "Secondary.playbackRate", duration, 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (animator.GetFloat(mecanimParameter) >= 0.5f && !hasBuffed)
            {
                hasBuffed = true;
                wardInstance = Object.Instantiate(buffWard);
                wardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                wardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject);
                //Util.PlaySound();
            }

            if (fixedAge >= duration)
                outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
