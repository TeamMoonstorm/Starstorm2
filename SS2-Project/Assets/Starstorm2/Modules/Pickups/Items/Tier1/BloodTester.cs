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
    public sealed class BloodTester : SS2Item
    {
        private const string token = "SS2_ITEM_BLOODTESTER_DESC";

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acBloodTester", SS2Bundle.Items);

        
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Time, in seconds, between health regeneration boosts.")]
        [FormatToken(token, 0)]
        public static float cooldown = 30f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Amount of health restored per 25 gold, per stack.")]
        [FormatToken(token, 1)]
        public static float healthRegen = 10f;

        public static float buffDuration = 1f;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.BloodTester;
            private float stopwatch;

            private void FixedUpdate()
            {
                if(NetworkServer.active && body.master)
                {
                    stopwatch += Time.fixedDeltaTime;
                    if (stopwatch >= cooldown)
                    {
                        // NEED SOUNDS REALLY BAD
                        stopwatch -= cooldown;
                        
                        body.AddTimedBuff(SS2Content.Buffs.BuffBloodTesterRegen, buffDuration);
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
            private void FixedUpdate()
            {
                if (NetworkServer.active && CharacterBody.master)
                {
                    healStopwatch += Time.fixedDeltaTime;
                    if(healStopwatch >= healInterval)
                    {
                        healStopwatch -= healInterval;
                        float totalHealing = healthRegen * CharacterBody.master.money / Run.instance.GetDifficultyScaledCost(25, Stage.instance.entryDifficultyCoefficient);
                        float healPerTick = totalHealing / buffDuration * healInterval;
                        CharacterBody.healthComponent.Heal(healPerTick, default(ProcChainMask));
                    }
                }
            }
            private void OnDisable()
            {
                if(NetworkServer.active && CharacterBody.master)
                {
                    float fractionRemaining = healStopwatch / healInterval;
                    float totalHealing = healthRegen * CharacterBody.master.money / Run.instance.GetDifficultyScaledCost(25, Stage.instance.entryDifficultyCoefficient);
                    float healPerTick = totalHealing / buffDuration * healInterval;
                    CharacterBody.healthComponent.Heal(healPerTick * fractionRemaining, default(ProcChainMask));
                }              
            }

        }
    }
}
