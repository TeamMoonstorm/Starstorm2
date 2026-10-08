using System;
using System.Collections.Generic;
using MSU;
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
        public List<string> skinBulbTokens = new();
        [SerializeField]
        public List<Material> skinBulbMaterials = new();
        
        private CharacterModel characterModel;

        private void OnEnable()
        {
            if (gameObject.transform.root?.TryGetComponent(out characterModel) != true) return;

            CharacterBody ownerBody = characterModel.AsValidOrNull()?.body.AsValidOrNull()?.master.AsValidOrNull()?.minionOwnership.AsValidOrNull()?.ownerMaster.AsValidOrNull()?.GetBody();
            int tokenIndex = skinBulbTokens.IndexOf(SkinSpecificOverrides.GetSkinName(ownerBody));
            if (tokenIndex == -1) return;
            
            itemDisplay.rendererInfos[0].renderer.material = skinBulbMaterials[tokenIndex];
            itemDisplay.rendererInfos[0].defaultMaterial = skinBulbMaterials[tokenIndex];
        }
    }
}