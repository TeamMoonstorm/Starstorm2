using MSU;
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
    public sealed class BloodTester : SS2Item
    {
        private const string token = "SS2_ITEM_BLOODTESTER_DESC";

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acBloodTester", SS2Bundle.Items);

        
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of gold required to proc.")]
        public static float goldRequirement = 25f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Flat amount of health restored on proc.")]
        [FormatToken(token, 0)]
        public static float healthRegen = 25f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Percentage of health total restored on proc, per stack.")]
        [FormatToken(token, 1)]
        public static float percentHealthRegen = 8f;

        public static float buffDuration = 2f;
        public static GameObject effectPrefab;

        public override void Initialize()
        {
            effectPrefab = SS2Assets.LoadAsset<GameObject>("BloodTesterProc", SS2Bundle.Items);
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.BloodTester;

            private float storedGold;

            private void OnEnable()
            {
                if (NetworkServer.active && body.master)
                {
                    body.master.OnGoldCollected += OnGoldCollected;
                }
            }
            private void OnDisable()
            {
                if (body.master)
                {
                    body.master.OnGoldCollected -= OnGoldCollected;
                }
            }

            private void OnGoldCollected(float gold)
            {
                storedGold += gold;

                float scaledGoldRequirement = Run.instance.GetDifficultyScaledCost((int)goldRequirement);
                if (storedGold >= scaledGoldRequirement)
                {
                    storedGold = 0;
                    body.AddTimedBuff(SS2Content.Buffs.BuffBloodTesterRegen, buffDuration);
                    if (effectPrefab)
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = body.corePosition,
                            scale = Mathf.Max(1f, body.radius),
                        };
                        if (body.mainHurtBox)
                            effectData.SetHurtBoxReference(body.mainHurtBox);
                        else
                            effectData.SetNetworkedObjectReference(body.gameObject);

                        EffectManager.SpawnEffect(effectPrefab, effectData, true);
                    }
                }
            }
        }

        public sealed class BuffBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffBloodTesterRegen;
            private static float healInterval = 0.2f;
            private float healStopwatch;

            private void OnEnable()
            {

            }
            private void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    healStopwatch -= Time.fixedDeltaTime;
                    if(healStopwatch <= 0f)
                    {
                        healStopwatch += healInterval;
                        float totalHealing = healthRegen + characterBody.healthComponent.fullHealth * percentHealthRegen * 0.01f;
                        totalHealing *= buffCount;
                        float healPerTick = totalHealing / buffDuration * healInterval;
                        characterBody.healthComponent.Heal(healPerTick, default(ProcChainMask));
                    }
                }
            }
            private void OnDisable()
            {
                if(NetworkServer.active)
                {
                    float fractionRemaining = healStopwatch / healInterval;
                    float totalHealing = healthRegen + characterBody.healthComponent.fullHealth * percentHealthRegen * 0.01f;
                    float healPerTick = totalHealing / buffDuration * healInterval;
                    characterBody.healthComponent.Heal(healPerTick * fractionRemaining, default(ProcChainMask));
                }              
            }

        }
    }
}
