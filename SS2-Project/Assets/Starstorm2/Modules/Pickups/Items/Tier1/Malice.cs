using RoR2;
using RoR2.Items;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    public sealed class Malice : ItemBase
    {
        private const string token = "SS2_ITEM_MALICE_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Malice");

        [ConfigurableField(ConfigDesc = "Radius of Malice, in meters")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float radiusBase = 13f;

        /*[ConfigurableField(ConfigDesc = "Bonus radius of malice per stack, in meters")]
         * [TokenModifier(token, StatTypes.Default, 3)]
        public static float radiusStack = 1f;*/

        [ConfigurableField(ConfigDesc = "Total damage each malice bounce carries over. (1 = 100%)")]
        [TokenModifier(token, StatTypes.Percentage, 2)]
        public static float damageCoeff = 0.30f;

        /*[ConfigurableField(ConfigDesc = "Total damage each malice bounce after the first carries over (1 = 100%)")]
         * [TokenModifier(token, StatTypes.Percentage, 4)]
        public static float scaleCoeff = 0.55f;*/

        [ConfigurableField(ConfigDesc = "Number of stacks required per extra bounce, after the first")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float bounceStack = 1;

        public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {

            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Malice;
            public void OnDamageDealtServer(DamageReport report)
            {
                if (report.damageInfo.procCoefficient > 0)      //theoretically the bounces should never happen since they're proc 0, but this works somehow
                {

                    //To-Do: Custom Orb
                    LightningOrb malOrb = new LightningOrb();
                    malOrb.bouncesRemaining = (int)Math.Truncate((stack - 1) / Mathf.Max(bounceStack, 1));

                    malOrb.range = report.victimBody.radius + radiusBase/* + (radiusStack * (stack - 1))*/;    //unsure if base enemy radius is recalculated for bounces, probably not that important

                    malOrb.damageCoefficientPerBounce = Malice./*scaleCoeff*/damageCoeff;
                    malOrb.damageValue = report.damageInfo.damage * Malice.damageCoeff;

                    malOrb.lightningType = LightningOrb.LightningType.RazorWire;            //this controls the VFX but setting it to nothing defaults to ukulele.
                                                                                            //can we add to the list of VFX in LightningOrb.LightningType?
                    malOrb.canBounceOnSameTarget = false;
                    malOrb.damageType = DamageType.Generic;
                    malOrb.isCrit = false;
                    malOrb.damageColorIndex = DamageColorIndex.Item;
                    malOrb.procCoefficient = 0f;
                    malOrb.origin = report.victimBody.corePosition;
                    malOrb.teamIndex = report.attackerTeamIndex;
                    malOrb.bouncedObjects = new List<HealthComponent>
                    {
                        report.victim
                    };
                    malOrb.teamIndex = report.attacker.GetComponent<TeamComponent>().teamIndex;
                    HurtBox hurtbox = malOrb.PickNextTarget(report.damageInfo.position);
                    if (hurtbox)
                    {
                        malOrb.target = hurtbox;
                        OrbManager.instance.AddOrb(malOrb);
                    }
                }
            }

            // OLD MALICE CODE BELOW - g

            //    public void OnDamageDealtServer(DamageReport report)
            //    {
            //        //TODO: custom proc type
            //        if (report.damageInfo.procChainMask.HasProc(maliceProcType) || report.damageInfo.damageType.HasFlag(DamageType.DoT) || report.damageInfo.damageType.HasFlag(DamageType.AOE))
            //            return;
            //
            //      EffectData maliceData = new EffectData();
            //      if (report.victim)
            //          maliceData.origin = report.victim.transform.position;
            //      else
            //          maliceData.origin = body.corePosition;
            //      maliceData.scale = radius;

            //Todo:
            //EffectManager.SpawnEffect(maliceEffect, maliceData, true);

            //      List<DamageInfo> damages = new List<DamageInfo>();
            //      DamageInfo previousDamageInfo = report.damageInfo;
            //      for (int i = 0; i < stack; i++)
            //      {
            //          damages.Add(GetDamageInfo(previousDamageInfo));
            //          previousDamageInfo = damages[i];
            //      }

            //      BlastAttack area = new BlastAttack();
            //      area.radius = radius;
            //      area.attacker = body.gameObject;
            //      area.inflictor = body.gameObject;
            //      area.teamIndex = body.teamComponent.teamIndex;
            //      area.attackerFiltering = AttackerFiltering.NeverHit;
            //      area.position = report.damageInfo.position;

            //      int hits = 0;
            //      var hitPoints = area.CollectHits()
            //                          .Distinct()
            //                          .GroupBy(hit => hit.hurtBox.healthComponent)
            //                          .Where(group => group.Key != report.victimBody.healthComponent)
            //                          .Select(group => group.OrderBy(hit => hit.distanceSqr).First())
            //                          .OrderBy(hit => hit.distanceSqr)
            //                          .ToArray();



            //      for (int i = 0; hits <= stack && i < hitPoints.Length; i++)
            //      {
            //          damages[hits].position = hitPoints[i].hitPosition;
            //          damages[hits].procChainMask.AddProc(maliceProcType);
            //          hitPoints[i].hurtBox.healthComponent.TakeDamage(damages[hits]);
            //          hits++;
            //      }
            //  }

            //  public DamageInfo GetDamageInfo(DamageInfo previousDamageInfo)
            //  {
            //      DamageInfo damageInfo = new DamageInfo();
            //      damageInfo.attacker = body.gameObject;
            //      damageInfo.crit = false;
            //      if (previousDamageInfo.crit)
            //          damageInfo.crit = body.RollCrit();
            //      damageInfo.damage = previousDamageInfo.damage * damageChainCoefficient;
            //      damageInfo.damageColorIndex = DamageColorIndex.Nearby;
            //      damageInfo.damageType = previousDamageInfo.damageType;
            //      damageInfo.dotIndex = DotController.DotIndex.None;

            //      ProcChainMask procMask = previousDamageInfo.procChainMask;
            //      Array allProcs = typeof(ProcType).GetEnumValues();
            //      for (int i = 0; i < allProcs.Length; i++)
            //      {
            //          ProcType proc = (ProcType)allProcs.GetValue(i);
            //          if (procMask.HasProc(proc) && !Util.CheckRoll(damageChainCoefficient, body.master))
            //              procMask.RemoveProc(proc);
            //      }
            //      damageInfo.procChainMask = procMask;

            //      return damageInfo;
            //  }
        }
    }
}
