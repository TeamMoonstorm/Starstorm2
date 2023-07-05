using EntityStates.DroneTable;
using Moonstorm.Starstorm2;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.DroneTable
{
    public class DestroyAction : DroneTableBaseState
    {

        public static float duration;

        public GameObject droneObject;

        public PickupIndex index;

        public GameObject tempDrone;

        public static Material whiteHoloMaterial = SS2Assets.LoadAsset<Material>("matHoloWhite");
        public static Material greenHoloMaterial = SS2Assets.LoadAsset<Material>("matHoloGreen");
        public static Material redHoloMaterial = SS2Assets.LoadAsset<Material>("matHoloRed");

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
            var locator = droneObject.GetComponent<ModelLocator>();
            if (locator)
            {
                var doubleTempDrone = locator.modelTransform.gameObject;
                tempDrone = UnityEngine.Object.Instantiate<GameObject>(doubleTempDrone, holo);
                //var curve = new AnimationCurve();
                //curve.AddKey(0, 1);
                //curve.AddKey(1, 0);
                //var controller = tempDrone.AddComponent<PrintController>();
                //controller.characterModel = tempDrone.GetComponent<CharacterModel>();
                //controller.printTime = .75f;
                //controller.enabled = true;
                //controller.disableWhenFinished = true;
                //controller.startingPrintHeight = 1;
                //controller.startingPrintBias = 1f;
                //controller.maxPrintBias = 3.5f;
                //controller.maxPrintHeight = 1;
                //controller.animateFlowmapPower = true;
                //controller.startingFlowmapPower = 1.14f;
                //controller.maxFlowmapPower = 30f;
                //controller.printCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
                //
                //TemporaryOverlay overlay = tempDrone.AddComponent<TemporaryOverlay>();
                //overlay.duration = .75f;
                //overlay.animateShaderAlpha = true;
                //overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                //overlay.destroyComponentOnEnd = true;
                //overlay.originalMaterial = RoR2.LegacyResourcesAPI.Load<Material>("Materials/matClayGooDebuff");
                //overlay.AddToCharacerModel(controller.characterModel);
                var cm = tempDrone.GetComponent<CharacterModel>();
                if (cm)
                {
                    
                    SS2Log.Info("index.pickupDef.itemTier: " + index.pickupDef.itemTier);
                    switch (index.pickupDef.itemTier)
                    {
                        case ItemTier.Tier1:
                            var render1 = cm.baseRendererInfos;
                            for (int i = 0; i < render1.Length; ++i)
                            {
                                render1[i].defaultMaterial = whiteHoloMaterial;
                            }
                            break;

                        case ItemTier.Tier2:
                            var render2 = cm.baseRendererInfos;
                            for (int i = 0; i < render2.Length; ++i)
                            {
                                render2[i].defaultMaterial = greenHoloMaterial;
                            }
                            break;

                        case ItemTier.Tier3:
                            var render3 = cm.baseRendererInfos;
                            for (int i = 0; i < render3.Length; ++i)
                            {
                                render3[i].defaultMaterial = redHoloMaterial;
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

        // Update is called once per frame
        public override void OnExit()
        {
            //SS2Log.Info("destroy action finished");
            base.OnExit();
            var target = this.gameObject.transform.Find("PickupOrigin");
            if (tempDrone)
            {
                Destroy(tempDrone);
            }
            Vector3 vec = Vector3.up * 10 + target.forward * 3.5f;
            PickupDropletController.CreatePickupDroplet(index, target.position, vec);

        }
    }
}
