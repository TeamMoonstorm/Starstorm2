using RoR2;
using System.ComponentModel;
using UnityEngine;

namespace EntityStates.Engineer
{
    public class QuantumTranslocator : BaseSkillState
    {
        public float baseDuration = 0.5f;
        private float duration;

        public override void OnEnter()
        {
            MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(characterBody.master.netId);

            if (minionGroup != null)
            {
                foreach (var minion in minionGroup.members)
                {
                    if (!minion)
                    {
                        continue;
                    }

                    CharacterMaster masterComponent = minion.GetComponent<CharacterMaster>();

                    if (masterComponent)
                    {
                        
                        Debug.Log("CharacterMaster name is " + masterComponent.name);

                        CharacterBody characterBody = masterComponent.GetBody();
                        if (characterBody)
                        {
                            Debug.Log("CharacterBody baseNameToken is " + characterBody.baseNameToken);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("YOU FAILED");
            }

            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;

        }
    }
}
