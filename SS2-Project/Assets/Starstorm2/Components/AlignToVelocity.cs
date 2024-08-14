using UnityEngine;

namespace SS2.Components
{
    // cringe
    public class AlignToVelocity : MonoBehaviour
    {
        Vector3 prev;
        private void Update()
        {
            base.transform.rotation = Quaternion.LookRotation(base.transform.position - prev);
            prev = base.transform.position;
        }
    }
}
