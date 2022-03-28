// RoR2.ArtifactFormulaDisplay
using RoR2;
using System;
using ThreeEyedGames;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    public class ModifiedArtifactFormulaDisplay : MonoBehaviour
    {
        [Serializable]
        public struct ArtifactCompoundDisplayInfo
        {
            public ArtifactCompoundDef artifactCompoundDef;

            public VanillaCompoundEnum vanillaCompoundEnum;

            public Decal decal;
        }

        public ArtifactCompoundDisplayInfo[] artifactCompoundDisplayInfos;

        private void Start()
        {
            ArtifactCompoundDisplayInfo[] array = artifactCompoundDisplayInfos;
            for (int i = 0; i < array.Length; i++)
            {
                ArtifactCompoundDisplayInfo artifactCompoundDisplayInfo = array[i];
                if (artifactCompoundDisplayInfo.artifactCompoundDef != null)
                {
                    artifactCompoundDisplayInfo.decal.Material = artifactCompoundDisplayInfo.artifactCompoundDef.decalMaterial;
                }
                else
                {
                    switch (artifactCompoundDisplayInfo.vanillaCompoundEnum)
                    {
                        case VanillaCompoundEnum.Circle:
                            artifactCompoundDisplayInfo.decal.Material = Resources.Load<ArtifactCompoundDef>("artifactcompound/acdCircle").decalMaterial;
                            break;
                        case VanillaCompoundEnum.Diamond:
                            artifactCompoundDisplayInfo.decal.Material = Resources.Load<ArtifactCompoundDef>("artifactcompound/acdDiamond").decalMaterial;
                            break;
                        case VanillaCompoundEnum.Square:
                            artifactCompoundDisplayInfo.decal.Material = Resources.Load<ArtifactCompoundDef>("artifactcompound/acdSquare").decalMaterial;
                            break;
                        case VanillaCompoundEnum.Triangle:
                            artifactCompoundDisplayInfo.decal.Material = Resources.Load<ArtifactCompoundDef>("artifactcompound/acdTriangle").decalMaterial;
                            break;
                        case VanillaCompoundEnum.None:
                            artifactCompoundDisplayInfo.decal.Material = null;
                            break;
                    }
                }
            }
        }
    }

    public enum VanillaCompoundEnum
    {
        Circle,
        Square,
        Triangle,
        Diamond,
        None
    }
}
