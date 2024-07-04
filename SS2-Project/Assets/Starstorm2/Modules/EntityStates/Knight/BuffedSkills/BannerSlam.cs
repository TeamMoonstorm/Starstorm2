using EntityStates;
using UnityEngine;
using RoR2;
using RoR2.Skills;
using EntityStates.Knight;
using UnityEngine.Networking;

namespace Assets.Starstorm2.Modules.EntityStates.Knight.BuffedSkills
{
    public class BannerSlam : BaseState
    {
        public static int duration;
        public static int swingTimeCoefficient;
        public static SkillDef originalSkillRef;

        public static SkillDef buffedSkillRef;
        public static GameObject powerBuffWard;
        public static GameObject slowBuffWard;

        private GameObject powerBuffWardInstance;
        private GameObject slowBuffWardInstance;
        public override void OnEnter()
        {
            base.OnEnter();

            PlayCrossfade("Body", "SwingSpecial", "Special.playbackRate", duration * swingTimeCoefficient, 0.15f);

            if (isAuthority & NetworkServer.active)
            {
                Debug.Log("DEBUGGER The banner slam network check was entered!!");
                Vector3 position = inputBank.aimOrigin - (inputBank.aimDirection);
                powerBuffWardInstance = UnityEngine.Object.Instantiate(powerBuffWard, position, Quaternion.identity);
                slowBuffWardInstance = UnityEngine.Object.Instantiate(slowBuffWard, position, Quaternion.identity);

                powerBuffWardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                slowBuffWardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;

                Debug.Log("DEBUGGER powerBuffWardInstance: " + powerBuffWardInstance);
                Debug.Log("DEBUGGER slowBuffWardInstance: " + slowBuffWardInstance);

                powerBuffWardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject);
                slowBuffWardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            GenericSkill originalSpecialSkill = skillLocator.special;
            originalSpecialSkill.UnsetSkillOverride(gameObject, BannerSpecial.buffedSkillRef, GenericSkill.SkillOverridePriority.Contextual);
            outer.SetNextStateToMain();
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }

}