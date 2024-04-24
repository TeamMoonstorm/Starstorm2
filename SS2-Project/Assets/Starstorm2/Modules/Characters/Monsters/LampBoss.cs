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
        public override NullableRef<MonsterCardProvider> CardProvider => null;

        public override NullableRef<DirectorAPI.DirectorCardHolder> DissonanceCard => null;

        public override NullableRef<GameObject> MasterPrefab => _masterPrefab;
        private GameObject _masterPrefab;

        public override GameObject CharacterPrefab => _characterPrefab;
        private GameObject _characterPrefab;

        public override void Initialize()
        {
            On.RoR2.CharacterBody.AddBuff_BuffDef += CharacterBody_AddBuff_BuffDef;

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
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * GameObject - "LampBossBody" - Monsters
             * GameObject - "LampBossMaster" - Monsters
             * MonsterCardProvider - "???" - Monsters
             */
            yield break;
        }

        private static void CharacterBody_AddBuff_BuffDef(On.RoR2.CharacterBody.orig_AddBuff_BuffDef orig, CharacterBody self, BuffDef buffDef)
        {
            //wayfarer can't buff itself/other wayfarers
            if (self.bodyIndex == BodyCatalog.FindBodyIndex("LampBossBody") && buffDef == SS2Content.Buffs.bdLampBuff)
                return;
            orig(self, buffDef);
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
