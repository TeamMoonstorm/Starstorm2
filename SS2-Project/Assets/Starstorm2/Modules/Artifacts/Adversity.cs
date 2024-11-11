using MonoMod.Cil;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Items;
using UnityEngine.Networking;
using Mono.Cecil.Cil;
using System;
using RoR2.Navigation;
using RoR2.Projectile;
using static MSU.BaseBuffBehaviour;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.PostProcessing;

namespace SS2.Artifacts
{
    public sealed class Adversity : SS2Artifact
    {
        public override SS2AssetRequest assetRequest => SS2Assets.LoadAssetAsync<ArtifactAssetCollection>("acAdversity", SS2Bundle.Artifacts);

        public static Material _reliquarySolarFlareEthereal;
        public static Material _reliquaryShellEthereal;
        public static PostProcessProfile _ppReliquaryEthereal;

        public override void Initialize(){
            _reliquarySolarFlareEthereal = assetCollection.FindAsset<Material>("matArtifactShellSolarFlareEthereal");
            _reliquaryShellEthereal = assetCollection.FindAsset<Material>("matArtifactShellOverlayEthereal");
            _ppReliquaryEthereal = assetCollection.FindAsset<PostProcessProfile>("ppReliquaryEthereal");
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override void OnArtifactDisabled(){}
        public override void OnArtifactEnabled(){}
    }
}
