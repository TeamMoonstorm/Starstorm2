using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Starstorm2.Modules.EntityStates.Knight
{
    // TODO: Make it happen only in combat
    public class InvigoratePassive : BaseState
    {
        public static float baseDuration;
        private float duration;
        public static GameObject buffWard;
        private GameObject wardInstance;
        private bool hasBuffed;
        private Animator animator;
        public static string mecanimParameter;

        public override void OnEnter()
        {
            base.OnEnter();
            hasBuffed = false;
            animator = GetModelAnimator();
            duration = baseDuration / attackSpeedStat;

            //PlayCrossfade("FullBody, Override", "SecondaryBuff", "Secondary.playbackRate", duration, 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //if (animator.GetFloat(mecanimParameter) >= 0.5f && !hasBuffed && isAuthority)
            if (!hasBuffed && isAuthority)
            {
                hasBuffed = true;

                wardInstance = UnityEngine.Object.Instantiate(buffWard);
                wardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                wardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject);
                //Util.PlaySound();
            }

            if (fixedAge >= duration)
                outer.SetNextStateToMain();
        }
    }
}