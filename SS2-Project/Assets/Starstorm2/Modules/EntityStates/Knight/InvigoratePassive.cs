using EntityStates;
using RoR2;
using UnityEngine;

namespace Assets.Starstorm2.Modules.EntityStates.Knight
{
    public class InvigoratePassive : BaseSkillState
    {
        public static float duration = 5f;
        public static float baseDuration = 5f;
        public static GameObject buffWard;
        private GameObject wardInstance;
        private bool hasBuffed;
        public static string mecanimParameter;

        public override void OnEnter()
        {
            base.OnEnter();
            hasBuffed = false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            if (!hasBuffed && isAuthority)
            {
                hasBuffed = true;
                wardInstance = UnityEngine.Object.Instantiate(buffWard);
                wardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                wardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject);
            }

            base.OnExit();
        }
    }
}