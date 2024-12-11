using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using RoR2.Orbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Items
{
    public sealed class StrangeCan : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_STRANGECAN_DESC";

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acStrangeCan", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Chance for Intoxicate to be applied on hit. (1 = 1%)")]
        [FormatToken(token, 0)]
        public static float procChance = 10;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Chance for Intoxicate to be applied on hit, per stack of this item. (1 = 1%")]
        [FormatToken(token, 1)]
        public static float procChancePerStack = 5;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Damage per second, per stack of Intoxicate. (1 = 1%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float damageCoefficient = .5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Health restored when killing intoxicated enemies, per stack of Intoxicate.")]
        [FormatToken(token, 3)]
        public static float healAmount = 10;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Health restored when killing intoxicated enemies, per stack of Intoxicate, per stack of this item.")]
        [FormatToken(token, 4)]
        public static float healAmountPerStack = 5;

        public static int maxStacks = 10;

        public static float buffDuration = 10f;

        private static GameObject _procEffect;


        public static DotController.DotIndex DotIndex { get; private set; }

        public override void Initialize()
        {
            _procEffect = AssetCollection.FindAsset<GameObject>("StrangeCanEffect");

            //N: This should be a behaviour, but i CBA to refactor. :sob:
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;

            DotIndex = DotAPI.RegisterDotDef(1 / 3f, 1 / 3f, DamageColorIndex.Poison, AssetCollection.FindAsset<BuffDef>("BuffIntoxicated"));
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }


        //theres no way this is correct
        //N: Yeah, cuz it aint no behaviour! >:C
        private void OnCharacterDeathGlobal(DamageReport report)
        {
            if (!NetworkServer.active) return;

            int buffCount = report.victimBody.GetBuffCount(SS2Content.Buffs.BuffIntoxicated);
            if (buffCount <= 0) return;

            //for each stack of Intoxicated, store the inflictor and the number of stacks they inflicted
            DotController dotController = DotController.FindDotController(report.victim.gameObject);
            Dictionary<GameObject, int> inflictors = new Dictionary<GameObject, int>();
            for (int i = 0; i < dotController.dotStackList.Count; i++)
            {
                DotController.DotStack dot = dotController.dotStackList[i];
                if (dot.dotIndex == DotIndex)
                {
                    GameObject inflictor = dot.attackerObject;

                    if (inflictor && !inflictors.TryGetValue(inflictor, out int _)) //theres no way a dictionary is correct here
                    {
                        inflictors.Add(inflictor, 0);
                    }
                    inflictors[inflictor]++;
                }
            }
            //heal each inflictor based on the number of stacks they inflicted
            foreach (KeyValuePair<GameObject, int> inflictor in inflictors)
            {
                CharacterBody body = inflictor.Key.GetComponent<CharacterBody>();
                if (body)
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
            if (!body || !body.inventory) return;

            int stack = body.inventory.GetItemCount(ItemDef);
            if (stack <= 0) return;

            float procChance = StrangeCan.procChance + procChancePerStack * (stack - 1);
            if (Util.CheckRoll(procChance * report.damageInfo.procCoefficient, body.master))
            {
                var dotInfo = new InflictDotInfo()
                {
                    attackerObject = body.gameObject,
                    victimObject = report.victim.gameObject,
                    dotIndex = DotIndex,
                    duration = buffDuration,
                    maxStacksFromAttacker = (uint)maxStacks,
                    damageMultiplier = damageCoefficient,
                };
                DotController.InflictDot(ref dotInfo);
                
                EffectManager.SimpleEffect(_procEffect, report.damageInfo.position, Quaternion.identity, true);
            }
        }
    }
}
