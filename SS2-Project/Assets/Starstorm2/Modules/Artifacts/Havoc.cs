using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Moonstorm;
using Moonstorm.Components;

namespace Moonstorm.Starstorm2.Artifacts
{
    //[DisabledContent]
    public class Havoc : ArtifactBase
    {
        public override ArtifactDef ArtifactDef { get; } = SS2Assets.LoadAsset<ArtifactDef>("Havoc", SS2Bundle.Artifacts);

        public override void OnArtifactDisabled()
        { }

        public override void OnArtifactEnabled()
        { }
    }
}
