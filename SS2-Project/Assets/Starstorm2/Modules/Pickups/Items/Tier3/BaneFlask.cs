using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS2.Items
{
    public sealed class BaneFlask : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_BANEFLASK_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acBaneFlask", SS2Bundle.Items);

        private BuffDef _baneBuffDef;
        public static DotController.DotIndex BaneDotIndex { get; private set; }

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Debuff Damage per Second. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float debuffDamage = .3f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Duration of applied Bane debuff. (1 = 1 second)")]
        [FormatToken(token, 1)]
        public static float debuffDuration = 5;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Range of on-death AOE. For reference, Gasoline's base range is 12m. (1 = 1m)")]
        [FormatToken(token, 2)]
        public static float aoeRange = 15;

        public static GameObject explosionGross;
        public static GameObject particleBase;
        public static GameObject floorGloop;

        public static Dictionary<BuffIndex, GameObject> buffColors = new Dictionary<BuffIndex, GameObject>();

        public override void Initialize()
        {
            _baneBuffDef = AssetCollection.FindAsset<BuffDef>("BuffBane");

            // idk which is which
            explosionGross = AssetCollection.FindAsset<GameObject>("BaneGraysparkVFX");
            particleBase = AssetCollection.FindAsset<GameObject>("BaneGrayVFX");
            floorGloop = AssetCollection.FindAsset<GameObject>("BaneGrayGoop");

            BaneDotIndex = DotAPI.RegisterDotDef(1f, .15f, DamageColorIndex.Poison, _baneBuffDef);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.BaneFlask;

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (damageReport.victimBody.GetBuffCount(SS2Content.Buffs.BuffBane) < 1)
                {
                    var dotInfo = new InflictDotInfo() //add bane
                    {
                        attackerObject = body.gameObject,
                        victimObject = damageReport.victimBody.gameObject,
                        dotIndex = BaneFlask.BaneDotIndex,
                        duration = debuffDuration,
                        damageMultiplier = stack
                    };
                    DotController.InflictDot(ref dotInfo);
                }

            }

            public void OnKilledOtherServer(DamageReport damageReport)
            {
                if (!damageReport.attacker || !damageReport.attackerBody || !damageReport.victim || !damageReport.victimBody)
                {
                    return; //dont think this is needed because of the interface but cant hurt right
                }

                CharacterBody victimBody = damageReport.victimBody;
                CharacterBody attackerBody = damageReport.attackerBody;

                //notes: 
                //timed buffs have the same index as regular buffs, so i can just check the timed list and then skip it in normal buffs
                //dots seem to behave as two things - a dot and a buffdef

                if (attackerBody.inventory)
                {
                    var baneCount = attackerBody.inventory.GetItemCount(SS2Content.Items.BaneFlask);
                    if (baneCount > 0)
                    {
                        List<CharacterBody.TimedBuff> timeddebuffs = victimBody.timedBuffs;
                        Dictionary<BuffIndex, CharacterBody.TimedBuff> timedBuffs = new Dictionary<BuffIndex, CharacterBody.TimedBuff>();

                        foreach (CharacterBody.TimedBuff buff in timeddebuffs)
                        {
                            bool real = timedBuffs.TryGetValue(buff.buffIndex, out var current);
                            if (!real && BuffCatalog.GetBuffDef(buff.buffIndex).isDebuff)
                            {
                                timedBuffs.Add(buff.buffIndex, buff);
                            }
                            else if (BuffCatalog.GetBuffDef(buff.buffIndex).isDebuff)
                            {
                                if (current.timer < buff.timer)
                                {
                                    timedBuffs.Remove(buff.buffIndex);
                                    timedBuffs.Add(buff.buffIndex, buff);
                                }
                            }
                        }

                        DotController dotController = DotController.FindDotController(damageReport.victim.gameObject);
                        Dictionary<BuffIndex, DotController.DotStack> uniqueDots = new Dictionary<BuffIndex, DotController.DotStack>();

                        if (dotController)
                        {
                            List<DotController.DotStack> dotList = dotController.dotStackList;
                            foreach (DotController.DotStack dot in dotList)
                            {
                                bool real = uniqueDots.TryGetValue(dot.dotDef.associatedBuff.buffIndex, out var current);
                                if (!real)
                                {
                                    uniqueDots.Add(dot.dotDef.associatedBuff.buffIndex, dot);
                                }
                                else
                                {
                                    if (current.timer < dot.timer)
                                    {
                                        uniqueDots.Remove(dot.dotDef.associatedBuff.buffIndex);
                                        uniqueDots.Add(dot.dotDef.associatedBuff.buffIndex, dot);
                                    }
                                }
                            }
                        }

                        BuffIndex[] buffList = victimBody.activeBuffsList;
                        List<BuffIndex> infDebuffs = new List<BuffIndex>();

                        foreach (BuffIndex ind in buffList)
                        {
                            if (victimBody.HasBuff(ind))
                            {
                                var relevantDef = BuffCatalog.GetBuffDef(ind);
                                bool real1 = uniqueDots.TryGetValue(ind, out _);
                                bool real2 = timedBuffs.TryGetValue(ind, out _);
                                if (relevantDef.isDebuff && !real1 && !real2)
                                {
                                    infDebuffs.Add(ind);
                                }
                            }
                        }

                        float stackRadius = aoeRange;
                        float victimRadius = victimBody.radius;
                        float effectiveRadius = stackRadius + victimRadius;

                        var attackerTeamIndex = attackerBody.teamComponent.teamIndex;


                        Vector3 corePosition = victimBody.corePosition;

                        SphereSearch baneAOESphereSearch = new SphereSearch();
                        List<HurtBox> baneAOEHurtBoxBuffer = new List<HurtBox>();

                        baneAOESphereSearch.origin = corePosition;
                        baneAOESphereSearch.mask = LayerIndex.entityPrecise.mask;
                        baneAOESphereSearch.radius = effectiveRadius;
                        baneAOESphereSearch.RefreshCandidates();
                        baneAOESphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(attackerTeamIndex));
                        baneAOESphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                        baneAOESphereSearch.OrderCandidatesByDistance();
                        baneAOESphereSearch.GetHurtBoxes(baneAOEHurtBoxBuffer);
                        baneAOESphereSearch.ClearCandidates();

                        for (int i = 0; i < baneAOEHurtBoxBuffer.Count; i++)
                        {
                            HurtBox hurtBox = baneAOEHurtBoxBuffer[i];
                            if (hurtBox.healthComponent)
                            {

                                foreach (var dot in uniqueDots) //add all dots
                                {
                                    var dotInfoTemp = new InflictDotInfo()
                                    {
                                        attackerObject = body.gameObject,
                                        victimObject = hurtBox.healthComponent.gameObject,
                                        dotIndex = dot.Value.dotIndex,
                                        duration = dot.Value.timer,
                                        damageMultiplier = dot.Value.damage
                                    };
                                    DotController.InflictDot(ref dotInfoTemp);

                                    EffectData effectDataTemp = new EffectData
                                    {
                                        origin = victimBody.transform.position,
                                        color = dot.Value.dotDef.associatedBuff.buffColor,
                                        scale = 1
                                    };
                                    EffectManager.SpawnEffect(particleBase, effectDataTemp, transmit: true);
                                }

                                foreach (var timed in timedBuffs) //add all timed debuffs
                                {
                                    hurtBox.healthComponent.body.AddTimedBuffAuthority(timed.Key, timed.Value.timer);

                                    EffectData effectDataTemp = new EffectData
                                    {
                                        origin = victimBody.transform.position,
                                        color = BuffCatalog.GetBuffDef(timed.Value.buffIndex).buffColor,
                                        scale = 1
                                    };
                                    EffectManager.SpawnEffect(particleBase, effectDataTemp, transmit: true);

                                }

                                foreach (var inf in infDebuffs) //add all infinite debuffs
                                {
                                    hurtBox.healthComponent.body.AddBuff(inf);

                                    EffectData effectDataTemp = new EffectData
                                    {
                                        origin = victimBody.transform.position,
                                        color = BuffCatalog.GetBuffDef(inf).buffColor,
                                        scale = 1
                                    };
                                    EffectManager.SpawnEffect(particleBase, effectDataTemp, transmit: true);
                                }
                            }
                        }
                        baneAOEHurtBoxBuffer.Clear();

                        EffectData effectData = new EffectData
                        {
                            origin = victimBody.transform.position,
                            scale = 1
                        };
                        EffectManager.SpawnEffect(explosionGross, effectData, transmit: true);
                        if (victimBody.characterMotor.isGrounded)
                        {
                            EffectData efd2 = new EffectData()
                            {
                                origin = victimBody.transform.position,
                                scale = 1
                            };
                            EffectManager.SpawnEffect(floorGloop, efd2, transmit: true);
                        }
                    }
                }

            }
        }
    }
}