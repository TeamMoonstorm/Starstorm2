using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]
    public sealed class BaneFlask : ItemBase
    {

        private const string token = "SS2_ITEM_BANEFLASK_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("BaneFlask", SS2Bundle.Items);
        public static DotController.DotIndex DotIndex;
        //public static float duration = 2;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Debuff Damage per Second. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float debuffDamage = .3f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Duration of applied Bane debuff. (1 = 1 second)")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float debuffDuration = 5;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Range of on-death AOE. For reference, Gasoline's base range is 12m. (1 = 1m)")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float aoeRange = 15;

        public static GameObject explosionGross;
        public static GameObject particleBase;
        public static GameObject floorGloop;

        public static Dictionary<BuffIndex, GameObject> buffColors = new Dictionary<BuffIndex, GameObject>();


        //[ConfigurableField(ConfigDesc = "Max active warbanners for each character.")]
        //[TokenModifier(token, StatTypes.Default, 3)]
        //public static int maxGreaterBanners = 5;

        public override void Initialize()
        {
            explosionGross = SS2Assets.LoadAsset<GameObject>("BaneGrayVFX", SS2Bundle.Items);
            particleBase = SS2Assets.LoadAsset<GameObject>("BaneHitsparkVFX", SS2Bundle.Items);
            floorGloop = SS2Assets.LoadAsset<GameObject>("BaneGrayGoop", SS2Bundle.Items);
            //DotController.onDotInflictedServerGlobal += RefreshInsects;
        }


        public sealed class Behavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.BaneFlask;

            public void OnDamageDealtServer(DamageReport damageReport)
            { 
                if(damageReport.victimBody.GetBuffCount(SS2Content.Buffs.BuffBane) < 1)
                {
                    var dotInfo = new InflictDotInfo() //add bane
                    {
                        attackerObject = body.gameObject,
                        victimObject = damageReport.victimBody.gameObject,
                        dotIndex = Buffs.Bane.index,
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
                            //SS2Log.Info("1: timed buff: " + buff + " | " + buff.buffIndex + " | " + buff.timer); //finds active timed buffs
                            bool real = timedBuffs.TryGetValue(buff.buffIndex, out var current);
                            if (!real && BuffCatalog.GetBuffDef(buff.buffIndex).isDebuff)
                            {
                                timedBuffs.Add(buff.buffIndex, buff);
                            }
                            else if (BuffCatalog.GetBuffDef(buff.buffIndex).isDebuff)
                            {
                                if (current.timer < buff.timer)
                                {
                                    //SS2Log.Info("overriding original debuff because old duration was " + current.timer + " and new one is " + buff.timer + " which is more awesome");
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
                                //SS2Log.Info("3: dot: " + dot + " | def: " + dot.dotDef + " | ind: " + dot.dotIndex + " | buff index " + dot.dotDef.associatedBuff.buffIndex); //gets DOT stacks 
                                bool real = uniqueDots.TryGetValue(dot.dotDef.associatedBuff.buffIndex, out var current);
                                if (!real)
                                {
                                    uniqueDots.Add(dot.dotDef.associatedBuff.buffIndex, dot);
                                }
                                else
                                {
                                    if (current.timer < dot.timer)
                                    {
                                        //SS2Log.Info("overriding original dot because old duration was " + current.timer + " and new one is " + dot.timer + " which is more awesome");
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
                                //SS2Log.Info("yeah they actually have " + BuffCatalog.GetBuffDef(ind) + " | " + victimBody.GetBuffCount(BuffCatalog.GetBuffDef(ind)));
                                bool real1 = uniqueDots.TryGetValue(ind, out _);
                                bool real2 = timedBuffs.TryGetValue(ind, out _);
                                if (relevantDef.isDebuff && !real1 && !real2)
                                {
                                    //SS2Log.Info("Adding " + BuffCatalog.GetBuffDef(ind) + " because it's a perm buff");
                                    infDebuffs.Add(ind);
                                }
                            }
                        }

                        float stackRadius = aoeRange;// + (aoeRangeStacking.Value * (float)(cryoCount - 1));
                        float victimRadius = victimBody.radius;
                        float effectiveRadius = stackRadius + victimRadius;

                        var attackerTeamIndex = attackerBody.teamComponent.teamIndex;

                        //float num = 8f + 4f * (float)cryoCount;
                        //float radius = victimBody.radius;
                        //float num2 = num + radius;
                        //float num3 = 1.5f;
                        //float baseDamage = obj.attackerBody.damage * num3;
                        //float value = (float)(1 + cryoCount) * 0.75f * obj.attackerBody.damage;

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

                                //var dotInfo = new InflictDotInfo() //add bane
                                //{
                                //    attackerObject = body.gameObject,
                                //    victimObject = hurtBox.healthComponent.gameObject,
                                //    dotIndex = Buffs.Bane.index,
                                //    duration = debuffDuration,
                                //    damageMultiplier = stack
                                //};
                                //DotController.InflictDot(ref dotInfo);

                                foreach(var dot in uniqueDots) //add all dots
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
                                        scale = 1// * (float)obj.victimBody.hullClassification
                                    };
                                    EffectManager.SpawnEffect(particleBase, effectDataTemp, transmit: true);
                                }

                                foreach(var timed in timedBuffs) //add all timed debuffs
                                {
                                    hurtBox.healthComponent.body.AddTimedBuffAuthority(timed.Key, timed.Value.timer);

                                    EffectData effectDataTemp = new EffectData
                                    {
                                        origin = victimBody.transform.position,
                                        color = BuffCatalog.GetBuffDef(timed.Value.buffIndex).buffColor,                                        
                                        scale = 1// * (float)obj.victimBody.hullClassification
                                    };
                                    EffectManager.SpawnEffect(particleBase, effectDataTemp, transmit: true);

                                }

                                foreach(var inf in infDebuffs) //add all infinite debuffs
                                {
                                    hurtBox.healthComponent.body.AddBuff(inf);

                                    EffectData effectDataTemp = new EffectData
                                    {
                                        origin = victimBody.transform.position,
                                        color = BuffCatalog.GetBuffDef(inf).buffColor,
                                        scale = 1// * (float)obj.victimBody.hullClassification
                                    };
                                    EffectManager.SpawnEffect(particleBase, effectDataTemp, transmit: true);
                                }
                            }
                        }
                        baneAOEHurtBoxBuffer.Clear();

                        //EntityStates.Mage.Weapon.IceNova.impactEffectPrefab
                        EffectData effectData = new EffectData
                        {
                            origin = victimBody.transform.position,
                            //
                            scale = 1// * (float)obj.victimBody.hullClassification
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

                        //Util.PlaySound("RefabricatorAction", victimBody.gameObject);

                        //Util.PlaySound("Play_acid_larva_impact", victimBody.gameObject);
                        //EffectManager.SpawnEffect(iceDeathAOEObject, new EffectData
                        //{
                        //    origin = corePosition,
                        //    scale = effectiveRadius,
                        //    rotation = Util.QuaternionSafeLookRotation(obj.damageInfo.force)
                        //}, true);
                    }
                }

            }

            //npublic void OnKilledServer(DamageReport damageReport)
            //n{
            //n    //throw new System.NotImplementedException();
            //n}
        }
    }
}
