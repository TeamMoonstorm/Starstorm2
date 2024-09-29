using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS2.Items
{
    public sealed class GreenChocolate : SS2Item
    {
        private const string token = "SS2_ITEM_GREENCHOCOLATE_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acGreenChocolate", SS2Bundle.Items);

        private static GameObject _effect;
        private static GameObject _damageBonusEffect;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Percentage of max hp that must be lost for Green Chocolate's effect to proc. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float damageThreshold = 0.2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Percent damage reduction that the damage in excess of the above threshold is reduced by. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float damageReduction = 0.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Base duration of the buff provided by Green Chocolate. (1 = 1 second)")]
        [FormatToken(token, 2)]
        public static float baseDuration = 6f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Duration of the buff gained per stack. (1 = 1 second)")]
        [FormatToken(token, 3)]
        public static float stackDuration = 3f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Percent damage increase from the buff. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 4)]
        public static float buffDamage = 0.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Crit chance increase from the buff. (1 = 1% crit chance)")]
        [FormatToken(token, 5)]
        public static float buffCrit = 20f;

        public static DamageColorIndex damageColor;
        public override void Initialize()
        {
            _effect = AssetCollection.FindAsset<GameObject>("ChocolateEffect");
            _damageBonusEffect = AssetCollection.FindAsset<GameObject>("ChocolateDamageBonusEffect");
            damageColor = R2API.ColorsAPI.RegisterDamageColor(new Color(.428f, .8f, 0f));
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
        }

        private void OnServerDamageDealt(DamageReport damageReport)
        {
            if (damageReport.attackerBody && damageReport.attackerBody.HasBuff(SS2Content.Buffs.BuffChocolate))
                EffectManager.SimpleImpactEffect(_damageBonusEffect, damageReport.damageInfo.position, Vector3.zero, true);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            
            if(sender.HasBuff(SS2Content.Buffs.BuffChocolate))
            {
                args.critAdd += buffCrit;
                args.damageMultAdd += buffDamage;
            }
            //int buffStacks = sender.GetBuffCount(SS2Content.Buffs.BuffChocolate);
            //args.critAdd += buffCrit * buffStacks;
            //args.damageMultAdd += buffDamage * buffStacks;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.GreenChocolate;
            private GameObject effectInstance;
            private PostProcessDuration ppUp;
            private PostProcessDuration ppDown;
            private bool effectEnabled;
            public void Start()
            {
                if (body.healthComponent)
                {
                    HG.ArrayUtils.ArrayAppend(ref body.healthComponent.onIncomingDamageReceivers, this);
                }
            }

            private void FixedUpdate()
            {
                if (body.HasBuff(SS2Content.Buffs.BuffChocolate))
                {
                    if (!effectInstance)
                    {
                        effectInstance = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("ChocolatePP", SS2Bundle.Items), body.coreTransform);
                        effectInstance.GetComponent<LocalCameraEffect>().targetCharacter = base.gameObject;
                        ppUp = effectInstance.transform.Find("CameraEffect/PP").GetComponent<PostProcessDuration>();
                        ppDown = effectInstance.GetComponent<PostProcessDuration>();
                        effectEnabled = true;
                    }
                    if (!effectEnabled)
                    {
                        ppUp.enabled = true;
                        ppDown.enabled = false;
                        effectEnabled = true;
                    }
                }
                else if (effectEnabled)
                {
                    ppUp.enabled = false;
                    ppDown.enabled = true;
                    effectEnabled = false;
                }
            }


            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo.damage >= body.healthComponent.fullCombinedHealth * damageThreshold)
                {
                    damageInfo.damage = damageInfo.damage * (1 - damageReduction) + (body.healthComponent.fullCombinedHealth * (damageThreshold * damageReduction));
                    body.AddTimedBuff(SS2Content.Buffs.BuffChocolate, baseDuration + (stackDuration * (stack - 1)));


                    // NO SOUND :(((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((
                    EffectData effectData = new EffectData
                    {
                        origin = this.body.corePosition,
                        scale = this.body.radius,
                    };
                    effectData.SetNetworkedObjectReference(this.body.gameObject);
                    EffectManager.SpawnEffect(_effect, effectData, true);
                }
            }
            private void OnDestroy()
            {
                if (effectInstance) Destroy(effectInstance);
                //This SHOULDNT cause any errors because nothing should be fucking with the order of things in this list... I hope.
                if (body.healthComponent)
                {
                    int i = Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this);
                    if (i > -1)
                        HG.ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onIncomingDamageReceivers, body.healthComponent.onIncomingDamageReceivers.Length, i);
                }
            }

        }
    }
}
