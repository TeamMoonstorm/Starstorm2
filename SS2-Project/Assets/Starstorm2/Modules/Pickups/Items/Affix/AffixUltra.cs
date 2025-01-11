using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using R2API;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using MSU;

namespace SS2.Items
{
    public sealed class AffixUltra : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acUltra", SS2Bundle.Equipments);
        private static GameObject _wardPrefab;

        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            _wardPrefab = AssetCollection.FindAsset<GameObject>("UltraWard");
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (body.HasBuff(SS2Content.Buffs.bdUltra))
            {
                args.attackSpeedReductionMultAdd += 0.25f;
            }

            if (body.HasBuff(SS2Content.Buffs.bdUltraBuff) && !body.HasBuff(SS2Content.Buffs.bdUltra))
            {
                //args.baseRegenAdd += body.maxHealth *= 0.05f;
                args.moveSpeedMultAdd += 0.2f;
                args.damageMultAdd += 0.1f;

            }
        }

        public sealed class BodyBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.AffixUltra;
            private GameObject wardInstance;

            public void Start()
            {
                if (NetworkServer.active)
                {
                    body.AddBuff(SS2Content.Buffs.bdUltra);

                    body.inventory.GiveItem(SS2Content.Items.BoostMovespeed, 50);
                    body.inventory.GiveItem(SS2Content.Items.BoostCharacterSize, 100);
                    //body.inventory.GiveItem(RoR2Content.Items.BoostHp, 100);

                    wardInstance = Instantiate(_wardPrefab);
                    wardInstance.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
                    wardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject);
                }
            }

            public void OnDestroy()
            {
                if (NetworkServer.active)
                {
                    body.RemoveBuff(SS2Content.Buffs.bdUltra);

                    body.inventory.RemoveItem(SS2Content.Items.BoostMovespeed, 50);
                    body.inventory.RemoveItem(SS2Content.Items.BoostCharacterSize, 100);
                    //body.inventory.RemoveItem(RoR2Content.Items.BoostHp, 100);

                    if (wardInstance != null)
                        Destroy(wardInstance);
                }
            }
        }
    }
}
