using UnityEngine;
using RoR2;
namespace SS2.Components
{
    [RequireComponent(typeof(EffectComponent))]
    public class CurseIconParticle : MonoBehaviour
    {
        public ParticleSystemRenderer renderer;
        private void Start()
        {          
            if (!renderer)
            {
                SS2Log.Warning($"No renderer for effect \"{base.gameObject.name}\"");
                return;
            }

            CurseIndex curseIndex = (CurseIndex)(Util.UintToIntMinusOne(base.GetComponent<EffectComponent>().effectData.genericUInt));
            renderer.sharedMaterial = CursePool.GetCurseMaterial(curseIndex);

        }
    }
}
