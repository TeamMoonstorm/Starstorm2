using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Items
{
    public sealed class StrangeCan : SS2Item
    {
        private const string token = "SS2_ITEM_STRANGECAN_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("StrangeCan", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.ID_ITEM, ConfigDesc = "Chance for Intoxicate to be applied on hit. (1 = 1%)")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float procChance = 10;

        [RooConfigurableField(SS2Config.ID_ITEM, ConfigDesc = "Chance for Intoxicate to be applied on hit, per stack of this item. (1 = 1%")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float procChancePerStack = 5;

        [RooConfigurableField(SS2Config.ID_ITEM, ConfigDesc = "Damage per second, per stack of Intoxicate. (1 = 1%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 2, "100")]
        public static float damageCoefficient = .5f;

        [RooConfigurableField(SS2Config.ID_ITEM, ConfigDesc = "Health restored when killing intoxicated enemies, per stack of Intoxicate.")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float healAmount = 10;

        [RooConfigurableField(SS2Config.ID_ITEM, ConfigDesc = "Health restored when killing intoxicated enemies, per stack of Intoxicate, per stack of this item.")]
        [TokenModifier(token, StatTypes.Default, 4)]
        public static float healAmountPerStack = 5;

        public static int maxStacks = 10;

        public static float buffDuration = 10f;

        public static GameObject procEffect = SS2Assets.LoadAsset<GameObject>("StrangeCanEffect", SS2Bundle.Items);

        public override void Initialize()
        {
            base.Initialize();
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
        }

        //theres no way this is correct
        private void OnCharacterDeathGlobal(DamageReport report)
        {
            if (!NetworkServer.active) return;

            int buffCount = report.victimBody.GetBuffCount(SS2Content.Buffs.BuffIntoxicated);
            if (buffCount <= 0) return;

            //for each stack of Intoxicated, store the inflictor and the number of stacks they inflicted
            DotController dotController = DotController.FindDotController(report.victim.gameObject);
            Dictionary<GameObject, int> inflictors = new Dictionary<GameObject, int>();
            for(int i = 0; i < dotController.dotStackList.Count; i++)
            {
                DotController.DotStack dot = dotController.dotStackList[i];
                if(dot.dotIndex == Buffs.Intoxicated.index)
                {
                    GameObject inflictor = dot.attackerObject;

                    if(inflictor && !inflictors.TryGetValue(inflictor, out int _)) //theres no way a dictionary is correct here
                    {
                        inflictors.Add(inflictor, 0);
                    }
                    inflictors[inflictor]++;
                }
            }
            //heal each inflictor based on the number of stacks they inflicted
            foreach(KeyValuePair<GameObject, int> inflictor in inflictors)
            {
                CharacterBody body = inflictor.Key.GetComponent<CharacterBody>();
                if(body)
                {
                    int itemStack = body.inventory ? body.inventory.GetItemCount(ItemDef) : 0;
                    float healPerDotStack = healAmount + healAmountPerStack * (itemStack - 1);
                    HealOrb healOrb = new HealOrb();
                    healOrb.origin = report.victimBody.corePosition;
                    healOrb.target = body.mainHurtBox;
                    healOrb.healValue = healPerDotStack * inflictor.Value;
                    healOrb.overrideDuration = 1f;
                    OrbManager.instance.AddOrb(healOrb);
                }
            }
        }

        private void OnServerDamageDealt(DamageReport report)
        {
            //sound is on the buffdef
            //Util.PlaySound("StrangeCan", report.victim.gameObject);
            CharacterBody body = report.attackerBody;
            if (!body || (body && !body.inventory)) return;

            int stack = body.inventory.GetItemCount(ItemDef);
            if (stack <= 0) return;

            float procChance = StrangeCan.procChance + procChancePerStack * (stack - 1);
            if (Util.CheckRoll(procChance * report.damageInfo.procCoefficient, body.master))
            {
                var dotInfo = new InflictDotInfo()
                {
                    attackerObject = body.gameObject,
                    victimObject = report.victim.gameObject,
                    dotIndex = Buffs.Intoxicated.index,
                    duration = buffDuration,
                    maxStacksFromAttacker = (uint)maxStacks,
                    damageMultiplier = damageCoefficient,
                };
                DotController.InflictDot(ref dotInfo);

                EffectManager.SimpleEffect(procEffect, report.damageInfo.position, Quaternion.identity, true);
            }
        }
    }
}
