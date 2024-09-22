using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Engineer
{
    public class QuantumTranslocator : BaseSkillState
    {
        public float baseDuration = 0.5f;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(characterBody.master.netId);

            if (minionGroup != null)
            {
                foreach (var item in minionGroup.members)
                {
                    Debug.Log(item);
                }
            }
            else
            {
                Debug.Log("YOU FAILED");
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;

        }
    }
}
