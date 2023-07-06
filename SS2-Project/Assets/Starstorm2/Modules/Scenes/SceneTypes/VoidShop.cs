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
    public sealed class VoidShop : SceneBase
    {
        public override SceneDef SceneDef { get; } = SS2Assets.LoadAsset<SceneDef>("VoidShop", SS2Bundle.Stages);
        private static MusicTrackDef music = Addressables.LoadAssetAsync<MusicTrackDef>("RoR2/Base/Common/muSong08.asset").WaitForCompletion();

        public override void Initialize()
        {
            SceneDef.mainTrack = music; //If there is custom music this is temporary. This is the Void Fields theme


        }
    }
}
