using System;
using UnityEngine;
using Moonstorm.AddressableAssets;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;

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

        private bool checkRequiredItem() 
        {
            if (!requiredItem)
            {
                return true;
            } 

            foreach (var player in PlayerCharacterMasterController.instances)
            {
                var playerBody = player.master.GetBody();

                if (!playerBody)
                {
                    var itemDef = Addressables.LoadAssetAsync<ItemDef>(requiredItem.address).WaitForCompletion();

                    var invetoryCount = player.body.inventory.GetItemCount(itemDef.itemIndex);

                   if (invetoryCount > 0)
                   {
                        return true;
                   }
                }
                    
            }
           
            return false;
        }
        public override bool IsAvailable()
        {
            var flag = base.IsAvailable();

            if(!flag)
                return flag;

            flag = checkRequiredItem(); // TODO: Some method that checks if any player has the required item

            return flag;
        }

        private void CreateVFXDictionary()
        {
            Dictionary<R2API.DirectorAPI.Stage, GameObject> dict1 = new Dictionary<R2API.DirectorAPI.Stage, GameObject>();;
            Dictionary<string, GameObject> dict2 = new Dictionary<string, GameObject>();

            foreach(EventVFX vfx in eventVFX)
            {
                if(vfx.stageEnum == DirectorAPI.Stage.Custom)
                {
                    dict2[vfx.customStageName] = vfx.effectPrefab;
                }
                else
                {
                    dict1[vfx.stageEnum] = vfx.effectPrefab;
                }
            }

            vanillaStageToFXPrefab = new ReadOnlyDictionary<R2API.DirectorAPI.Stage, GameObject>(dict1);
            customStageToFXPrefab = new ReadOnlyDictionary<string, GameObject>(dict2);
            eventVFX = Array.Empty<EventVFX>();
        }
    }
}
