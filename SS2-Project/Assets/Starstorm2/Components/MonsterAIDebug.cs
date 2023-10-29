using RoR2;
using UnityEngine;
using RoR2.CharacterAI;
namespace Moonstorm.Starstorm2.Components
{
    public class MonsterAIDebug : MonoBehaviour // should expand on this and make some UI. its pretty useful
    {
        private BaseAI ai;
        private AISkillDriver selection;
        private GameObject target;



        private void Awake()
        {
            this.ai = base.GetComponent<BaseAI>();
            if(!this.ai)
            {
                SS2Log.Warning("cant debug with no ai stupid");
                Destroy(this);
                return;
            }


        }

        private void FixedUpdate()
        {
            if((ai.skillDriverEvaluation.aimTarget != null && ai.skillDriverEvaluation.aimTarget.gameObject != target) 
                || ai.skillDriverEvaluation.dominantSkillDriver != selection)
            {
                Chat.AddMessage("-----------------------");
                Chat.AddMessage("AISkillDriver: " + (ai.skillDriverEvaluation.dominantSkillDriver ? ai.skillDriverEvaluation.dominantSkillDriver.customName : "None") );
                Chat.AddMessage("Target: " + (ai.skillDriverEvaluation.aimTarget != null ? ai.skillDriverEvaluation.aimTarget.gameObject.name : "None"));
                Chat.AddMessage("Distance: " + Mathf.Sqrt(ai.skillDriverEvaluation.separationSqrMagnitude));
                Chat.AddMessage("-----------------------");

                this.target = ai.skillDriverEvaluation.aimTarget != null ? ai.skillDriverEvaluation.aimTarget.gameObject : null;
                this.selection = ai.skillDriverEvaluation.dominantSkillDriver;
            }
        }
    }
}