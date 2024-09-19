﻿using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Items
{
    public sealed class UniversalCharger : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_UNIVERSALCHARGER_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acUniversalCharger", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Time it takes for Universal Charger to recharge, in seconds.")]
        [FormatToken(token, 0)]
        public static float baseCooldown = 18f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "How much faster Universal Charger recharges, per stack. (1 = 100%)")]
        [FormatToken(token, 1)]
        public static float cooldownReductionPerStack = 10f; // percent

        public static GameObject overlayPanel;

        public static GameObject procEffect;

        private BuffDef _universalChargerBuff; //{ get; } = SS2Assets.LoadAsset<BuffDef>("BuffUniversalCharger", SS2Bundle.Items);
        public override void Initialize()
        {
            procEffect = AssetCollection.FindAsset<GameObject>("UniversalChargerEffect");
            overlayPanel = AssetCollection.FindAsset<GameObject>("RefreshPanel");
            On.RoR2.UI.HUD.Awake += AddIcons;
        }

        private void AddIcons(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);

            Transform scaler = self.transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomRightCluster/Scaler");
            Transform skill1 = scaler.Find("Skill1Root");
            if (skill1) GameObject.Instantiate(overlayPanel, skill1);
            Transform skill2 = scaler.Find("Skill2Root");
            if (skill2) GameObject.Instantiate(overlayPanel, skill2);
            Transform skill3 = scaler.Find("Skill3Root");
            if (skill3) GameObject.Instantiate(overlayPanel, skill3);
            Transform skill4 = scaler.Find("Skill4Root");
            if (skill4) GameObject.Instantiate(overlayPanel, skill4);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.UniversalCharger;

            private float cooldownTimer;
            private void OnEnable()
            {
                body.onSkillActivatedServer += RemoveBuff;
                body.onSkillActivatedAuthority += TryRefresh;
            }
            private void OnDisable()
            {
                body.onSkillActivatedServer -= RemoveBuff;
                body.onSkillActivatedAuthority -= TryRefresh;
                if (body.HasBuff(SS2Content.Buffs.BuffUniversalCharger))
                    body.RemoveBuff(SS2Content.Buffs.BuffUniversalCharger);
            }

            private void RemoveBuff(GenericSkill genericSkill)
            {
                // lazy, giga jank. but server needs to see it for skill icon overlay to work
                // ^ shouod just be some network message but idc
                if (CanSkillRefresh(genericSkill) && body.HasBuff(SS2Content.Buffs.BuffUniversalCharger))
                {
                    body.RemoveBuff(SS2Content.Buffs.BuffUniversalCharger);
                    float cooldownReduction = Util.ConvertAmplificationPercentageIntoReductionPercentage(cooldownReductionPerStack * (stack - 1)) / 100f;
                    this.cooldownTimer = baseCooldown * (1 - cooldownReduction); // XDDDDDDDDDDDDDDDDD fuck im stupid
                }
            }

            private void TryRefresh(GenericSkill genericSkill)
            {
                if (CanSkillRefresh(genericSkill))
                {
                    genericSkill.ApplyAmmoPack();
                    EffectData effectData = new EffectData
                    {
                        origin = this.body.corePosition,
                    };
                    effectData.SetNetworkedObjectReference(this.body.gameObject);
                    EffectManager.SpawnEffect(procEffect, effectData, true);
                    float cooldownReduction = Util.ConvertAmplificationPercentageIntoReductionPercentage(cooldownReductionPerStack * (stack - 1)) / 100f;
                    this.cooldownTimer = baseCooldown * (1 - cooldownReduction);
                }
            }

            private void FixedUpdate()
            {
                this.cooldownTimer -= Time.fixedDeltaTime;
                // buff is just for server jank
                if (!NetworkServer.active) return;
                if (!body.HasBuff(SS2Content.Buffs.BuffUniversalCharger) && this.cooldownTimer <= 0)
                {
                    body.AddBuff(SS2Content.Buffs.BuffUniversalCharger);
                }
            }
            //dont want to consume it on skills with no cooldown. or on primaries because loader primary has a cooldown XDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD
            private bool CanSkillRefresh(GenericSkill skill)
            {
                return skill && skill.baseRechargeInterval > 0 && skill.skillDef.stockToConsume > 0 && skill.characterBody.skillLocator.primary != skill && this.cooldownTimer <= 0;
            }


        }
    }
}
