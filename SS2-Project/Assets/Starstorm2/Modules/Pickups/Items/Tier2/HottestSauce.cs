using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]

    public sealed class HottestSauce : ItemBase
    {
        private const string token = "SS2_ITEM_HOTTESTSAUCE_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("HottestSauce", SS2Bundle.Items);

        [ConfigurableField(SS2Config.IDItem, ConfigDesc = "Radius in which the hottest sauce deals damage, in meters.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float radius = 30f;

        [ConfigurableField(SS2Config.IDItem, ConfigDesc = "Duration of the burn effect, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float DOTDuration = 6f;

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

                MSUtil.PlayNetworkedSFX("HottestSauce", gameObject.transform.position);

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
                //EffectData effectData = new EffectData
                //{
                //    origin = slot.characterBody.transform.position
                //};
                //effectData.SetNetworkedObjectReference(slot.characterBody.gameObject);
                //EffectManager.SpawnEffect(CharacterBody.AssetReferences., effectData, transmit: true);
                
            }
            public void OnDestroy()
            {
                EquipmentSlot.onServerEquipmentActivated -= FireHotSauce;
            }

        }

    }
}
