using System;
using System.Linq;
using UnityEngine;


namespace Moonstorm.Starstorm2.Components
{
    public class CreateAnimationCurve : MonoBehaviour
    {
        public AnimationCurve outputCurve;

        public AnimationKeyframe[] keyframes;

        // Use this for initialization
        void Start()
        {
            outputCurve = new AnimationCurve(keyframes.Select(keyframe => keyframe.GetKeyframe()).ToArray());
        }

        [Serializable]
        public struct AnimationKeyframe
        {
            public float time;
            public float value;
            public float inTangent;
            public float outTangent;
            public float inWeight;
            public float outWeight;
            public WeightedMode weightedMode;
            public int tangentMode;

            public Keyframe GetKeyframe()
            {
                return new Keyframe(time, value, inTangent, outTangent, inWeight, outWeight);
            }
        }
    }
}