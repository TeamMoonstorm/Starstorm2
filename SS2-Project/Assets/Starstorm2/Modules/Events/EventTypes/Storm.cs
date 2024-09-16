using MSU;
using RoR2;
using RoR2.ContentManagement;
using R2API;
using System.Collections;
using MSU.Config;
using RiskOfOptions;
namespace SS2
{
    public class Storm// : SS2Event
    {
        public static ConfiguredBool ReworkedStorm = SS2Config.ConfigFactory.MakeConfiguredBool(false, b =>
        {
            b.section = "Events";
            b.key = "Enable Reworked Storms";
            b.description = "Enables Starstorm 2's unfinished Storm rework, which will be shipped fully with version 0.7.0.";
            b.configFile = SS2Config.ConfigMain;
            //b.CheckBoxConfig = new CheckBoxConfig
            //{
            //    restartRequired = true
            //};
        }).DoConfigure();

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
                if(ReworkedStorm.value)
                {
                    args.armorAdd += 5f + 2.5f * (stack - 1);
                    //args.damageMultAdd += 0.1f * stack; // i dont like damage
                    args.attackSpeedMultAdd += 0.08f * (stack - 1);
                    args.moveSpeedMultAdd += 0.15f * stack;
                    args.cooldownMultAdd += 0.1f * stack;
                }
                else
                {
                    args.armorAdd += 20f;
                    //args.critAdd += 20f;
                    args.damageMultAdd += 0.2f;
                    args.attackSpeedMultAdd += 0.5f;
                    args.moveSpeedMultAdd += 0.5f;
                    args.cooldownMultAdd += 0.2f;
                }
            }

        }
    }
}