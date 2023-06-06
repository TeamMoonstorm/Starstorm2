using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    public class AddRotationByDifficulty : MonoBehaviour
    {
        public bool localRotationOnly = true;
        public Vector3 rotationPerDifficultyIncrease;
        public Vector3 transformOffsetPerDifficultyIncrease;


        private const float maxScaling = 3.5f;
        private void Start()
        {
            if (Run.instance)
            {
                Vector3 totalRotation = rotationPerDifficultyIncrease * (Mathf.Clamp(DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue, 1, maxScaling) - 1);
                Vector3 finalPosition = new Vector3(transformOffsetPerDifficultyIncrease.x * ((Mathf.Clamp(DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue, 1, maxScaling) - 1)) * 33f, transformOffsetPerDifficultyIncrease.y * (Mathf.Clamp(DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue, 1, maxScaling) - 1) + 300f, transformOffsetPerDifficultyIncrease.z * (Mathf.Clamp(DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue, 1, maxScaling) - 1) * -8.33f);
                if (localRotationOnly)
                    transform.localEulerAngles += totalRotation;
                else
                    transform.eulerAngles += totalRotation;
                transform.position = finalPosition;
            }
        }
    }
}