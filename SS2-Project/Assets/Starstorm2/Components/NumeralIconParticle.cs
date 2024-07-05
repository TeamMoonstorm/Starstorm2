using UnityEngine;
using RoR2;
namespace SS2.Components
{
    [RequireComponent(typeof(EffectComponent))]
    public class NumeralIconParticle : MonoBehaviour
    {
        public Vector3 screenPosition;
        public ParticleSystem ps;
        private void Start()
        {                     
            if (!ps)
            {
                SS2Log.Warning($"No ParticleSystem for effect \"{base.gameObject.name}\"");
                return;
            }
            UICamera.onUICameraPreRender += OnPreRenderUI;
            ParticleSystem.TextureSheetAnimationModule ts = ps.textureSheetAnimation;
            ts.startFrameMultiplier = 0;
            ts.frameOverTimeMultiplier = 0; /// cant see this in editor
            ts.startFrame = base.GetComponent<EffectComponent>().effectData.genericUInt;
            ps.Play();
        }
        private void OnDestroy()
        {
            UICamera.onUICameraPreRender -= OnPreRenderUI;
        }
        private void OnPreRenderUI(UICamera uiCam)
        {
            PositionForUI(uiCam.camera);
        }
        public void PositionForUI(Camera uiCamera)
        {
            Vector3 pos = new Vector3(screenPosition.x * uiCamera.pixelWidth, screenPosition.y * uiCamera.pixelHeight, screenPosition.z);
            base.transform.position = uiCamera.ScreenToWorldPoint(pos);
            
        }
    }
}
