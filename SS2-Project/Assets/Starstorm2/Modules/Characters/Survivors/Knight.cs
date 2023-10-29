using RoR2;
using UnityEngine;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;

namespace Moonstorm.Starstorm2.Survivors
{
    [DisabledContent]
    public sealed class Knight : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("KnightBody", SS2Bundle.Indev);
        public override GameObject MasterPrefab { get; } = null;
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("survivorKnight", SS2Bundle.Indev);

        public override void Initialize()
        {
            base.Initialize();
            CharacterBody.onBodyStartGlobal += KnightBodyStart;
        }

        public override void ModifyPrefab()
        {
            var cb = BodyPrefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SimpleDotCrosshair.prefab").WaitForCompletion();
            //cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
        }

        private void KnightBodyStart(CharacterBody body)
        {
            if (body.baseNameToken == "SS2_KNIGHT_BODY_NAME")
                body.SetBuffCount(SS2Content.Buffs.bdFortified.buffIndex, 3);
        }
    }
}
