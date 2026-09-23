using SS2.Equipments;
using UnityEngine;

namespace SS2.Components
{
    public class WhiteFlagPickupPrider : MonoBehaviour
    {
        [SerializeField]
        public MeshRenderer flagRenderer;
        
        public void OnEnable()
        {
            if (!WhiteFlag.usePrideEdits) return;
            
            flagRenderer.material = PrideHelper.flagMaterials[PrideHelper.GetFlagIndexFromName("gay")];
        }
    }
}