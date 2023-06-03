using RoR2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


namespace Moonstorm.Starstorm2.Survivors
{
    [DisabledContent]
    public sealed class Executioner : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("ExecutionerBody", SS2Bundle.Executioner);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("ExecutionerMonsterMasterNew", SS2Bundle.Executioner);
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("SurvivorExecutioner", SS2Bundle.Executioner);

        public static ReadOnlyCollection<BodyIndex> BodiesThatGiveSuperCharge { get; private set; }
        private static List<BodyIndex> bodiesThatGiveSuperCharge = new List<BodyIndex>();

        [SystemInitializer(typeof(BodyCatalog))]
        private static void InitializeSuperchargeList()
        {
            List<string> defaultBodyNames = new List<string>
            {
                //"BrotherGlassBody",
                "BrotherHurtBody",
                "ScavLunar1Body",
                "ScavLunar2Body",
                "ScavLunar3Body",
                "ScavLunar4Body",
                "ShopkeeperBody"
                //"SuperRoboBallBossBody",
                //"DireseekerBossBody"
            };

            foreach(string bodyName in defaultBodyNames)
            {
                BodyIndex index = BodyCatalog.FindBodyIndexCaseInsensitive(bodyName);
                if(index != BodyIndex.None)
                {
                    AddBodyToSuperchargeList(index);
                }
            }
        }

        public static void AddBodyToSuperchargeList(BodyIndex bodyIndex)
        {
            if (bodyIndex == BodyIndex.None)
            {
                SS2Log.Debug($"Tried to add a body to the supercharge list, but it's index is none");
                return;
            }

            if (bodiesThatGiveSuperCharge.Contains(bodyIndex))
            {
                GameObject prefab = BodyCatalog.GetBodyPrefab(bodyIndex);
                SS2Log.Debug($"Body prefab {prefab} is already in the list of bodies that give supercharge.");
                return;
            }
            bodiesThatGiveSuperCharge.Add(bodyIndex);
            BodiesThatGiveSuperCharge = new ReadOnlyCollection<BodyIndex>(bodiesThatGiveSuperCharge);
        }

        public override void ModifyPrefab()
        {
            base.ModifyPrefab();

            var cb = BodyPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
            /*var footstepHandler = BodyPrefab.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>();
            footstepHandler.footstepDustPrefab = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");*/
        }

        internal static int GetIonCountFromBody(CharacterBody body)
        {
            if (body == null) return 1;
            if (body.bodyIndex == BodyIndex.None) return 1;

            if (BodiesThatGiveSuperCharge.Contains(body.bodyIndex))
                return 100;

            if (body.isChampion)
                return 10;

            switch(body.hullClassification)
            {
                case HullClassification.Human:
                    return 1;
                case HullClassification.Golem:
                    return 3;
                case HullClassification.BeetleQueen:
                    return 5;
                default:
                    return 1;
            }
        }

    }
}