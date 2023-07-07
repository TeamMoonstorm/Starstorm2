using EntityStates.DroneTable;
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

        public uint soundID;

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
            var body = droneObject.GetComponent<CharacterBody>();

            soundID = Util.PlaySound("Play_MULT_m1_sawblade_impact_loop", this.gameObject); //awesome
            if (locator)
            {
                var doubleTempDrone = locator.modelTransform.gameObject;
                SS2Log.Info("dronheobject name : " + droneObject.name);
                RefabricatorTriple outvar;
                bool success = droneTripletPairs.TryGetValue(droneObject.name, out outvar);
                if (success)
                {
                    tempDrone = UnityEngine.Object.Instantiate<GameObject>(doubleTempDrone, outvar.position, Quaternion.Euler(new Vector3(0,0,0)), holo); //i fucking hate rotations 
                    SS2Log.Info(tempDrone.transform.rotation.eulerAngles + " | " + outvar.rotation);
                    tempDrone.transform.localPosition = outvar.position;
                    tempDrone.transform.localEulerAngles = outvar.rotation;
                    tempDrone.transform.localScale = outvar.scale;  
                }
                else
                {
                    var cm = doubleTempDrone.GetComponent<CharacterModel>();
                    var obj = cm.baseRendererInfos[0].renderer.bounds;
                    SS2Log.Info(obj);
                    var center = new Vector3(obj.center.x, obj.center.y, obj.center.z);
                    var transform = new Vector3(doubleTempDrone.transform.position.x, doubleTempDrone.transform.position.y, doubleTempDrone.transform.position.z);
                    var difference = transform - center;
                    //holo.position -= difference;
                    //tempDrone = UnityEngine.Object.Instantiate<GameObject>(doubleTempDrone, holo);
                    tempDrone = UnityEngine.Object.Instantiate<GameObject>(doubleTempDrone, holo.position + difference, holo.rotation, holo);
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
                if (tempModel)
                { 
                    SS2Log.Info("index.pickupDef.itemTier: " + index.pickupDef.itemTier);
                    var render = tempModel.baseRendererInfos;
                    switch (index.pickupDef.itemTier)
                    {
                        case ItemTier.Tier1:
                            for (int i = 0; i < render.Length; ++i)
                            {
                                render[i].defaultMaterial = whiteHoloMaterial;
                                render[i].renderer.material = whiteHoloMaterial;
                            }
                            break;

                        case ItemTier.Tier2:
                            for (int i = 0; i < render.Length; ++i)
                            {
                                render[i].defaultMaterial = greenHoloMaterial;
                                render[i].renderer.material = greenHoloMaterial;
                            }
                            break;

                        case ItemTier.Tier3:
                            for (int i = 0; i < render.Length; ++i)
                            {
                                render[i].defaultMaterial = redHoloMaterial;
                                render[i].renderer.material = redHoloMaterial;
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
            //if (fixedAge > duration/2)
            //{
                //tempDrone.transform.rotation = Quaternion.Euler(fuck3);
            //}

            if (fixedAge > duration)
                outer.SetNextStateToMain();
        }

        // Update is called once per frame
        public override void OnExit()
        {
            //SS2Log.Info("destroy action finished");
            base.OnExit();

            AkSoundEngine.StopPlayingID(soundID);

            if (tempDrone)
            {
                Destroy(tempDrone);
            }

            var target = this.gameObject.transform.Find("PickupOrigin");
            Vector3 vec = Vector3.up * 10 + target.forward * 3.5f;
            PickupDropletController.CreatePickupDroplet(index, target.position, vec);

        }
    }
}
