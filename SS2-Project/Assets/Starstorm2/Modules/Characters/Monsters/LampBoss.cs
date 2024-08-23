using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using UnityEngine;
using static RoR2.ExplicitPickupDropTable;
namespace SS2.Monsters
{
    public sealed class LampBoss : SS2Monster
    {
        public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acLampBoss", SS2Bundle.Monsters);

        private static BodyIndex BodyIndex;
        public override void Initialize()
        {
            //N: i hate that people want to disable the boss's item but keep the boss, it doesnt make any fucking sense imo. Either that or i'm just schizo. Either way item disabling the way prod did it is not really feasable i think rn so i'll just comment this out
            
            /*var lampItem = SS2Assets.LoadAsset<ItemDef>("ShackledLamp", SS2Bundle.Items);
            if (lampItem.tier != ItemTier.Boss) // item is disabled
            {
                DeathRewards deathRewards = CharacterPrefab.GetComponent<DeathRewards>();
                ExplicitPickupDropTable dt = ScriptableObject.CreateInstance<ExplicitPickupDropTable>();
                dt.pickupEntries = new ExplicitPickupDropTable.PickupDefEntry[]
                {
                    new ExplicitPickupDropTable.PickupDefEntry {pickupDef = RoR2Content.Items.Pearl, pickupWeight = 1f},
                };
                deathRewards.bossDropTable = dt;
            }*/

            //N: IDK how you'd do this with the monster card providers, but i aint bothering.
            /*DateTime currentDate = DateTime.Now;

            double daysSinceLastFullMoon = (currentDate - new DateTime(2023, 12, 25)).TotalDays;

            //:mariosit:
            double moonPhase = daysSinceLastFullMoon % 29.53;

            double fullMoonThreshold = 1.0;

            if (moonPhase < fullMoonThreshold || moonPhase > (29.53 - fullMoonThreshold))
            {
                MonsterDirectorCards.Add(moonCard);
                Debug.Log("A brilliant light shines above. - " + moonPhase);
            }
            else
            {
                MonsterDirectorCards.Add(defaultCard);
                Debug.Log("You stand alone in the dark. - " + moonPhase);
            }*/

            RoR2Application.onLoad += () => BodyIndex = BodyCatalog.FindBodyIndex("LampBossBody");
            R2API.RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
        }

        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(SS2Content.Buffs.bdLampBuff) && sender.bodyIndex != BodyIndex)
            {
                args.primaryCooldownMultAdd += 0.5f;
                args.secondaryCooldownMultAdd += 0.25f;
                args.damageMultAdd += 0.2f;
                args.moveSpeedMultAdd += 1.5f;
            }
        }


        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        [Serializable]
        public class MoonPhaseData
        {
            public PhaseData[] phasedata;
        }

        [Serializable]
        public class PhaseData
        {
            public string phase;
        }
    }
}
