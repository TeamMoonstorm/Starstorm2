using MSU;
using RoR2;
using RoR2.ContentManagement;
using R2API;
using System.Collections;
using MSU.Config;
using RiskOfOptions;
using Stage = R2API.DirectorAPI.Stage;
using System.Collections.Generic;
namespace SS2
{
    public class Storm// : SS2Event
    {
        public static ConfiguredFloat EffectIntensity = SS2Config.ConfigFactory.MakeConfiguredFloat(100, b =>
        {
            b.section = "Storms";
            b.key = "Storm Effect Intensity";
            b.description = "Changes the visual intensity of storms, from 0-100.";
            b.configFile = SS2Config.ConfigEvent;
        }).DoConfigure();

        public static List<Stage> brightStages = new List<Stage> { Stage.RallypointDelta, Stage.SiphonedForest }; // should probs just check for blizzard
        public static IEnumerator Init()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += AddStormBuff;
            ContentUtil.AddContentFromAssetCollection(SS2Content.SS2ContentPack, SS2Assets.LoadAsset<ExtendedAssetCollection>("acStorm", SS2Bundle.Events));
            yield break;
        }

        private static void AddStormBuff(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int stack = sender.GetBuffCount(SS2Content.Buffs.BuffStorm);
            if (stack > 0)
            {
                args.armorAdd += 10f + 8f * (stack - 1);
                //args.damageMultAdd += 0.1f * stack; // i dont like damage
                args.attackSpeedMultAdd += .1f + 0.08f * (stack - 1);
                args.moveSpeedMultAdd += 0.15f * stack;
                args.cooldownMultAdd += 0.15f * stack;
            }

        }
    }
}