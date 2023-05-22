using KinematicCharacterController;
using R2API;
using RoR2;
using RoR2.Items;
using System;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class TerminationHelper : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("TerminationHelper", SS2Bundle.Items);

        public static GameObject globalMarkEffectTwo;

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.TerminationHelper;
            private static AnimationCurve sizeCurve;
            public GameObject terminationMark;
            public GameObject markRing;
            float timer = 0;
            float maxTime = 30;
            public Transform pointerToken;
            int i = 0;
            public new void Awake()
            {
                base.Awake();
                //sizeCurve = AnimationCurve.Linear(0, .235f, 1, .066f);
                globalMarkEffectTwo = SS2Assets.LoadAsset<GameObject>("TerminationPositionIndicator", SS2Bundle.Items);

                //body.teamComponent.RequestDefaultIndicator(globalMarkEffectTwo);

                float modifier = RelicOfTermination.scaleMod;

                body.modelLocator.modelTransform.localScale *= modifier;

                if (body.isFlying)
                {
                    KinematicCharacterMotor[] list = body.GetComponentsInChildren<KinematicCharacterMotor>();
                    foreach (KinematicCharacterMotor motor in list)
                    {
                        if (motor)
                        {
                            motor.SetCapsuleDimensions(motor.Capsule.radius * modifier, motor.CapsuleHeight * modifier, modifier);
                        }
                    }
                    // obj.characterMotor.
                }

                var printController = body.modelLocator.modelTransform.GetComponent<PrintController>();

                if (printController)
                {
                    printController.printTime = 25f;
                    printController.maxPrintHeight = 100;
                }

                body.teamComponent.RequestDefaultIndicator(globalMarkEffectTwo);
                
                //body.gameObject.AddComponent<Fuck>();
                //pointerToken = body.transform.Find("TerminationPositionIndicator(Clone)"); //the default indicator might not be in by now so it wont find it
                //Transform[] tranf = body.gameObject.GetComponentsInChildren<Transform>();
                //foreach(Transform trans in tranf)
                //{
                //    SS2Log.Info(trans.name + " | " + trans);
                //}
                //pointerToken = body.transform.Find("TerminationPositionIndicator");
                //pointerToken = body.gameObject.GetComponentInChildren<TerminationPointerToken>();
                //SS2Log.Info("pointer token: " + pointerToken);
                //if (pointerToken)
                //{
                //    var posind = this.GetComponent<PositionIndicator>();
                //    SS2Log.Info("posind: " + posind);
                //    if (posind)
                //    {
                //        var insobj = posind.insideViewObject;
                //        SS2Log.Info("posind: " + insobj);
                //        //insobj.GetComponent
                //        //insobj
                //        //curve = insobj.GetComponent<ObjectScaleCurve>();
                //        if (insobj)
                //        {
                //            markRing = insobj.transform.Find("Ring").gameObject;
                //            SS2Log.Info("posind: " + markRing);
                //            //markRing = pointerToken.gameObject;
                //        }
                //    }
                //}
                //insobj.transform.Find("Ring").gameObject;
                //terminationMark = body.transform.Find("TerminationPositionIndicator").gameObject;
                //markRing = terminationMark.transform.Find("Ring").gameObject;
                //body.gameObject.AddComponent<Fuck>();
                //var ind = globalMarkEffectTwo.GetComponent<PositionIndicator>();

                //ind.targetTransform = body.transform;
            }

            public void FixedUpdate()
            {
                if (markRing)
                {
                    float ratio = timer / maxTime;
                    if (ratio < 1)
                    {
                        //SS2Log.Info("updating ring " + ratio);
                        float amount = sizeCurve.Evaluate(ratio);
                        Vector3 vec = new Vector3(amount, amount, amount);
                        markRing.transform.localScale = vec;
                        //SS2Log.Info("timer " + ratio + " | " + amount);
                        timer += Time.fixedDeltaTime;
                    }
                    //terminationMark.transform.Find("Ring");
                }
                else if(i < 25)
                {
                    pointerToken = body.transform.Find("TerminationPositionIndicator(Clone)");
                    //pointerToken = body.gameObject.GetComponentInChildren<TerminationPointerToken>();
                    //SS2Log.Info("pointer token: " + pointerToken);
                    if (pointerToken)
                    {
                        var posind = pointerToken.GetComponent<PositionIndicator>();
                        //SS2Log.Info("posind: " + posind);
                        if (posind)
                        {
                            var insobj = posind.insideViewObject;
                            //SS2Log.Info("posind: " + insobj);
                            //insobj.GetComponent
                            //insobj
                            //curve = insobj.GetComponent<ObjectScaleCurve>();
                            if (insobj)
                            {
                                markRing = insobj.transform.Find("Ring").gameObject;
                                //SS2Log.Info("posind: " + markRing);
                                //markRing = pointerToken.gameObject;
                                sizeCurve = AnimationCurve.Linear(0, .235f, 1, .066f);
                            }
                        }
                    }
                    ++i;
                    if(!(i < 25))
                    {
                        SS2Log.Info("Giving up on finding termination ring for " + body.name); //this is so that it's less shit that im doing getcomponent in fixedupdate listen its ok
                    }
                }

            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.healthMultAdd += RelicOfTermination.healthMult;
                args.damageMultAdd += RelicOfTermination.damageMult;
                args.moveSpeedMultAdd += RelicOfTermination.speedMult;
                args.attackSpeedMultAdd += RelicOfTermination.atkSpeedMult;
            }

        }
    }
    public class Fuck : MonoBehaviour
    {

    }
}