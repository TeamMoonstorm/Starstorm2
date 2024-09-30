using System;
using UnityEngine;
using RoR2;
using ThreeEyedGames;
using UnityEngine.Rendering;
namespace SS2.Components
{
    // Decals crash the editor. this is a workaround
    public class RuntimeDecal : MonoBehaviour
    {
        protected Color _colorTransparent = new Color(0f, 0f, 0f, 0f);

        public Material material;
        public GameObject limitTo;
        public AnimateShaderAlpha animateShaderAlpha;
        private void OnEnable()
        {
            Decal decal = base.gameObject.AddComponent<Decal>();
            decal.Material = material;
            if (animateShaderAlpha) animateShaderAlpha.decal = decal;
            if (limitTo) decal.LimitTo = limitTo;

            base.GetComponent<MeshFilter>().sharedMesh = LegacyResourcesAPI.Load<Mesh>("DecalCube");
            MeshRenderer component = base.GetComponent<MeshRenderer>();
            component.shadowCastingMode = ShadowCastingMode.Off;
            component.receiveShadows = false;
            component.materials = new Material[0];
            component.lightProbeUsage = LightProbeUsage.BlendProbes;
            component.reflectionProbeUsage = ReflectionProbeUsage.Off;

            Destroy(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = base.transform.localToWorldMatrix;
            Gizmos.color = this._colorTransparent;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            Gizmos.color = Color.white * 0.2f;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = base.transform.localToWorldMatrix;
            Gizmos.color = Color.white * 0.5f;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}
