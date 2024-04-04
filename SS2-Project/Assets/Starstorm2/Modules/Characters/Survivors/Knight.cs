using MSU;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.ContentManagement;
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
            ParallelAssetLoadCoroutineHelper helper = new ParallelAssetLoadCoroutineHelper();

            helper.AddAssetToLoad<GameObject>("KnightBody", SS2Bundle.Indev);
            helper.AddAssetToLoad<SurvivorDef>("survivorKnight", SS2Bundle.Indev);

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            _prefab = helper.GetLoadedAsset<GameObject>("KnightBody");
            _survivorDef = helper.GetLoadedAsset<SurvivorDef>("survivorKnight");
        }
    }
}
#endif
