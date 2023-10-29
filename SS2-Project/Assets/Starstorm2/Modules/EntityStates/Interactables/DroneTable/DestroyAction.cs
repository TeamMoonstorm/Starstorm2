﻿using EntityStates.DroneTable;
using Moonstorm.Starstorm2;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Moonstorm.Starstorm2.Interactables.DroneTable;

namespace EntityStates.DroneTable
{
    public class DestroyAction : DroneTableBaseState
    {

        public static float duration;

        public GameObject droneObject;

        public PickupIndex index;

        public GameObject tempDrone;

        public static Material whiteHoloMaterial;// = SS2Assets.LoadAsset<Material>("matHoloWhite");
        public static Material greenHoloMaterial;// = SS2Assets.LoadAsset<Material>("matHoloGreen");
        public static Material redHoloMaterial;// = SS2Assets.LoadAsset<Material>("matHoloRed");

        public CharacterModel tempModel;

        protected override bool enableInteraction
        {
            get
            {
                return false;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            tempDrone = null;
            
            var holo = this.gameObject.transform.Find("DroneHologramRoot");
            var scrapRoot = this.gameObject.transform.Find("ScrapRoot").gameObject;
            scrapRoot.SetActive(true);

            var locator = droneObject.GetComponent<ModelLocator>();

            //soundID = Util.PlaySound("Play_MULT_m1_sawblade_impact_loop", this.gameObject); //awesome
            Util.PlaySound("RefabricatorAction", this.gameObject);
            if (locator)
            {
                var doubleTempDrone = locator.modelTransform.gameObject;
                RefabricatorTriple outvar;
                bool success = droneTripletPairs.TryGetValue(droneObject.name, out outvar);
                if (success)
                {
                    tempDrone = UnityEngine.Object.Instantiate<GameObject>(doubleTempDrone, outvar.position, Quaternion.Euler(new Vector3(0,0,0)), holo);
                    tempDrone.transform.localPosition = outvar.position;
                    tempDrone.transform.localEulerAngles = outvar.rotation;
                    tempDrone.transform.localScale = outvar.scale;  
                }
                else
                {
                    tempDrone = UnityEngine.Object.Instantiate<GameObject>(doubleTempDrone, holo.position, holo.rotation, holo);
                }

                if(droneObject.name == "MegaDroneBody")
                {
                    try
                    {
                        tempDrone.transform.Find("MegaDroneArmature").Find("Base").gameObject.SetActive(false);
                    }
                    catch (NullReferenceException e)
                    {
                        Debug.Log("failed to turn off megadrone stuff (" + e + ")");
                    }
                }

                var locator2 = tempDrone.GetComponent<ChildLocator>(); //turn off clone drone sparks
                if (locator2)
                {
                    //sparks.FindChildIndex("ChargeSparks");
                    var sparks = locator2.FindChild(locator2.FindChildIndex("ChargeSparks"));
                    if (sparks)
                    {
                        sparks.gameObject.SetActive(false);
                    }

                }

                var fans = droneObject.GetComponentsInChildren<RotateObject>(true);
                if(fans.Length > 0)
                {
                    foreach (var fan in fans)
                    {
                        fan.enabled = false;
                    }
                }

                var hurtboxes = tempDrone.GetComponent<HurtBoxGroup>();
                if (hurtboxes)
                {
                    foreach (var box in hurtboxes.hurtBoxes)
                    {
                        box.enabled = false;
                        box.gameObject.SetActive(false);
                    }
                    hurtboxes.enabled = false;
                }

                var anim = tempDrone.GetComponent<Animator>();
                if (anim)
                {
                    anim.enabled = false;
                }

                tempModel = tempDrone.GetComponent<CharacterModel>();
                SS2Log.Info("tempmodel: " + tempModel + " | " + tempModel.body);
                if (tempModel)
                { 
                    var render = tempModel.baseRendererInfos;
                    switch (index.pickupDef.itemTier)
                    {
                        case ItemTier.Tier1:
                            for (int i = 0; i < render.Length; ++i)
                            {
                                render[i].defaultMaterial = whiteHoloMaterial;
                            }
                            break;

                        case ItemTier.Tier2:
                            for (int i = 0; i < render.Length; ++i)
                            {
                                render[i].defaultMaterial = greenHoloMaterial;
                            }
                            break;

                        case ItemTier.Tier3:
                            for (int i = 0; i < render.Length; ++i)
                            {
                                render[i].defaultMaterial = redHoloMaterial;
                            }
                            break;

                        default:
    
                            break;
                    }
                }
                
            }
            //SS2Log.Info("entered destroy action");
            PlayCrossfade("Main", "Action", "Action.playbackRate", duration, 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge > duration)
                outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();

            //AkSoundEngine.StopPlayingID(soundID);

            var hungry = this.gameObject.transform.Find("Hungry");
            //hungry.localPosition.y = 0.005;
            if (hungry)
            {
                hungry.localPosition += new Vector3(0, .0048f, 0); //appears after 21 drones (.1 / 21)
                if (hungry.localPosition.y > 1.277f)
                {
                    Destroy(hungry.gameObject);
                }
            }

            if (tempDrone)
            {
                Destroy(tempDrone);
            }

            var target = this.gameObject.transform.Find("PickupOrigin");
            Vector3 vec = Vector3.up * 10 + target.forward * 4f;
            PickupDropletController.CreatePickupDroplet(index, target.position, vec);
        }
    }
}
