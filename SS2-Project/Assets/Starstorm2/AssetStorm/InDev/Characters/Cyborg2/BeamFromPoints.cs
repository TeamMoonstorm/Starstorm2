using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    [ExecuteAlways]
    public class BeamFromPoints : MonoBehaviour
    {
        public Transform endPoint;
        public Transform startPoint;
        public Transform[] transformsToScale;
        public Transform[] transformsToRotate;

        private void Update()
        {
            Vector3 between = endPoint.position - startPoint.position;
            float distance = between.magnitude;


            foreach(Transform transform in transformsToScale)
            {
                Vector3 scale = transform.localScale;
                scale.z = distance;
                transform.localScale = scale;
            }
            foreach (Transform transform in transformsToRotate)
            {
                transform.forward = between;
            }
        }
    }
}
