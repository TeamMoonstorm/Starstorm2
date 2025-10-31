using RoR2;
using UnityEngine;
using RoR2.ContentManagement;
using RoR2.Orbs;
using R2API;
using UnityEngine.Networking;
namespace SS2.Items
{
    public sealed class ShieldGate : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acShieldGate", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => SS2Config.enableBeta;
        public static GameObject orbEffect;
        public override void Initialize()
        {
            orbEffect = SS2Assets.LoadAsset<GameObject>("ShieldOnKillOrbEffect", SS2Bundle.Items);
            RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
        }

        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender.inventory) return;
            args.baseShieldAdd += sender.inventory.GetItemCount(SS2Content.Items.StackShieldGate);
        }

        private void OnCharacterDeathGlobal(DamageReport damageReport)
        {
            if (!NetworkServer.active) return;
            Inventory inventory = damageReport.attackerMaster ? damageReport.attackerMaster.inventory : null;
            if (inventory)
            {
                int shield = inventory.GetItemCount(SS2Content.Items.ShieldGate);
                Vector3 origin = damageReport.victimBody ? damageReport.victimBody.corePosition : Vector3.zero;
                if (shield > 0)
                {
                    GrantItemOrb orb = new GrantItemOrb();
                    orb.origin = origin;
                    orb.target = Util.FindBodyMainHurtBox(damageReport.attackerBody);
                    orb.item = SS2Content.Items.StackShieldGate.itemIndex;
                    orb.count = 1 + shield;
                    orb.effectPrefab = orbEffect;
                    OrbManager.instance.AddOrb(orb);
                }
            }
        }
    }   
}
