using RoR2;
using UnityEngine;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;

namespace Moonstorm.Starstorm2.Survivors
{
    [DisabledContent]
    public sealed class Seraph : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("SeraphBody", SS2Bundle.Indev);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemmandoMonsterMaster", SS2Bundle.Indev);
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("survivorSeraph", SS2Bundle.Indev);

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void ModifyPrefab()
        {
            var cb = BodyPrefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SimpleDotCrosshair.prefab").WaitForCompletion();
            //cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
        }
    }
}
