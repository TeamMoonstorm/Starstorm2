using KinematicCharacterController;
using R2API;
using RoR2;
using RoR2.Items;
using System;
using UnityEngine;
using static Moonstorm.Starstorm2.Items.RelicOfTermination;

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
            public GameObject markSpinBoss;
            float timer = 0;
            float maxTime = 30;
            public Transform pointerToken;
            int i = 0;
            public bool activeRing;

            public RelicOfTermination.TerminationToken token;

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

                //body.teamComponent.indicator
                if (!body.isBoss)
                {
                    body.teamComponent.RequestDefaultIndicator(globalMarkEffectTwo);
                }
                
                //var comps = body.gameObject.GetComponents<Component>();
                //foreach(var comp in comps)
                //{
                //    /SS2Log.Info(comp.name + " | " + comp.GetType());
                //}

                token = body.gameObject.GetComponent<TerminationToken>();

                if (token)
                {
                    //SS2Log.Info("limit: " + token.timeLimit + " from " + maxTime);
                    maxTime = token.timeLimit;
                }
            }

            public void FixedUpdate()
            {
                if (markRing)
                {
                    float ratio = timer / maxTime;
                    if (ratio < 1)
                    {
                        float amount = sizeCurve.Evaluate(ratio);
                        Vector3 vec = new Vector3(amount, amount, amount);
                        markRing.transform.localScale = vec;
                        timer += Time.fixedDeltaTime;
                    }
                    else if(activeRing)
                    {
                        markRing.SetActive(false);
                        activeRing = false;
                    }
                }
                else if(i < 25)
                {
                    pointerToken = body.transform.Find("TerminationPositionIndicator(Clone)");
                    if (pointerToken)
                    {
                        var posind = pointerToken.GetComponent<PositionIndicator>();
                        if (posind)
                        {
                            var insobj = posind.insideViewObject;
                            if (insobj)
                            {
                                markRing = insobj.transform.Find("Ring").gameObject;
                                if (body.isBoss)
                                {
                                    insobj.transform.Find("Crosshair2").gameObject.SetActive(true);
                                    //SS2Log.Info("object is boss helper");
                                }
                                sizeCurve = AnimationCurve.Linear(0, .235f, 1, .066f);
                                activeRing = true;
                            }
                        }
                    }
                    ++i;
                    if(!(i < 25))
                    {
                        SS2Log.Info("Giving up on finding termination ring for " + body.name); //this is so that it's less shit that im doing getcomponent in fixedupdate listen its ok
                        //conceded = true; // lol oopps
                    }
                }
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.healthMultAdd += RelicOfTermination.hpMult;
                args.damageMultAdd += RelicOfTermination.damageMult;
                args.moveSpeedMultAdd += RelicOfTermination.speedMult;
                args.attackSpeedMultAdd += RelicOfTermination.atkSpeedMult;
                args.baseHealthAdd += RelicOfTermination.hpAdd + ((RelicOfTermination.hpAdd / 4) * (body.level - 1));
            }

        }
    }
}