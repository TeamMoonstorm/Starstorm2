using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static RoR2.ExplicitPickupDropTable;

namespace Moonstorm.Starstorm2.Monsters
{
    //[DisabledContent]
    public sealed class LampBoss : MonsterBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("LampBossBody", SS2Bundle.Monsters);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("LampBossMaster", SS2Bundle.Monsters);
        //public override MSMonsterDirectorCardHolder directorCards { get; set; } = Assets.Instance.MainAssetBundle.LoadAsset<MSMonsterDirectorCardHolder>("WayfarerCardHolder");
        //public override MSMonsterDirectorCard MonsterDirectorCard { get; } = SS2Assets.LoadAsset<MSMonsterDirectorCard>("msmdcLampBoss", SS2Bundle.Indev);

        private MSMonsterDirectorCard defaultCard = SS2Assets.LoadAsset<MSMonsterDirectorCard>("msmdcLampBoss", SS2Bundle.Monsters);
        private MSMonsterDirectorCard chainedCard = SS2Assets.LoadAsset<MSMonsterDirectorCard>("msmdcLampBossChained", SS2Bundle.Monsters);
        private MSMonsterDirectorCard moonCard = SS2Assets.LoadAsset<MSMonsterDirectorCard>("msmdcLampBossMoon", SS2Bundle.Monsters);

        internal static GameObject wayfarerBuffWardPrefab;

        public override void Initialize()
        {
            base.Initialize();
            MonsterDirectorCards.Add(chainedCard);
            

            //RoR2.SceneDirector.onPrePopulateSceneServer += die;

            var comp = BodyPrefab.GetComponent<LampDropOverride>();
            if (!comp)
            {
                var lampItem = SS2Assets.LoadAsset<ItemDef>("ShackledLamp", SS2Bundle.Items);
                if (lampItem.tier != ItemTier.Boss) // item is disabled
                {
                    //var drw = BodyPrefab.GetComponent<DeathRewards>();
                    //SS2Log.Info(drw);
                    //var fuck = (ExplicitPickupDropTable)drw.bossDropTable;
                    //
                    //PickupDefEntry pde;
                    //pde.pickupDef = RoR2Content.Items.Pearl;
                    //pde.pickupWeight = 1;
                    //
                    //SS2Log.Info("setting fuck | " + fuck.pickupEntries[0]);
                    //try
                    //{
                    //    SS2Log.Info("fuck: " + fuck.pickupEntries[0].pickupDef.name);
                    //}catch(Exception e)
                    //{
                    //    SS2Log.Info("exceptionl ol " + e);
                    //}
                    //fuck.pickupEntries[0] = pde;
                    ////SerializablePickupIndex fuck2;
                    ////fuck2.pickupName = RoR2Content.Items.Pearl.name;
                    ////drw.bossPickup = fuck2;
                    //SS2Log.Info("done");
                    ////SS2Log.Info("Disabling lamp drop");
                    ////BodyPrefab.AddComponent<LampDropOverride>();
                    //
                    //
                    ////ExplicitPickupDropTable dt = ScriptableObject.CreateInstance<ExplicitPickupDropTable>();
                    ////dt.pickupEntries = new ExplicitPickupDropTable.PickupDefEntry[]
                    ////{
                    ////    new ExplicitPickupDropTable.PickupDefEntry {pickupDef = RoR2Content.Items.Pearl, pickupWeight = 1f},
                    ////};
                    ////drw.bossDropTable = dt;
                    ///

                    DeathRewards deathRewards = BodyPrefab.GetComponent<DeathRewards>();
                    ExplicitPickupDropTable dt = ScriptableObject.CreateInstance<ExplicitPickupDropTable>();
                    dt.pickupEntries = new ExplicitPickupDropTable.PickupDefEntry[]
                    {
                        new ExplicitPickupDropTable.PickupDefEntry {pickupDef = RoR2Content.Items.Pearl, pickupWeight = 1f},
                    };
                    deathRewards.bossDropTable = dt;


                }
            }

            DateTime currentDate = DateTime.Now;

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
            }
        }

        private void die(SceneDirector obj)
        {
            //SS2Log.Info("guh");
            var comp = BodyPrefab.GetComponent<LampDropOverride>();
            if (!comp)
            {
                var lampItem = SS2Assets.LoadAsset<ItemDef>("ShackledLamp", SS2Bundle.Items);
                if (lampItem.tier != ItemTier.Boss) // item is disabled
                {
                    var drw = BodyPrefab.GetComponent<DeathRewards>();
                    SS2Log.Info(drw);
                    var fuck = (ExplicitPickupDropTable)drw.bossDropTable;

                    //BasicPickupDropTable

                    PickupDefEntry pde;
                    pde.pickupDef = RoR2Content.Items.Pearl;
                    pde.pickupWeight = 1;

                    //SS2Log.Info("setting fuck | " + fuck.pickupEntries[0]);
                    //try
                    //{
                    //    SS2Log.Info("fuck: " + fuck.pickupEntries[0].pickupDef.name);
                    //}
                    //catch (Exception e)
                    //{
                    //    SS2Log.Info("exceptionl ol " + e);
                    //}

                    fuck.pickupEntries[0] = pde;
                    SerializablePickupIndex fuck2;
                    fuck2.pickupName = RoR2Content.Items.Pearl.name;
                    drw.bossPickup = fuck2;
                    //SS2Log.Info("done");
                    //SS2Log.Info("Disabling lamp drop");
                    //BodyPrefab.AddComponent<LampDropOverride>();


                    //ExplicitPickupDropTable dt = ScriptableObject.CreateInstance<ExplicitPickupDropTable>();
                    //dt.pickupEntries = new ExplicitPickupDropTable.PickupDefEntry[]
                    //{
                    //    new ExplicitPickupDropTable.PickupDefEntry {pickupDef = RoR2Content.Items.Pearl, pickupWeight = 1f},
                    //};
                    //drw.bossDropTable = dt;

                }
            }

        }

        public override void Hook()
        {
            On.RoR2.CharacterBody.AddBuff_BuffDef += CharacterBody_AddBuff_BuffDef;
            //On.RoR2.Projectile.HookProjectileImpact.FixedUpdate += HookProjectileImpact_FixedUpdate;
        }

        private static void CharacterBody_AddBuff_BuffDef(On.RoR2.CharacterBody.orig_AddBuff_BuffDef orig, CharacterBody self, BuffDef buffDef)
        {
            //wayfarer can't buff itself/other wayfarers
            if (self.bodyIndex == BodyCatalog.FindBodyIndex("LampBossBody") && buffDef == SS2Content.Buffs.bdLampBuff)
                return;
            orig(self, buffDef);
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

    public class LampDropOverride : MonoBehaviour
    {
        //why is there not a list of yellow items before a run starts
    }

}
