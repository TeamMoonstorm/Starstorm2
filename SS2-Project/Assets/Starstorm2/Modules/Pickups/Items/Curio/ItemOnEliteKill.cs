using RoR2;
using UnityEngine;
using RoR2.ContentManagement;
using UnityEngine.Networking;
namespace SS2.Items
{
    public sealed class ItemOnEliteKill : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("ItemOnEliteKill", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => SS2Config.enableBeta;
        private static BasicPickupDropTable dropTable;
        private static Xoroshiro128Plus dropRng;
        public override void Initialize()
        {
            dropTable = SS2Assets.LoadAsset<BasicPickupDropTable>("dtItemOnEliteKill", SS2Bundle.Items);
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
            Run.onRunStartGlobal += (run) =>
            {
                dropRng = new Xoroshiro128Plus(run.treasureRng.nextUlong);
            };
        }
        private void OnCharacterDeathGlobal(DamageReport damageReport)
        {
            if (!NetworkServer.active) return;
            if (damageReport.victimTeamIndex == TeamIndex.Player || !damageReport.victimBody) return;
            int eliteItems = SS2Util.GetItemCountForPlayers(SS2Content.Items.ItemOnEliteKill);
            if (eliteItems > 0 && damageReport.victimIsElite && Util.CheckRoll(6f + 2f * (eliteItems - 1))) // realizing i have no idea how to use run seeds/rng correctly
            {
                UniquePickup pickup = dropTable.GeneratePickup(dropRng);
                if (pickup.pickupIndex != PickupIndex.none)
                {
                    PickupDropletController.CreatePickupDroplet(new GenericPickupController.CreatePickupInfo
                    {
                        pickup = pickup,
                    },
                    damageReport.victimBody.corePosition, Vector3.up * 20f);
                }
            }
        }
    }
}
