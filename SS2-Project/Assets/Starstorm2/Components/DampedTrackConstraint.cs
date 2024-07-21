using System;
using UnityEngine;

namespace SS2.Components
{
    //Implementation of Blender's Damped Track
    [ExecuteAlways]
    public class DampedTrackConstraint : MonoBehaviour
    {
        public Transform source;

        private void LateUpdate()
        {
            if (source)
            {
                // thuis took an embarrassingly long time to figure out
                Vector3 obvec = Vector3.up;
                Vector3 tarvec = (source.position - transform.position).normalized;
                Vector3 raxis = Vector3.Cross(obvec, tarvec);
                float rangle = Vector3.Dot(obvec, tarvec);
                rangle = Mathf.Acos(rangle) * Mathf.Rad2Deg;
                base.transform.rotation = Quaternion.AngleAxis(rangle, raxis);
            }

        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(base.transform.position, source.position);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.up * 3f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(base.transform.position, base.transform.position + Vector3.Cross(source.position, transform.position).normalized);
        }
    }
}
