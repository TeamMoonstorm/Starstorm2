using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{

    public sealed class BaneFlask : ItemBase
    {

        private const string token = "SS2_ITEM_BANEFLASK_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("BaneFlask", SS2Bundle.Items);
        public static DotController.DotIndex DotIndex;
        //public static float duration = 2;

        [RooConfigurableField(ConfigDesc = "Debuff Damage per Second. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float debuffDamage = .3f;

        [RooConfigurableField(ConfigDesc = "Duration of applied Bane debuff. (1 = 1 second)")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float debuffDuration = 2;

        [RooConfigurableField(ConfigDesc = "Range of on-death AOE. (1 = 1m)")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float aoeRange = 12; 

        //[ConfigurableField(ConfigDesc = "Max active warbanners for each character.")]
        //[TokenModifier(token, StatTypes.Default, 3)]
        //public static int maxGreaterBanners = 5;

        public override void Initialize()
        {
            //DotController.onDotInflictedServerGlobal += RefreshInsects;
        }
         
        //private void RefreshInsects(DotController dotController, ref InflictDotInfo inflictDotInfo)
        //{
        //    if (inflictDotInfo.dotIndex == DotIndex)
        //    {
        //        int i = 0;
        //        int count = dotController.dotStackList.Count;
        //
        //        while (i < count)
        //        {
        //            if (dotController.dotStackList[i].dotIndex == DotIndex)
        //            {
        //                dotController.dotStackList[i].timer = Mathf.Max(dotController.dotStackList[i].timer, duration);
        //            }
        //            i++;
        //        }
        //    }
        //}

        public sealed class Behavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.BaneFlask;
            //public void OnDamageDealtServer(DamageReport report)
            //{
            //    if (report.victimBody.teamComponent.teamIndex != report.attackerBody.teamComponent.teamIndex && report.damageInfo.procCoefficient > 0)
            //    {
            //        var dotInfo = new InflictDotInfo()
            //        {
            //            attackerObject = body.gameObject,
            //            victimObject = report.victim.gameObject,
            //            dotIndex = Buffs.Insecticide.index,
            //            duration = report.damageInfo.procCoefficient * debuffDuration,
            //            damageMultiplier = stack
            //        };
            //        DotController.InflictDot(ref dotInfo);
            //    }
            //}

            public void OnKilledOtherServer(DamageReport damageReport)
            {
                if (!damageReport.attacker || !damageReport.attackerBody || !damageReport.victim || !damageReport.victimBody)
                {
                    return; //dont think this is needed because of the interface but cant hurt right
                }

                CharacterBody victimBody = damageReport.victimBody;
                CharacterBody attackerBody = damageReport.attackerBody;             

                //BuffIndex[] buffList = victimBody.activeBuffsList;
                //int[] buffList2 = victimBody.buffs;
                //int[] buffList2 = victimBody.buff;

                //foreach (BuffDef def in BuffCatalog.buffDefs)
                //{
                //    SS2Log.Info("Testing " + def.name + " | " + def);
                //    if (victimBody.HasBuff(def))
                //    {
                //        SS2Log.Debug("0: has " + def); //seems like on death the enemy no longer "has" these buffs, so this won't work (what deathmark does) 
                //        //num14++;
                //    }
                //}
                //

                //
                ////DotController dotControlle2r = DotController.FindDotController(damageReport.victim.gameObject);
                //if (dotController)
                //{
                //    for (DotController.DotIndex dotIndex = DotController.DotIndex.Bleed; dotIndex < DotController.DotIndex.Count; dotIndex++)
                //    {
                //        SS2Log.Info("Testing DOT " + dotIndex + " | " + DotController.GetDotDef(dotIndex));
                //        if (dotController.HasDotActive(dotIndex))
                //        {
                //            SS2Log.Info("2: dot active index: " + dotIndex); //finds active dots
                //        }
                //    }
                //}

                //notes: 
                //timed buffs have the same index as regular buffs, so i can just check the timed list and then skip it in normal buffs
                //dots seem to behave as two things - a dot and a buffdef

                List<CharacterBody.TimedBuff> timeddebuffs = victimBody.timedBuffs;
                Dictionary<BuffIndex, CharacterBody.TimedBuff> timedBuffs = new Dictionary<BuffIndex, CharacterBody.TimedBuff>();

                foreach (CharacterBody.TimedBuff buff in timeddebuffs)
                {
                    SS2Log.Info("1: timed buff: " + buff + " | " + buff.buffIndex + " | " + buff.timer); //finds active timed buffs
                    bool real = timedBuffs.TryGetValue(buff.buffIndex, out var output);
                    if (!real && BuffCatalog.GetBuffDef(buff.buffIndex).isDebuff)
                    {
                        timedBuffs.Add(buff.buffIndex, buff);
                    }
                }

                DotController dotController = DotController.FindDotController(damageReport.victim.gameObject);
                Dictionary<BuffIndex, DotController.DotStack> uniqueDots = new Dictionary<BuffIndex, DotController.DotStack>();

                if (dotController)
                {
                    List<DotController.DotStack> dotList = dotController.dotStackList;
                    foreach (DotController.DotStack dot in dotList)
                    {
                        SS2Log.Info("3: dot: " + dot + " | def: " + dot.dotDef + " | ind: " + dot.dotIndex + " | buff index " + dot.dotDef.associatedBuff.buffIndex); //gets DOT stacks 
                        bool real = uniqueDots.TryGetValue(dot.dotDef.associatedBuff.buffIndex, out var current);
                        if (!real)
                        {
                            uniqueDots.Add(dot.dotDef.associatedBuff.buffIndex, dot);
                        }
                        else
                        {
                            if(current.timer < dot.timer)
                            {
                                SS2Log.Info("overriding original debuff because old duration was " + current.timer + " and new one is " + dot.timer + " which is more awesome");
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
                        SS2Log.Info("yeah they actually have " + BuffCatalog.GetBuffDef(ind) + " | " + victimBody.GetBuffCount(BuffCatalog.GetBuffDef(ind)));
                        bool real1 = uniqueDots.TryGetValue(ind, out _);
                        bool real2 = timedBuffs.TryGetValue(ind, out _);
                        if (relevantDef.isDebuff && !real1 && !real2)
                        {
                            //debuffDict.Add(ind, )
                            SS2Log.Info("Adding " + BuffCatalog.GetBuffDef(ind) + " because it's a perm buff");
                            infDebuffs.Add(ind);
                        }
                    }
                }

                foreach(var debuff in infDebuffs)
                {
                    SS2Log.Info("inf debuffs: " + debuff + " | " + BuffCatalog.GetBuffDef(debuff));
                }

                foreach (var dot in uniqueDots)
                {
                    SS2Log.Info("dots: " + dot.Key + " | " + dot.Value.dotDef.associatedBuff.name + " | " + dot.Value.dotDef.damageCoefficient);
                }

                foreach (var timed in timedBuffs)
                {
                    SS2Log.Info("timed: " + timed.Key + " | " + BuffCatalog.GetBuffDef(timed.Value.buffIndex).name + " | " + timed.Value.timer);
                }

                if (attackerBody.inventory)
                {
                    var baneCount = attackerBody.inventory.GetItemCount(SS2Content.Items.BaneFlask);
                    if (baneCount > 0)
                    {
                        float stackRadius = aoeRange;// + (aoeRangeStacking.Value * (float)(cryoCount - 1));
                        float victimRadius = victimBody.radius;
                        float effectiveRadius = stackRadius + victimRadius;
                        float AOEDamageMult = debuffDamage;// + (stackingDamageAOE.Value * (float)(cryoCount - 1));
                        float AOEDamage = damageReport.attackerBody.damage * AOEDamageMult;

                        float duration = debuffDuration; // + (slowDurationStacking.Value * (cryoCount - 1));

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
                            //Debug.Log("hurtbox " + hurtBox);
                            if (hurtBox.healthComponent)
                            {
                                //hurtBox.healthComponent.body.AddTimedBuffAuthority(Buffs.Bane.index, duration);
                                var dotInfo = new InflictDotInfo() //add bane
                                {
                                    attackerObject = body.gameObject,
                                    victimObject = hurtBox.healthComponent.gameObject,
                                    dotIndex = Buffs.Bane.index,
                                    duration = damageReport.damageInfo.procCoefficient * duration,
                                    damageMultiplier = stack
                                };
                                DotController.InflictDot(ref dotInfo);

                                foreach(var dot in uniqueDots) //add all dots
                                {
                                    var dotInfoTemp = new InflictDotInfo()
                                    {
                                        attackerObject = body.gameObject,
                                        victimObject = hurtBox.healthComponent.gameObject,
                                        dotIndex = dot.Value.dotIndex,
                                        duration = damageReport.damageInfo.procCoefficient * dot.Value.timer,
                                        damageMultiplier = dot.Value.damage
                                    };
                                    DotController.InflictDot(ref dotInfoTemp);
                                }

                                foreach(var timed in timedBuffs) //add all timed debuffs
                                {
                                    hurtBox.healthComponent.body.AddTimedBuffAuthority(timed.Key, timed.Value.timer);
                                }

                                foreach(var inf in infDebuffs) //add all infinite debuffs
                                {
                                    hurtBox.healthComponent.body.AddBuff(inf);
                                }
                            }
                        }
                        baneAOEHurtBoxBuffer.Clear();

                        new BlastAttack
                        {
                            radius = effectiveRadius,
                            baseDamage = AOEDamage,
                            procCoefficient = 0f,
                            crit = Util.CheckRoll(damageReport.attackerBody.crit, damageReport.attackerMaster),
                            damageColorIndex = DamageColorIndex.Item,
                            attackerFiltering = AttackerFiltering.Default,
                            falloffModel = BlastAttack.FalloffModel.None,
                            attacker = damageReport.attacker,
                            teamIndex = attackerTeamIndex,
                            position = corePosition,
                            //baseForce = 0,
                            //damageType = DamageType.AOE
                        }.Fire();

                        //EntityStates.Mage.Weapon.IceNova.impactEffectPrefab

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
