using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    public class AddRotationByDifficulty : MonoBehaviour
    {
        public bool localRotationOnly = true;
        public Vector3 rotationPerDifficultyIncrease;


        private const float maxScaling = 3.5f;
        private void Start()
        {
            if (Run.instance)
            {
                Vector3 totalRotation = rotationPerDifficultyIncrease * (Mathf.Clamp(DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue, 1, maxScaling) - 1);
                if (localRotationOnly)
                    transform.localEulerAngles += totalRotation;
                else
                    transform.eulerAngles += totalRotation;
            }
        }
    }
}