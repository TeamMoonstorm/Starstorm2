using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.DamageAPI;

#if DEBUG
namespace SS2.Survivors
{

    public sealed class MULE : SS2Survivor
    {
        public override SurvivorDef SurvivorDef => _survivorDef;
        private SurvivorDef _survivorDef;
        public override NullableRef<GameObject> MasterPrefab => _monsterMaster;
        private GameObject _monsterMaster;
        public override GameObject CharacterPrefab => _prefab;
        private GameObject _prefab;

        private GameObject projectilePrefab;

        public static DamageAPI.ModdedDamageType NetDamageType { get; private set; }

        public override void Initialize()
        {
            ModifyPrefab();

            NetDamageType = DamageAPI.ReserveDamageType();
            GlobalEventManager.onServerDamageDealt += ApplyNet;
        }

        public override bool IsAvailable()
        {
            return false;
        }

        public void ModifyPrefab()
        {
            var cb = _prefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/RoboCratePod.prefab").WaitForCompletion(); ;
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }

        private void ApplyNet(DamageReport report)
        {
            var victimBody = report.victimBody;
            var damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, ModdedDamageType))
            {
                victimBody.AddTimedBuffAuthority(SS2Content.Buffs.bdMULENet.buffIndex, duration);
            }
        }

        public override IEnumerator LoadContentAsync()
        {
            ParallelAssetLoadCoroutineHelper helper = new ParallelAssetLoadCoroutineHelper();
            helper.AddAssetToLoad<GameObject>("MULEBody", SS2Bundle.Indev);
            helper.AddAssetToLoad<SurvivorDef>("survivorMULE", SS2Bundle.Indev);

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            _survivorDef = helper.GetLoadedAsset<SurvivorDef>("survivorMULE");
            _prefab = helper.GetLoadedAsset<GameObject>("MULEBody");
        }
    }
}
#endif