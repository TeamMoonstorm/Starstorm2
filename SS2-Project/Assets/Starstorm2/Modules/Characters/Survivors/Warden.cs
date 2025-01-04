using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;

namespace SS2.Survivors
{
    public class Warden : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acWarden", SS2Bundle.Indev);

        public static GameObject wardenProjectile;
        public static GameObject wardenSurgeWard;

        public override void Initialize()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += ModifyStats;
            wardenProjectile = AssetCollection.FindAsset<GameObject>("WardProjectile");
            wardenSurgeWard = AssetCollection.FindAsset<GameObject>("WardenSpecialWard");
        }

        private void ModifyStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(SS2Content.Buffs.bdWardenSurgeBuff))
            {
                args.attackSpeedMultAdd += 0.2f;
                args.moveSpeedMultAdd += 0.2f;
                args.armorAdd += 50f;
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
