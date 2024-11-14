using UnityEngine;
using RoR2;
namespace SS2.Components
{
    [RequireComponent(typeof(EffectComponent))]
    public class EffectStickToTarget : MonoBehaviour
    {
        private Transform stuckTransform;
        private Vector3 localPosition;
        private void Start()
        {
            EffectComponent effectComponent = base.GetComponent<EffectComponent>();
            GameObject bodyObject = effectComponent.effectData.ResolveNetworkedObjectReference();
            if (!bodyObject)
            {
                SS2Log.Warning($"No body object for effect \"{base.gameObject.name}\"");
                return;
            }
            localPosition = bodyObject.transform.InverseTransformPoint(base.transform.position);
            stuckTransform = bodyObject.transform;
        }
        private void Update()
        {
            if(stuckTransform)
            {
                base.transform.position = this.stuckTransform.TransformPoint(this.localPosition);
            }
        }
    }
}
