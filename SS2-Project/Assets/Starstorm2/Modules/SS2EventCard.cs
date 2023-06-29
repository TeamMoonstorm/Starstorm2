using Moonstorm.Config;
using Moonstorm.Starstorm2.ScriptableObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using RiskOfOptions.OptionConfigs;
using Object = UnityEngine.Object;
using BepInEx.Configuration;
using static System.Collections.Specialized.BitVector32;

namespace Moonstorm.Starstorm2
{
    public class SS2EventCard : EventCard
    {
        [Serializable]
        public class EventVFX
        {
            public R2API.DirectorAPI.Stage stageEnum;
            public string customStageName;
            public GameObject effectPrefab;
        }

        private static List<SS2EventCard> instances = new List<SS2EventCard>();
        public AddressableItemDef requiredItem;
        public GameObject fallbackVFX;
        public EventVFX[] eventVFX = Array.Empty<EventVFX>();
        
        public ReadOnlyDictionary<R2API.DirectorAPI.Stage, GameObject> vanillaStageToFXPrefab;
        public ReadOnlyDictionary<string, GameObject> customStageToFXPrefab;

        private void Awake()
        {
            instances?.Add(this);    
        }
        private void OnDestroy()
        {
            instances?.Remove(this);
        }

        [SystemInitializer]
        private static void SystemInit()
        {
            EventCatalog.resourceAvailability.CallWhenAvailable(() =>
            {
                foreach(var instance in instances)
                {
                    instance.CreateVFXDictionary();
                }
            });
        }
        public override bool IsAvailable()
        {
            var flag = base.IsAvailable();
            if(!flag)
                return flag;
            flag = true // TODO: Some method that checks if any player has the required item
            return flag;
        }

        private void CreateVFXDictionary()
        {
            Dictionary<R2API.DirectorAPI.Stage, GameObject> dict1 = new Dictionary<R2API.DirectorAPI.Stage, GameObject>();;
            Dictionary<string, GameObject> dict2 = new Dictioanry<string, GameObject>();
            foreach(EventVFX vfx in eventVFX)
            {
            if(vfx.stageEnum == DirectorAPI.Stage.Custom)
            {
                dict2[vfx.customStageName] = vfx.effectPrefab;
            }
            else
            {
                dict1[vfx.stageEnum] = vfx.EffectPrefab;
            }
            }
            vanillaStageToFXPrefab = new ReadOnlyDictioanry(dict1);
            customStageToFXPrefab = new ReadOnlyDictioanry(dict2);
            eventVFX = Array.Empty<EventVFX>();
        }
    }
}
