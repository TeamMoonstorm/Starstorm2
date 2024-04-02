using RoR2;
using UnityEngine;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using MSU;
using System.Collections;
#if DEBUG
namespace SS2.Survivors
{
    public sealed class NemCaptain : SS2Survivor
    {
        public override SurvivorDef SurvivorDef => _survivorDef;
        private SurvivorDef _survivorDef;
        public override NullableRef<GameObject> MasterPrefab => _monsterMaster;
        private GameObject _monsterMaster;
        public override GameObject CharacterPrefab => _prefab;
        private GameObject _prefab;

        private BuffDef _droneBuff;

        public override void Initialize()
        {
            ModifyPrefab();
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            if(sender.HasBuff(_droneBuff))
            {
                args.armorAdd += 30f;
                args.baseAttackSpeedAdd += 0.2f;
            }
        }

        public void ModifyPrefab()
        {
            var cb = _prefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
            //cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
        }

        public override bool IsAvailable()
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            ParallelAssetLoadCoroutineHelper helper = new ParallelAssetLoadCoroutineHelper();
            helper.AddAssetToLoad<GameObject>("NemCaptainBody", SS2Bundle.Indev);
            helper.AddAssetToLoad<SurvivorDef>("survivorNemCaptain", SS2Bundle.Indev);
            helper.AddAssetToLoad<BuffDef>("bdNemCapDroneBuff", SS2Bundle.Indev);

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            _survivorDef = helper.GetLoadedAsset<SurvivorDef>("survivorNemCaptain");
            _prefab = helper.GetLoadedAsset<GameObject>("NemCaptainBody");
            _droneBuff = helper.GetLoadedAsset<BuffDef>("bdNemCapDroneBuff");
        }
    }
}
#endif