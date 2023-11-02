using RoR2;
using Moonstorm.Components;
using RoR2.ExpansionManagement;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;
using System.Collections;
using RoR2.ContentManagement;

namespace Moonstorm.Starstorm2.Scenes
{
    [DisabledContent]
    public sealed class VoidShop : SceneBase
    {
        public override SceneDef SceneDef { get; } = SS2Assets.LoadAsset<SceneDef>("VoidShop", SS2Bundle.Stages);
        private static MusicTrackDef music = Addressables.LoadAssetAsync<MusicTrackDef>("RoR2/Base/Common/muSong08.asset").WaitForCompletion();
        public static GameObject portalPrefab;

        public override void Initialize()
        {
            SceneDef.mainTrack = music; //If there is custom music this is temporary. This is the Void Fields theme

            portalPrefab = PrefabAPI.InstantiateClone(SS2Assets.LoadAsset<GameObject>("PortalStrangerExit", SS2Bundle.Stages), "StrangerPortal", true);
            portalPrefab.RegisterNetworkPrefab();

            //is there a better way to spawn zanzan directly in the scene?
            //probably.
            //here's a hook:
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
        }

        public static void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);

            var currStage = SceneManager.GetActiveScene().name;
            if (currStage == "VoidShop")
            {
                //summon trader
                var traderSummon = new MasterSummon();
                traderSummon.position = new Vector3(-26f, 0f, 65.5f);
                traderSummon.rotation = new Quaternion(0f, 90f, 0f, 0f);
                traderSummon.masterPrefab = SS2Assets.LoadAsset<GameObject>("TraderMaster", SS2Bundle.Indev);
                traderSummon.teamIndexOverride = TeamIndex.Neutral;
                traderSummon.Perform();

                //place exit portal
                var position = new Vector3(9f, 35f, -110f);
                var rotation = Quaternion.Euler(330, 0, 0);
                GameObject portal = GameObject.Instantiate(portalPrefab, position, rotation);
                NetworkServer.Spawn(portal);
                //Debug.Log("TERM : " + portal);

               // Debug.Log("placed portal at: " + position + "pos & " + rotation + "rot");
            }
        }
    }
}
