using UnityEngine;
using RoR2;
namespace SS2.Components
{
    [RequireComponent(typeof(EffectComponent))]
    public class ParentEffectFromEffectData : MonoBehaviour
    {
        private Transform parentTransform;
        private void Start()
        {
            EffectComponent effectComponent = base.GetComponent<EffectComponent>();
            GameObject parent = effectComponent.effectData.ResolveNetworkedObjectReference();
            if (!parent)
            {
                SS2Log.Warning($"No parent for effect \"{base.gameObject.name}\"");
                return;
            }
            CharacterBody body = parent.GetComponent<CharacterBody>();
            if (body && body.coreTransform) this.parentTransform = body.coreTransform;

        }

        private void LateUpdate()
        {
            if(this.parentTransform)
            {
                base.transform.position = this.parentTransform.position;
            }
        }
    }
}
