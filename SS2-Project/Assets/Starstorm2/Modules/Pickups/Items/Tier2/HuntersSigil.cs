using SS2.Components;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;
using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using R2API;

namespace SS2.Items
{
    public sealed class HuntersSigil : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_HUNTERSSIGIL_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acHuntersSigil", SS2Bundle.Items);

        private static GameObject _effect;
        private static GameObject _damageBonusEffect;
        private static GameObject _sigilWard;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Radius the effect is applied in.")]
        [FormatToken(token, 0)]
        public static float radius = 8f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Base amount of extra armor added.")]
        [FormatToken(token, 1)]
        public static float baseArmor = 30;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of extra armor added per stack.")]
        [FormatToken(token, 2)]
        public static float stackArmor = 15;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Base amount of extra damage added. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float baseDamage = .3f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of extra damage added per stack. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 4)]
        public static float stackDamage = .15f;

        

        public override void Initialize()
        {
            _effect = AssetCollection.FindAsset<GameObject>("SigilEffect");
            _damageBonusEffect = AssetCollection.FindAsset<GameObject>("SigilDamageBonusEffect");
            _sigilWard = AssetCollection.FindAsset<GameObject>("SigilWard");

            BuffOverlays.AddBuffOverlay(AssetCollection.FindAsset<BuffDef>("BuffSigil"), AssetCollection.FindAsset<Material>("matSigilBuffOverlay"));

            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
            On.RoR2.CharacterBody.RemoveBuff_BuffIndex += FUCK;
        }

        private void OnServerDamageDealt(DamageReport damageReport)
        {
            if (damageReport.attackerBody && damageReport.attackerBody.HasBuff(SS2Content.Buffs.BuffSigil))
                EffectManager.SimpleImpactEffect(_damageBonusEffect, damageReport.damageInfo.position, Vector3.zero, true);
        }

        // dumbass
        // remove all buff stacks when one gets removed. time d buffs r a fuck
        private void FUCK(On.RoR2.CharacterBody.orig_RemoveBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
        {
            orig(self, buffType);
            if(buffType == SS2Content.Buffs.BuffSigil.buffIndex)
            {
                self.SetBuffCount(SS2Content.Buffs.BuffSigilStack.buffIndex, 0);
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(SS2Content.Buffs.BuffSigilStack);

            if(buffCount > 0)
            {
                args.armorAdd += HuntersSigil.baseArmor + stackArmor * (buffCount-1);
                args.damageMultAdd += HuntersSigil.baseDamage + stackDamage * (buffCount-1);
            }
            
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.HuntersSigil;
            private bool sigilActive = false;
            private GameObject sigilInstance;

            public void FixedUpdate()
            {
                if (!NetworkServer.active) return;
                if (body.notMovingStopwatch > 1f && !body.HasBuff(SS2Content.Buffs.BuffSigil))
                {
                    if (!sigilActive)
                    {
                        EffectManager.SimpleEffect(_effect, body.aimOrigin, Quaternion.identity, true);
                        Vector3 position = body.corePosition;
                        //float radius = 13f;

                        if (sigilInstance != null)
                            NetworkServer.Destroy(sigilInstance);

                        sigilInstance = Object.Instantiate(_sigilWard, position, Quaternion.identity);
                        sigilInstance.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
                        sigilInstance.GetComponent<BuffWard>().radius = radius;

                        WardUtils wu = sigilInstance.GetComponent<WardUtils>();
                        wu.body = body;
                        wu.buffStacks = stack;
                        wu.stackBuff = SS2Content.Buffs.BuffSigilStack.buffIndex;
                        NetworkServer.Spawn(sigilInstance);

                        sigilActive = true;
                    }

                }
                else
                    sigilActive = false;
            }

            public void OnDestroy()
            {
                if (sigilInstance != null)
                    Destroy(sigilInstance);
            }

            public void OnDisable()
            {
                if (sigilInstance != null)
                    Destroy(sigilInstance);
            }
        }
    }
}
