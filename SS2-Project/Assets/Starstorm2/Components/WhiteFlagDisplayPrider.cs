using RoR2;
using SS2.Equipments;
using UnityEngine;

namespace Starstorm2.Components
{
    public class WhiteFlagDisplayPrider : MonoBehaviour
    {
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        
        [SerializeField]
        private ItemDisplay display;
        
        [SerializeField]
        private MeshRenderer displayFlagRenderer;
        
        private CharacterBody characterBody;
        private Material displayFlagMat;
        
        private void OnEnable()
        {
            if (!WhiteFlag.usePrideEdits) return;
            
            characterBody = GetComponentInParent<CharacterModel>()?.body;
            displayFlagMat = Instantiate(displayFlagRenderer.material);
            
            displayFlagMat.SetTexture(MainTex, PrideHelper.GetFlagTexture(characterBody?.master));
            display.rendererInfos[0].defaultMaterial = displayFlagMat;
            displayFlagRenderer.material = displayFlagMat;
        }

        private void OnDestroy()
        {
            if (!WhiteFlag.usePrideEdits) return;
            
            Destroy(displayFlagMat);
        }
    }
}