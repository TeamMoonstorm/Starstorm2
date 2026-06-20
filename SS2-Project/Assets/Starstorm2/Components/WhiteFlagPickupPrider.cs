using System.Linq;
using RoR2;
using SS2;
using SS2.Equipments;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class WhiteFlagPickupPrider : MonoBehaviour
    {
        [SerializeField]
        public MeshRenderer flagRenderer;
        
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private Material flagMat;

        public void OnEnable()
        {
            if (!WhiteFlag.usePrideEdits) return;
            
            flagMat = Instantiate(flagRenderer.material);
            flagMat.SetTexture(MainTex, WhiteFlag.flagTextures.Keys.ToArray()[PrideHelper.GetFlagIndexFromName("gay")]);
            flagRenderer.material = flagMat;
        }

        private void OnDestroy()
        {
            if (!WhiteFlag.usePrideEdits) return;
            
            Destroy(flagMat);
        }
    }
}