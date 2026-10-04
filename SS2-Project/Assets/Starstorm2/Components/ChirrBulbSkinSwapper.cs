using System;
using RoR2;
using SS2;
using SS2.Modules;
using UnityEngine;

namespace Starstorm2.Components
{
    public class ChirrBulbSkinSwapper : MonoBehaviour
    {
        [SerializeField]
        public ItemDisplay itemDisplay;
        [SerializeField]
        public Material isopodBulb;
        
        private CharacterModel characterModel;

        private void OnEnable()
        {
            if (gameObject.transform.root?.TryGetComponent(out characterModel) != true) return;

            CharacterBody ownerBody = characterModel.body?.master?.minionOwnership?.ownerMaster?.GetBody();
            if (SkinSpecificOverrides.GetSkinName(ownerBody) == "SS2_SKIN_CHIRR_ISOPOD")
            {
                itemDisplay.rendererInfos[0].renderer.material = isopodBulb;
                itemDisplay.rendererInfos[0].defaultMaterial = isopodBulb;
            }
        }
    }
}