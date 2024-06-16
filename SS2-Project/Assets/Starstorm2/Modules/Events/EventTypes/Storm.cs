using MSU;
using RoR2;
using RoR2.ContentManagement;
using R2API;
using System.Collections;
namespace SS2
{
    public class Storm// : SS2Event
    {
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
                args.armorAdd += 5f + 2.5f * (stack - 1);
                //args.damageMultAdd += 0.1f * stack; // i dont like damage
                args.attackSpeedMultAdd += 0.08f * (stack - 1);
                args.moveSpeedMultAdd += 0.1f * stack;
                args.cooldownMultAdd += 0.1f * stack;
            }

        }
    }
}
