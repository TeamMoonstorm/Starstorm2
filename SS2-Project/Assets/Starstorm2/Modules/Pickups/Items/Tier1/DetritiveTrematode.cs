using SS2.Buffs;
using RoR2;
using RoR2.Items;
using UnityEngine;
using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using R2API;

namespace SS2.Items
{
    public sealed class DetritiveTrematode : SS2Item
    {
        private const string token = "SS2_ITEM_DETRITIVETREMATODE_DESC";

        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigNameOverride = "Trematode Threshold", ConfigDescOverride = "Amount of missing health needed for Trematode to proc. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float missingHealthPercentage = 0.30f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigNameOverride = "Trematode Threshold Per Stack", ConfigDescOverride = "Increase in missing health threshold, per stack. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float missingHealthPercentagePerStack = 0.15f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Damage dealt by the Trematode debuff, per second. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float trematodeDamage = 1f;

        private static DotController.DotIndex _trematodeDotIndex;
        private static GameObject _biteEffect;

        public BuffDef _buffTrematodes; //SS2Assets.LoadAsset<BuffDef>("BuffTrematodes", SS2Bundle.Items);
        public Material overlay;//SS2Assets.LoadAsset<Material>("matBloodOverlay", SS2Bundle.Items);
        public override void Initialize()
        {
            _trematodeDotIndex = DotAPI.RegisterDotDef(.5f, .5f, DamageColorIndex.Item, _buffTrematodes);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "DetritiveTrematode" - Items
             * GameObject - "TrematodeBiteEffect" - Items
             * BuffDef - "BuffTrematodes" - Items
             */
            yield break;
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.DetritiveTrematode;

            private static BodyIndex fucker = BodyIndex.None;
            private static BodyIndex Fucker => fucker == BodyIndex.None ? BodyCatalog.FindBodyIndexCaseInsensitive("ArtifactShellBody") : fucker;
            public void OnDamageDealtServer(DamageReport report)
            {
                if (!report.victim.alive) return;

                var victim = report.victim;
                var attacker = report.attacker;
                bool hasDot = false;

                hasDot = victim.body.HasBuff(SS2Content.Buffs.BuffTrematodes);
                // dots apparently dont get updated instantly???? so we can apply multiple of the same dot before HasDotActive returns true. hopo game

                if (victim.body.bodyIndex == Fucker)
                    return;

                //30% + 15% per stack hyperbolically
                //1 = 30%
                //5 = 63.5%
                //10 = 83.8%
                float requiredHealthPercentage = 1 - (1 - missingHealthPercentage) * Mathf.Pow(1 - missingHealthPercentagePerStack, stack - 1);

                //25% + 10% per stack
                //1 = 30%
                //5 = 65%
                // 9 = 105%
                if (victim.combinedHealthFraction < requiredHealthPercentage && !hasDot && report.damageInfo.dotIndex != _trematodeDotIndex && (victim.gameObject != attacker))
                {

                    // do the first tick instantly
                    DamageInfo damageInfo = new DamageInfo();
                    damageInfo.attacker = base.gameObject;
                    damageInfo.crit = false;
                    damageInfo.damage = trematodeDamage * base.body.damage;
                    damageInfo.force = Vector3.zero;
                    damageInfo.inflictor = base.gameObject;
                    damageInfo.position = victim.body.corePosition;
                    damageInfo.procCoefficient = 0f;
                    damageInfo.damageColorIndex = DamageColorIndex.Item;
                    damageInfo.damageType = DamageType.DoT;
                    damageInfo.dotIndex = _trematodeDotIndex;
                    victim.TakeDamage(damageInfo);

                    var dotInfo = new InflictDotInfo()
                    {
                        attackerObject = attacker,
                        victimObject = victim.gameObject,
                        dotIndex = _trematodeDotIndex,
                        duration = Mathf.Infinity,
                        damageMultiplier = trematodeDamage * stack,
                    };
                    DotController.InflictDot(ref dotInfo);

                    EffectManager.SimpleEffect(_biteEffect, report.damageInfo.position, Quaternion.identity, true);
                }
            }
        }
    }
}
