using System;
using UnityEngine;
using Moonstorm.AddressableAssets;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RoR2;
using R2API;

namespace Moonstorm.Starstorm2
{
    [CreateAssetMenu(fileName = "SS2EventCard", menuName = "Starstorm2/SS2EventCard")]
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
        public AddressableItemDef requiredItemAddressable;
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

        /// <summary>
        /// Checks if a player in the game has the required item to activate the event.
        /// If no item is specified return true. 
        /// </summary>
        private bool checkRequiredItem() 
        {
            if (!requiredItemAddressable)
            {
                return true;
            } 

            foreach (var player in PlayerCharacterMasterController.instances)
            {
                var playerBody = player.master.GetBody();

                if (!playerBody)
                {
                   var invetoryCount = player.body.inventory.GetItemCount(requiredItemAddressable.Asset.itemIndex);

                   if (invetoryCount > 0)
                   {
                        return true;
                   }
                }
                    
            }
           
            return false;
        }

        // this is needed to make nemesis events only spawn once per stage/not overlap.
        // can be removed once MSU is fixed to not allow events to overridde each other on custom state machines.
        private bool HopefullyTemporaryCheck()
        {
            if (!Moonstorm.Components.EventDirector.Instance) return true; // the fuck?  nebby whyyy
            EntityStateMachine m = EntityStateMachine.FindByCustomName(Moonstorm.Components.EventDirector.Instance.gameObject, "Nemesis");
            return m && m.IsInMainState();
        }
        public override bool IsAvailable()
        {
            var flag = base.IsAvailable();

            if(!flag)
                return flag;

            flag = checkRequiredItem() && HopefullyTemporaryCheck();

            return flag;
        }

        private void CreateVFXDictionary()
        {
            Dictionary<R2API.DirectorAPI.Stage, GameObject> vanillaStageDict = new Dictionary<R2API.DirectorAPI.Stage, GameObject>();;
            Dictionary<string, GameObject> customStageDict = new Dictionary<string, GameObject>();

            foreach(EventVFX vfx in eventVFX)
            {
                if(vfx.stageEnum == DirectorAPI.Stage.Custom)
                {
                    customStageDict[vfx.customStageName] = vfx.effectPrefab;
                }
                else
                {
                    vanillaStageDict[vfx.stageEnum] = vfx.effectPrefab;
                }
            }

            vanillaStageToFXPrefab = new ReadOnlyDictionary<R2API.DirectorAPI.Stage, GameObject>(vanillaStageDict);
            customStageToFXPrefab = new ReadOnlyDictionary<string, GameObject>(customStageDict);
            eventVFX = Array.Empty<EventVFX>();
        }
    }
}
