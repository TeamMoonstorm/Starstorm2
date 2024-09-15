﻿
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RoR2;
using R2API;
using Moonstorm;
using RoR2.ContentManagement;
namespace SS2
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
        public ItemDef requiredItem;
        public ArtifactDef requiredArtifactDef;
        public bool requiresVoid;
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
                foreach (var instance in instances)
                {
                    instance.CreateVFXDictionary();
                }
            });
        }

        /// <summary>
        /// Checks if a player in the game has the required item to activate the event.
        /// If no item is specified return true. 
        /// </summary>
        private bool CheckRequiredItem()
        {
            if (!requiredItem)
            {
                return true;
            }

            foreach (var player in PlayerCharacterMasterController.instances)
            {
                var playerBody = player.master.GetBody();

                if (player.body.inventory != null && requiredItem != null)
                {
                    var inventoryCount = player.body.inventory.GetItemCount(requiredItem.itemIndex);

                    if (inventoryCount > 0)
                    {
                        return true;
                    }
                }
            }

            //this doesn't work i think because this is checked before player bodies are actually initialized....?
            //to-do: rewrite or alternate use case.

            return true;
        }

        private bool CheckVoid()
        {
            if (!requiresVoid)
                return true;

            //Debug.Log(Items.VoidRock.invasionStage + " : invasion stage");
            return Items.VoidRock.invasionStage;
        }

        // this is needed to make nemesis events only spawn once per stage/not overlap.
        // can be removed once MSU is fixed to not allow events to overridde each other on custom state machines.
        private bool HopefullyTemporaryCheck()
        {
            if (!Moonstorm.Components.EventDirector.Instance) return true; // the fuck?  nebby whyyy
            if (!this.requiredStateMachine.Equals("Nemesis")) return true; // oops forgot this
            EntityStateMachine m = EntityStateMachine.FindByCustomName(Moonstorm.Components.EventDirector.Instance.gameObject, "Nemesis");
            return m && m.IsInMainState();
        }
        public override bool IsAvailable()
        {
            var flag = base.IsAvailable();

            if (!flag)
                return flag;

            flag = CheckRequiredItem() && HopefullyTemporaryCheck() && CheckVoid();

            return flag;
        }

        private void CreateVFXDictionary()
        {
            Dictionary<R2API.DirectorAPI.Stage, GameObject> vanillaStageDict = new Dictionary<R2API.DirectorAPI.Stage, GameObject>(); ;
            Dictionary<string, GameObject> customStageDict = new Dictionary<string, GameObject>();

            foreach (EventVFX vfx in eventVFX)
            {
                if (vfx.stageEnum == DirectorAPI.Stage.Custom)
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
