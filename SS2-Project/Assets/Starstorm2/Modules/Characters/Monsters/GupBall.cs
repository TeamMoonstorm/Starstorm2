using RoR2;
using UnityEngine;
using MSU;
using MSU.Config;
using RoR2.ContentManagement;
using RiskOfOptions.OptionConfigs;
using SS2.Components;

namespace SS2.Monsters
{
    public sealed class GupBall : SS2Monster
    {
        public override SS2AssetRequest<MonsterAssetCollection> AssetRequest =>
            SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acGupBall", SS2Bundle.Indev);

        // Config values
        private static ConfiguredFloat cfgKickSpeed;
        private static ConfiguredFloat cfgMaxSpeed;
        private static ConfiguredFloat cfgVerticalKickMultiplier;
        private static ConfiguredFloat cfgUpwardBias;
        private static ConfiguredFloat cfgBaseMoveSpeed;

        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta && base.IsAvailable(contentPack);
        }

        public override void Initialize()
        {
            cfgKickSpeed = SS2Config.ConfigFactory.MakeConfiguredFloat(30f, b =>
            {
                b.section = "GupBall";
                b.key = "Kick Speed";
                b.description = "Velocity applied to the ball per hit.";
                b.configFile = SS2Config.ConfigMonster;
                b.sliderConfig = new SliderConfig { min = 5f, max = 100f };
            }).DoConfigure();

            cfgMaxSpeed = SS2Config.ConfigFactory.MakeConfiguredFloat(60f, b =>
            {
                b.section = "GupBall";
                b.key = "Max Speed";
                b.description = "Maximum velocity the ball can reach.";
                b.configFile = SS2Config.ConfigMonster;
                b.sliderConfig = new SliderConfig { min = 10f, max = 200f };
            }).DoConfigure();

            cfgVerticalKickMultiplier = SS2Config.ConfigFactory.MakeConfiguredFloat(0.6f, b =>
            {
                b.section = "GupBall";
                b.key = "Vertical Kick Multiplier";
                b.description = "Multiplier for the vertical component of kicks. Lower values = flatter kicks.";
                b.configFile = SS2Config.ConfigMonster;
                b.sliderConfig = new SliderConfig { min = 0f, max = 1f };
            }).DoConfigure();

            cfgUpwardBias = SS2Config.ConfigFactory.MakeConfiguredFloat(0.1f, b =>
            {
                b.section = "GupBall";
                b.key = "Upward Bias";
                b.description = "Upward nudge added to each kick so ground-level hits arc slightly.";
                b.configFile = SS2Config.ConfigMonster;
                b.sliderConfig = new SliderConfig { min = 0f, max = 0.5f };
            }).DoConfigure();

            cfgBaseMoveSpeed = SS2Config.ConfigFactory.MakeConfiguredFloat(7f, b =>
            {
                b.section = "GupBall";
                b.key = "Friction (Base Move Speed)";
                b.description = "Controls how quickly the ball decelerates. Higher = more friction.";
                b.configFile = SS2Config.ConfigMonster;
                b.sliderConfig = new SliderConfig { min = 0f, max = 20f };
            }).DoConfigure();

            if (CharacterPrefab == null)
            {
                SS2Log.Warning("GupBall: CharacterPrefab is null, skipping Initialize.");
                return;
            }

            // Set body flags
            if (CharacterPrefab.TryGetComponent<CharacterBody>(out var characterBody))
            {
                characterBody.baseMoveSpeed = cfgBaseMoveSpeed;
                characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            }
            else
            {
                Debug.LogError("GupBall: Missing CharacterBody on " + CharacterPrefab.name);
            }

            // Hide health bar
            if (CharacterPrefab.TryGetComponent<HealthComponent>(out var healthComponent))
            {
                healthComponent.dontShowHealthbar = true;
            }
            else
            {
                Debug.LogError("GupBall: Missing HealthComponent on " + CharacterPrefab.name);
            }

            // Add kick receiver
            var kickReceiver = CharacterPrefab.AddComponent<GupBallKickReceiver>();
            kickReceiver.kickSpeed = cfgKickSpeed;
            kickReceiver.maxSpeed = cfgMaxSpeed;
            kickReceiver.verticalKickMultiplier = cfgVerticalKickMultiplier;
            kickReceiver.upwardBias = cfgUpwardBias;
        }
    }
}
