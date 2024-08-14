using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine;

using MSU;
using RoR2.ContentManagement;
using System.Collections;
using HG;
using MSU.Config;

namespace SS2.Items
{
#if DEBUG
    public sealed class HottestSauce : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_HOTTESTSAUCE_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acHottestSauce", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Radius in which the hottest sauce deals damage, in meters.")]
        [FormatToken(token, 0)]
        public static float radius = 30f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Duration of the burn effect, in seconds.")]
        [FormatToken(token, 1)]
        public static float DOTDuration = 6f;

        private GameObject sauceProjectile;

        public override void Initialize()
        {
            sauceProjectile = AssetCollection.FindAsset<GameObject>("SauceProjectile");
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.HottestSauce;
            //ToDo: Implement hot sauce pool
            private static GameObject hotSauce;
            public void Start()
            {
                EquipmentSlot.onServerEquipmentActivated += FireHotSauce;
            }

            private void FireHotSauce(EquipmentSlot slot, EquipmentIndex index)
            {
                if (slot.characterBody != body)
                    return;

                //N: got removed from MSU 2.0, lol
                //MSUtil.PlayNetworkedSFX("HottestSauce", gameObject.transform.position);

                var hits = new List<HurtBox>();
                SphereSearch sauceSearch = new SphereSearch();
                sauceSearch.ClearCandidates();
                sauceSearch.origin = body.corePosition;
                sauceSearch.mask = LayerIndex.entityPrecise.mask;
                sauceSearch.radius = radius;
                sauceSearch.RefreshCandidates();
                sauceSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(body.teamComponent.teamIndex));
                sauceSearch.FilterCandidatesByDistinctHurtBoxEntities();
                sauceSearch.GetHurtBoxes(hits);

                foreach (HurtBox hit in hits)
                {
                    if (hit.healthComponent && hit.healthComponent != body.healthComponent)
                    {
                        DamageInfo di = new DamageInfo()
                        {
                            attacker = body.gameObject,
                            position = hit.healthComponent.transform.position,
                            crit = body.RollCrit(),
                            damage = body.damage * 2,
                            procChainMask = default(ProcChainMask)
                        };
                        hit.healthComponent.TakeDamage(di);
                        var dotInfo = new InflictDotInfo()
                        {
                            attackerObject = body.gameObject,
                            victimObject = hit.healthComponent.gameObject,
                            dotIndex = DotController.DotIndex.Burn,
                            duration = DOTDuration,
                            damageMultiplier = 0.5f + stack
                        };
                        DotController.InflictDot(ref dotInfo);
                    }
                }

            }
            public void OnDestroy()
            {
                EquipmentSlot.onServerEquipmentActivated -= FireHotSauce;
            }

        }
    }
#endif
}
