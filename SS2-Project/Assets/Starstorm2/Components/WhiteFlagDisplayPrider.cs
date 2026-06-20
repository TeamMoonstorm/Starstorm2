using RoR2;
using SS2.Equipments;
using UnityEngine;

namespace Starstorm2.Components
{
    public class WhiteFlagDisplayPrider : MonoBehaviour
    {
        [SerializeField]
        private ItemDisplay display;
        [SerializeField]
        private MeshRenderer displayFlagRenderer;
        
        private void OnEnable()
        {
            if (!WhiteFlag.usePrideEdits) return;
            
            Material displayFlagMat = PrideHelper.GetFlagMaterial(GetComponentInParent<CharacterModel>()?.body?.master);
            display.rendererInfos[0].defaultMaterial = displayFlagMat;
            displayFlagRenderer.material = displayFlagMat;
        }
    }
}