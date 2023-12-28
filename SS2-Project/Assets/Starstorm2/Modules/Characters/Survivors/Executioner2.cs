using Mono.Cecil.Cil;
using MonoMod.Cil;
using Moonstorm.Starstorm2.Components;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


namespace Moonstorm.Starstorm2.Survivors
{
    public sealed class Executioner2 : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("Executioner2Body", SS2Bundle.Executioner2);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("Executioner2Master", SS2Bundle.Executioner2);
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("SurvivorExecutioner2", SS2Bundle.Executioner2);

        public static ReadOnlyCollection<BodyIndex> BodiesThatGiveSuperCharge { get; private set; }
        private static List<BodyIndex> bodiesThatGiveSuperCharge = new List<BodyIndex>();

        [SystemInitializer(typeof(BodyCatalog))]
        private static void InitializeSuperchargeList()
        {
            List<string> defaultBodyNames = new List<string>
            {
                "BrotherHurtBody",
                "ScavLunar1Body",
                "ScavLunar2Body",
                "ScavLunar3Body",
                "ScavLunar4Body",
                "ShopkeeperBody"
            };

            foreach(string bodyName in defaultBodyNames)
            {
                BodyIndex index = BodyCatalog.FindBodyIndexCaseInsensitive(bodyName);
                if(index != BodyIndex.None)
                {
                    //AddBodyToSuperchargeList(index); //this counts as a fix.
                }
            }
        }

        public static void AddBodyToSuperchargeList(BodyIndex bodyIndex)
        {
            if (bodyIndex == BodyIndex.None)
            {
                //SS2Log.Debug($"Tried to add a body to the supercharge list, but it's index is none");
                return;
            }

            if (bodiesThatGiveSuperCharge.Contains(bodyIndex))
            {
                GameObject prefab = BodyCatalog.GetBodyPrefab(bodyIndex);
                //SS2Log.Debug($"Body prefab {prefab} is already in the list of bodies that give supercharge.");
                return;
            }
            bodiesThatGiveSuperCharge.Add(bodyIndex);
            BodiesThatGiveSuperCharge = new ReadOnlyCollection<BodyIndex>(bodiesThatGiveSuperCharge);
        }

        public override void ModifyPrefab()
        {
            base.ModifyPrefab();

            var cb = BodyPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
            /*var footstepHandler = BodyPrefab.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>();
            footstepHandler.footstepDustPrefab = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");*/
        }

        public override void Hook()
        {
            SetupFearExecute();
            On.RoR2.MapZone.TeleportBody += MarkOOB;
            On.RoR2.TeleportHelper.TeleportBody += HelpOOB;
        }

        private void MarkOOB(On.RoR2.MapZone.orig_TeleportBody orig, MapZone self, CharacterBody characterBody)
        {
            //orig(self, characterBody);
            var exc = characterBody.gameObject.GetComponent<ExecutionerController>();
            if (exc)
            {
                exc.hasOOB = true;
            }
            orig(self, characterBody);
        }

        private void HelpOOB(On.RoR2.TeleportHelper.orig_TeleportBody orig, CharacterBody body, Vector3 targetFootPosition)
        {
            var exc = body.gameObject.GetComponent<ExecutionerController>();
            if (exc)
            {
                //SS2Log.Info("isOOB? " + exc.hasOOB + " | " + exc.isExec);
                if (exc.isExec)
                {
                    exc.hasOOB = true;
                    targetFootPosition += new Vector3(0, 1, 0);
                }
            }
            orig(body, targetFootPosition);
        }

        private HealthComponent.HealthBarValues FearExecuteHealthbar(On.RoR2.HealthComponent.orig_GetHealthBarValues orig, HealthComponent self)
        {
            var hbv = orig(self);
            if (!self.body.bodyFlags.HasFlag(CharacterBody.BodyFlags.ImmuneToExecutes) && self.body.HasBuff(SS2Content.Buffs.BuffFear))
            {
                hbv.cullFraction += 0.15f;//(self.body && self.body.isChampion) ? 0.15f : 0.3f; //might stack too crazy if it's 30% like Freeze
            }
            return hbv;
        }

        private void SetupFearExecute() //thanks moffein, hope you're doing well ★
        {
            On.RoR2.HealthComponent.GetHealthBarValues += FearExecuteHealthbar;

            //Prone to breaking when the game updates
            IL.RoR2.HealthComponent.TakeDamage += (il) =>
            {
                bool error = true;
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After,
                    x => x.MatchStloc(53)   //num17 = float.NegativeInfinity, stloc53 = Execute Fraction, first instance it is used
                    ))
                {
                    if (c.TryGotoNext(MoveType.After,
                    x => x.MatchLdloc(8)   //flag 5, this is checked before final Execute damage calculations.
                    ))
                    {
                        c.Emit(OpCodes.Ldarg_0);//self
                        c.Emit(OpCodes.Ldloc, 53);//execute fraction
                        c.EmitDelegate<Func<HealthComponent, float, float>>((self, executeFraction) =>
                        {
                            if (self.body.HasBuff(SS2Content.Buffs.BuffFear))
                            {
                                if (executeFraction < 0f) executeFraction = 0f;
                                executeFraction += 0.15f;
                            }
                            return executeFraction;
                        });
                        c.Emit(OpCodes.Stloc, 53);

                        error = false;
                    }
                }

                if (error)
                {
                    Debug.LogError("Starstorm 2: Fear Execute IL Hook failed.");
                }

            };
            IL.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += (il) =>
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(
                      x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "Cripple")
                     ))
                {
                    c.Index += 2;
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<bool, CharacterBody, bool>>((hasBuff, self) =>
                    {
                        return hasBuff || self.HasBuff(SS2Content.Buffs.BuffFear);
                    });
                }
                else
                {
                    Debug.LogError("Starstorm 2: Fear VFX IL Hook failed.");
                }
            };
        }

        internal static int GetIonCountFromBody(CharacterBody body)
        {
            if (body == null) return 1;
            if (body.bodyIndex == BodyIndex.None) return 1;

            if (BodiesThatGiveSuperCharge.Contains(body.bodyIndex))
                return 100;

            if (body.isChampion)
                return 10;

            switch(body.hullClassification)
            {
                case HullClassification.Human:
                    return 1;
                case HullClassification.Golem:
                    return 3;
                case HullClassification.BeetleQueen:
                    return 5;
                default:
                    return 1;
            }
        }

    }
}