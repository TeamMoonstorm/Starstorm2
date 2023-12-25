using RoR2;
using UnityEngine;
using RoR2.CharacterAI;
using System.Text;
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
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("AISkillDriver: " + (ai.skillDriverEvaluation.dominantSkillDriver ? ai.skillDriverEvaluation.dominantSkillDriver.customName : "None")).AppendLine(); ;
                stringBuilder.Append("SkillSlot: " + (ai.skillDriverEvaluation.dominantSkillDriver ? ai.skillDriverEvaluation.dominantSkillDriver.skillSlot.ToString() : "None")).AppendLine();
                stringBuilder.Append("Target: " + (ai.skillDriverEvaluation.aimTarget != null ? ai.skillDriverEvaluation.aimTarget.gameObject.name : "None")).AppendLine();
                stringBuilder.Append("Distance: " + Mathf.Sqrt(ai.skillDriverEvaluation.separationSqrMagnitude)).AppendLine(); ;
                stringBuilder.Append("-----------------------").AppendLine(); ;
                Chat.AddMessage(stringBuilder.ToString());

                this.target = ai.skillDriverEvaluation.aimTarget != null ? ai.skillDriverEvaluation.aimTarget.gameObject : null;
                this.selection = ai.skillDriverEvaluation.dominantSkillDriver;
            }
        }
    }
}