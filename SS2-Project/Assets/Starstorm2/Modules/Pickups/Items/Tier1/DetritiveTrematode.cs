using SS2.Buffs;
using RoR2;
using RoR2.Items;
using RoR2.Orbs;
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

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acDetritiveTrematode", SS2Bundle.Items);

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
        public override void Initialize()
        {
            _biteEffect = AssetCollection.FindAsset<GameObject>("TrematodeBiteEffect");
            BuffDef buffTrematodes = AssetCollection.FindAsset<BuffDef>("BuffTrematodes");
            _trematodeDotIndex = DotAPI.RegisterDotDef(.5f, .5f, DamageColorIndex.Item, buffTrematodes);
            BuffOverlays.AddBuffOverlay(buffTrematodes, AssetCollection.FindAsset<Material>("matBloodOverlay"));
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
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

                bool hasDot = victim.body.HasBuff(SS2Content.Buffs.BuffTrematodes);
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
                if (victim.combinedHealthFraction < requiredHealthPercentage && !hasDot && (victim.gameObject != attacker))
                {

                    // do the first "tick" instantly
                    ExtraDamageInstanceOrb orb = new ExtraDamageInstanceOrb();
                    orb.origin = victim.body.mainHurtBox.transform.position;
                    orb.target = victim.body.mainHurtBox;
                    orb.damageValue = trematodeDamage * base.body.damage * 0.5f; // 2 ticks per second means divide by 2
                    orb.attacker = base.gameObject;
                    OrbManager.instance.AddOrb(orb);

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
        // dumb (but probably correct) way of adding an extra instance of damage from within HealthComponent.TakeDamage
        // simply calling TakeDamage again can make enemies die twice
        private class ExtraDamageInstanceOrb : Orb
        {
            public override void Begin()
            {
                base.duration = 0;
            }
            public override void OnArrival()
            {
                if (this.target)
                {
                    HealthComponent healthComponent = this.target.healthComponent;
                    if (healthComponent)
                    {
                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.damage = this.damageValue;
                        damageInfo.attacker = this.attacker;
                        damageInfo.inflictor = null;
                        damageInfo.force = Vector3.zero;
                        damageInfo.crit = false;
                        damageInfo.procChainMask = default(ProcChainMask);
                        damageInfo.procCoefficient = 0f;
                        damageInfo.position = this.target.transform.position;
                        damageInfo.damageColorIndex = DamageColorIndex.Item;
                        damageInfo.damageType = DamageType.DoT;
                        healthComponent.TakeDamage(damageInfo);

                        //purposefully not doing this. keeping it here to remind u not to do this.
                        //GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                        //GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
                    }
                }
            }
            public float damageValue;
            public GameObject attacker;
            public TeamIndex teamIndex;
        }
    }
}
