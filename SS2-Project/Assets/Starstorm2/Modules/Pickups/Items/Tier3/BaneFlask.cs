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

        [ConfigurableField(ConfigDesc = "Debuff Damage per Second. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float debuffDamage = .3f;

        [ConfigurableField(ConfigDesc = "Duration of applied Bane debuff. (1 = 1 second)")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float debuffDuration = 2;

        [ConfigurableField(ConfigDesc = "Range of on-death AOE. (1 = 1m)")]
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

                DotController dotController = DotController.FindDotController(damageReport.victim.gameObject);
                

                BuffIndex[] buffList = victimBody.activeBuffsList;
                int[] buffList2 = victimBody.buffs;
                //int[] buffList2 = victimBody.buff;

                List<CharacterBody.TimedBuff> a = victimBody.timedBuffs;

                foreach (BuffDef def in BuffCatalog.buffDefs)
                {
                    SS2Log.Info("Testing " + def.name + " | " + def);
                    if (victimBody.HasBuff(def))
                    {
                        SS2Log.Debug("0: has " + def); //seems like on death the enemy no longer "has" these buffs, so this won't work (what deathmark does) 
                        //num14++;
                    }
                }

                foreach (CharacterBody.TimedBuff buff in a)
                {
                    SS2Log.Info("1: timed buff: " + buff + " | " + buff.buffIndex + " | " + buff.timer); //finds active timed buffs
                }

                //DotController dotControlle2r = DotController.FindDotController(damageReport.victim.gameObject);
                if (dotController)
                {
                    for (DotController.DotIndex dotIndex = DotController.DotIndex.Bleed; dotIndex < DotController.DotIndex.Count; dotIndex++)
                    {
                        SS2Log.Info("Testing DOT " + dotIndex + " | " + DotController.GetDotDef(dotIndex));
                        if (dotController.HasDotActive(dotIndex))
                        {
                            SS2Log.Info("2: dot active index: " + dotIndex); //finds active dots
                        }
                    }
                }


                if (dotController)
                {
                    List<DotController.DotStack> dotList = dotController.dotStackList;
                    foreach (DotController.DotStack dot in dotList)
                    {
                        SS2Log.Info("3: dot: " + dot + " | def: " + dot.dotDef + " | ind: " + dot.dotIndex); //gets DOT stacks 
                    }
                }


                foreach(BuffIndex ind in buffList)
                {
                    SS2Log.Info("4: buff: " + ind + " | def: " + BuffCatalog.GetBuffDef(ind));
                    if (body.HasBuff(ind))
                    {
                        SS2Log.Info("yeah they actually have " + BuffCatalog.GetBuffDef(ind));
                    }
                }

                //foreach(int ind in buffList2)
                //{
                //    SS2Log.Info("total buffs: " + BuffCatalog.buffCount);
                //    if(BuffCatalog.buffCount >= ind)
                //    {
                //        SS2Log.Info("ind: " + ind + " | " + BuffCatalog.GetBuffDef((BuffIndex)ind));
                //    }
                //    else
                //    {
                //        SS2Log.Info("ind: " + ind + " | " + "invalid index");
                //    }
                //}

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
                                var dotInfo = new InflictDotInfo()
                                {
                                    attackerObject = body.gameObject,
                                    victimObject = hurtBox.healthComponent.gameObject,
                                    dotIndex = Buffs.Bane.index,
                                    duration = damageReport.damageInfo.procCoefficient * duration,
                                    damageMultiplier = stack
                                };
                                DotController.InflictDot(ref dotInfo);
                                
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
