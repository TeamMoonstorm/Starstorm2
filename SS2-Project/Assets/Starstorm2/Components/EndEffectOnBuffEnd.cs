using UnityEngine;
using RoR2;
namespace SS2.Components
{
    [RequireComponent(typeof(EffectComponent))]
    public class EndEffectOnBuffEnd : MonoBehaviour
    {
        public BuffDef targetBuff;
        private CharacterBody body;
        private void Start()
        {
            EffectComponent effectComponent = base.GetComponent<EffectComponent>();
            GameObject bodyObject = effectComponent.effectData.ResolveNetworkedObjectReference();
            if (!bodyObject)
            {
                SS2Log.Warning($"No body object for effect \"{base.gameObject.name}\"");
                return;
            }
            body = bodyObject.GetComponent<CharacterBody>();

        }

        private void FixedUpdate()
        {
            if (body && body.HasBuff(targetBuff)) return;

            Destroy(base.gameObject);
        }
    }
}
