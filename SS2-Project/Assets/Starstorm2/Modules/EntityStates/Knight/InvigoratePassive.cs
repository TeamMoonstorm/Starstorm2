using EntityStates;
using RoR2;
using UnityEngine;

namespace Assets.Starstorm2.Modules.EntityStates.Knight
{
    // TODO: Make it happen only in combat
    public class InvigoratePassive : BaseSkillState
    {
        public static float baseDuration;
        public static GameObject buffWard;
        private GameObject wardInstance;
        private bool hasBuffed;
        public static string mecanimParameter;

        public override void OnEnter()
        {
            base.OnEnter();
            hasBuffed = false;
            //duration = baseDuration / attackSpeedStat;

            if (!hasBuffed && isAuthority)
            {
                hasBuffed = true;
                wardInstance = UnityEngine.Object.Instantiate(buffWard);
                wardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                wardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();


            //if (fixedAge >= duration)
            //    outer.SetNextStateToMain();
        }
    }
}