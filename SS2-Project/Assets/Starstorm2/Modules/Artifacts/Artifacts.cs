using R2API;
using R2API.ScriptableObjects;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Artifacts : ArtifactModuleBase
    {
        public static Artifacts Instance { get; private set; }
        public override R2APISerializableContentPack SerializableContentPack { get; } = SS2Content.Instance.SerializableContentPack;

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            SS2Log.Info($"Initializing Artifacts");
            GetArtifactBases();

            var compound = SS2Assets.LoadAsset<ArtifactCompoundDef>("acdStar", SS2Bundle.Artifacts);
            //compound.decalMaterial.shader = Resources.Load<ArtifactCompoundDef>("artifactcompound/acdCircle").decalMaterial.shader;
            ArtifactCodeAPI.AddCompound(compound);
        }

        protected override IEnumerable<ArtifactBase> GetArtifactBases()
        {
            base.GetArtifactBases()
                .ToList()
                .ForEach(artifact => AddArtifact(artifact));
            return null;
        }
    }
}