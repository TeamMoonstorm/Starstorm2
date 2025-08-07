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
