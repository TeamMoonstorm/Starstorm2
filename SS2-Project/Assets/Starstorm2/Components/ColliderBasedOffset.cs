using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    public class ColliderBasedOffset : MonoBehaviour
    {
        [Tooltip("-1 to 1 offset. 1 = top of bounds, -1 = bottom of bounds.")]
        public Vector3 normalizedOffset;

        private Transform parentTransform;
        private Collider bodyCollider;

        private void Start()
        {
            parentTransform = base.transform.parent;
            bodyCollider = base.transform.parent.GetComponent<Collider>();
        }

        private void Update()
        {
            Vector3 pos = parentTransform.position;
            if (bodyCollider)
            {
                pos = this.bodyCollider.bounds.center;
            }
            base.transform.position = pos + (new Vector3(
                normalizedOffset.x * bodyCollider.bounds.extents.x,
                normalizedOffset.y * bodyCollider.bounds.extents.y,
                normalizedOffset.z * bodyCollider.bounds.extents.z));
        }
    }
}
