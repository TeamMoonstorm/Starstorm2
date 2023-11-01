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
        public static GameObject blueBuffWard;
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

            hasBuffed = false;

            PlayCrossfade("FullBody, Override", "SecondaryBuff", "Secondary.playbackRate", duration, 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (animator.GetFloat(mecanimParameter) >= 0.5f && !hasBuffed && isAuthority)
            {
                hasBuffed = true;

                bool isBlue = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken == "SS2_SKIN_LAMP_BLUE";
                GameObject ward = isBlue ? blueBuffWard : buffWard;

                wardInstance = Object.Instantiate(ward);
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
