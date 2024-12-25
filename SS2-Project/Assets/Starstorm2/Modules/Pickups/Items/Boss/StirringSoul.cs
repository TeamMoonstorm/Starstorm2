using SS2.Components;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;
using MSU;
using System.Collections;
using MSU.Config;
using RoR2.ContentManagement;
using System.Collections.Generic;

namespace SS2.Items
{
    public sealed class StirringSoul : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acStirringSoul", SS2Bundle.Items);

        private static GameObject _monsterSoulPickup;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Chance for souls to drop an item. (1 = 100%)")]
        public static float chance = 1f;

        public override void Initialize()
        {
            _monsterSoulPickup = AssetCollection.FindAsset<GameObject>("MonsterSoul");
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
        }

        private void OnCharacterDeathGlobal(DamageReport report)
        {
            if (!NetworkServer.active) return;
            if (Run.instance.isRunStopwatchPaused || !report.victimMaster) return;
            if (report.attackerMaster && report.attackerMaster.inventory.GetItemCount(SS2Content.Items.StirringSoul) > 0)
            {
                GameObject soul = GameObject.Instantiate(_monsterSoulPickup, report.victimBody.corePosition, Random.rotation);
                soul.GetComponent<TeamFilter>().teamIndex = report.attackerTeamIndex;
                SoulPickup pickup = soul.GetComponentInChildren<SoulPickup>();
                pickup.team = soul.GetComponent<TeamFilter>();
                pickup.chance = chance;
                NetworkServer.Spawn(soul);
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
