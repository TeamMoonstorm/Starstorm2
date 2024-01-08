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
            Debug.Log("DEBUGGER The passive was entered!!");
            Debug.Log("DEBUGGER duration: " + duration);
            Debug.Log("DEBUGGER baseDuration: " + baseDuration);
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
                Debug.Log("DEBUGGER wtf is the ward??" + buffWard.ToString());
                hasBuffed = true;
                wardInstance = UnityEngine.Object.Instantiate(buffWard);
                wardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                wardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject);
            }

            base.OnExit();
        }
    }
}