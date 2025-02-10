using RoR2;
using UnityEngine;
using RoR2.ContentManagement;
using UnityEngine.Networking;
namespace SS2.Items
{
    public sealed class ItemOnBossKill : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("ItemOnBossKill", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => true;
        private static BossDropTable dropTable;
        private static Xoroshiro128Plus dropRng;
        public override void Initialize()
        {
            dropTable = SS2Assets.LoadAsset<BossDropTable>("dtItemOnBossKill", SS2Bundle.Items);
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
            Run.onRunStartGlobal += (run) =>
            {
                if(NetworkServer.active)
                    dropRng = new Xoroshiro128Plus(run.treasureRng.nextUlong);
            };
        }
        private void OnCharacterDeathGlobal(DamageReport damageReport)
        {
            if (!NetworkServer.active) return;
            if (damageReport.victimTeamIndex == TeamIndex.Player || !damageReport.victimBody) return;
            int bossItems = SS2Util.GetItemCountForPlayers(SS2Content.Items.ItemOnBossKill);
            if (bossItems > 0 && damageReport.victimIsChampion)
            {
                PickupIndex pickupIndex = PickupIndex.none;
                if (Util.CheckRoll(1 - Mathf.Pow(0.97f, bossItems) * 100f) && damageReport.victimBody.TryGetComponent(out DeathRewards deathRewards))
                {
                    pickupIndex = deathRewards.bossDropTable ? deathRewards.bossDropTable.GenerateDrop(dropRng) : (PickupIndex)deathRewards.bossPickup;
                }
                if (pickupIndex == PickupIndex.none)
                    pickupIndex = dropTable.GenerateDrop(dropRng);
                if (pickupIndex != PickupIndex.none)
                {
                    PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, Vector3.up * 20f);
                }
            }
        }
    }
}
