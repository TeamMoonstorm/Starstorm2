using MSU;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2.ContentManagement;
using MSU.Config;
using RiskOfOptions.OptionConfigs;

namespace SS2
{
    /// <summary>
    /// <inheritdoc cref="ISurvivorContentPiece"/>
    /// </summary>
    public abstract class SS2Survivor : ISurvivorContentPiece, IContentPackModifier
    {
        public  SurvivorAssetCollection AssetCollection { get; set; }
        public SurvivorDef survivorDef { get; protected  set; }
        public NullableRef<GameObject> masterPrefab { get; protected set; }
        CharacterBody IGameObjectContentPiece<CharacterBody>.component => CharacterPrefab.GetComponent<CharacterBody>();
        GameObject IContentPiece<GameObject>.asset => CharacterPrefab;
        public GameObject CharacterPrefab { get; protected set; }

        public abstract void Initialize();
        public virtual bool IsAvailable(ContentPack conentPack)
        {
            ConfiguredBool isDisabled = SS2Config.ConfigFactory.MakeConfiguredBool(false, b =>
            {
                b.section = "00 - Survivor Disabling";
                b.key = $"Disable Survivor: {MSUtil.NicifyString(GetType().Name)}";
                b.description = "Set this to true if you want to disable this survivor from appearing in game. Why....? Make sure everyone has this enabled or disabled in multiplayer otherwise desyncs could occur.";
                b.configFile = SS2Config.ConfigSurvivor;
                b.checkBoxConfig = new CheckBoxConfig
                {
                    restartRequired = true
                };
            }).DoConfigure();

            return !isDisabled;
        }

        public abstract SS2AssetRequest<SurvivorAssetCollection> AssetRequest { get; }

        protected void SetupDefaultBody(GameObject prefab)
        {
            CharacterBody cb = prefab.GetComponent<CharacterBody>();
            if (cb)
            {
                UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPod.prefab").Completed += (x) => { cb.preferredPodPrefab = x.Result; };
                UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab ").Completed += (x) => { cb._defaultCrosshairPrefab = x.Result; };
            }
        }
        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest<SurvivorAssetCollection> request = AssetRequest;

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            AssetCollection = request.Asset;

            CharacterPrefab = AssetCollection.bodyPrefab;
            masterPrefab = AssetCollection.masterPrefab;
            survivorDef = AssetCollection.survivorDef;

        }


        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }
    }
}