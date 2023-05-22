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

            public new void Awake()
            {
                base.Awake();

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
                //var ind = globalMarkEffectTwo.GetComponent<PositionIndicator>();

                //ind.targetTransform = body.transform;
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
}