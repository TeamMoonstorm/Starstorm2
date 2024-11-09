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
using SS2.Components;
namespace SS2.Items
{
    //check StickyOverloaderController for rest of behavior
    public sealed class StickyOverloader : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_STICKYOVERLOADER_DESC";

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acStickyOverloader", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Chance for Sticky Overloader to be applied on hit, per stack. (1 = 1%)")]
        [FormatToken(token, 0)]
        public static float procChance = 3;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Damage of the explosion, per charge. (1 = 1%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float damageCoefficient = 1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Damage of the explosion, per charge, per stack. (1 = 1%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float damageCoefficientPerStack = 0.8f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Radius of the explosion, per charge. (1 = 1m)")]
        [FormatToken(token, 3)]
        public static float blastRadius = 0.6f;

        //[RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Radius of the explosion, per charge, per stack. (1 = 1m)")]
        //[FormatToken(token, 4)]
        //public static float blastRadiusPerStack = 0.2f;

        public static int maxStacks = 25;

        public static int maxStacksPerStack = 5;

        public static float buffDuration = 1f;

        private static GameObject _procEffect;

        private static R2API.ModdedProcType sticky;
        public override void Initialize()
        {
            _procEffect = SS2Assets.LoadAsset<GameObject>("StrangeCanEffect", SS2Bundle.Items);
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
            On.RoR2.Orbs.VineOrb.OnArrival += VineOrb_OnArrival;
            sticky = ProcTypeAPI.ReserveProcType();
        }

        // too lazy to ilhook. its beta code so i dont want to keep fixing it
        // noxious throsn spreads bomba :3
        private void VineOrb_OnArrival(On.RoR2.Orbs.VineOrb.orig_OnArrival orig, VineOrb self)
        {
            orig(self);
            foreach (VineOrb.SplitDebuffInformation s in self.splitDebuffInformation)
            {
                if(s.index == SS2Content.Buffs.BuffStickyOverloader.buffIndex)
                {
                    StickyOverloaderController.TrySpawnBomb(self.target.healthComponent.body, s.attacker.GetComponent<CharacterBody>());
                    return;
                }
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

      
        private void OnServerDamageDealt(DamageReport report)
        {
            CharacterBody body = report.attackerBody;
            if (!body || !body.inventory || report.damageInfo.procChainMask.HasModdedProc(sticky)) return;
            //if (!report.damageInfo.damageType.IsDamageSourceSkillBased) return;

            int stack = body.inventory.GetItemCount(SS2Content.Items.StickyOverloader);
            if (stack <= 0) return;
            int buffStacks = report.victimBody.GetBuffCount(SS2Content.Buffs.BuffStickyOverloader);
            float chance = buffStacks > 0 ? 100f : procChance; // 100% chance if already applied, * proc coefficient
            if (Util.CheckRoll(chance * report.damageInfo.procCoefficient, body.master))
            {
                if(buffStacks == 0) StickyOverloaderController.TrySpawnBomb(report.victimBody, report.attackerBody);
                if (buffStacks < maxStacks + maxStacksPerStack * (stack - 1))
                {
                    report.victimBody.AddBuff(SS2Content.Buffs.BuffStickyOverloader);                 
                    EffectManager.SimpleEffect(_procEffect, report.damageInfo.position, Quaternion.identity, true);
                }               
            }
        }
    }
}
