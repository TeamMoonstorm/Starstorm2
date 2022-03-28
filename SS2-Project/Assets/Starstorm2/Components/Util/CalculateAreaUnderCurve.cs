using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{

    public class CalculateAreaUnderCurve : MonoBehaviour
    {
        public AnimationCurve curve;
        public Vector2 range;
        public int steps;

        public void OnEnable()
        {
            float stepLength = (range.y - range.x) / (float)steps;
            float estimate1 = 0;
            for (int i = 0; i < steps; i++)
                estimate1 += curve.Evaluate(stepLength * i) * stepLength;
            float estimate2 = 0;
            for (int i = steps; i > 0; i--)
                estimate2 += curve.Evaluate(stepLength * i) * stepLength;
            Debug.Log($"Area under curve is {(estimate1 + estimate2) / 2}");
        }
    }
}