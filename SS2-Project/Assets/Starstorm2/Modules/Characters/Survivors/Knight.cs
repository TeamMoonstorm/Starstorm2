using MSU;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.ContentManagement;
using R2API;
using static MSU.BaseBuffBehaviour;
#if DEBUG
namespace SS2.Survivors
{
    public sealed class Knight : SS2Survivor
    {
        public override SurvivorDef SurvivorDef => _survivorDef;
        private SurvivorDef _survivorDef;
        public override NullableRef<GameObject> MasterPrefab => _monsterMaster;
        private GameObject _monsterMaster;
        public override GameObject CharacterPrefab => _prefab;
        private GameObject _prefab;

        public override void Initialize()
        {
            CharacterBody.onBodyStartGlobal += KnightBodyStart;
            ModifyPrefab();
        }


        public void ModifyPrefab()
        {
            var cb = _prefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SimpleDotCrosshair.prefab").WaitForCompletion();
        }

        private void KnightBodyStart(CharacterBody body)
        {
            if (body.baseNameToken == "SS2_KNIGHT_BODY_NAME") // Every time one does an unecesary string comparasion, a developer dies -N
                body.SetBuffCount(SS2Content.Buffs.bdFortified.buffIndex, 3);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * GameObject - "KnightBody" - Indev
             * SurvivorDef - "survivorKnight" - Indev
             */
            yield break;
        }

        // TODO: Load the actual buff 
        // The buff behavior for Knight's default passive
        public class KnightPassiveBuff : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdKnightBuff;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.attackSpeedMultAdd += 0.4f;
                args.damageMultAdd += 0.4f;
            }
        }

        // TODO: Comment explaining the buff
        // TODO: replace public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdKnightCharged", SS2Bundle.Indev);
        public class KnightChargedUpBuff : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdKnightCharged;

            // TODO: idk if this works since in MSU 1.0 this was overriden property
            public Material OverlayMaterial { get; } = SS2Assets.LoadAsset<Material>("matKnightSuperShield", SS2Bundle.Indev);
        }

        // TODO: Comment explaining the buff
        // TODO: replace public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdShield", SS2Bundle.Indev);
        public class KnightShieldBuff : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdShield;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.armorAdd += 100f;
                args.moveSpeedReductionMultAdd += 0.6f;
            }
        }
    }
}
#endif
