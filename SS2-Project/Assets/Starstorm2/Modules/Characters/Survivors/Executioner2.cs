using Mono.Cecil.Cil;
using MonoMod.Cil;
using SS2.Components;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using MSU;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using R2API;
using RoR2.ContentManagement;

namespace SS2.Survivors
{
    public sealed class Executioner2 : SS2Survivor
    {
        public override SurvivorDef SurvivorDef => _survivorDef;
        private SurvivorDef _survivorDef;
        public override NullableRef<GameObject> MasterPrefab => _monsterMaster;
        private GameObject _monsterMaster;
        public override GameObject CharacterPrefab => _prefab;
        private GameObject _prefab;

        private BuffDef _exeChargeBuffDef;
        private BuffDef _exeArmor;

        public static GameObject plumeEffect;
        public static GameObject plumeEffectLarge;


        public sealed class ExeArmorBehavior : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffExecutionerArmor;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                //the stacking amounts are added by the item - these base values are here in case the buff is granted by something other than sigil
                args.armorAdd += 60;
            }
        }

        public sealed class ExeChargeBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdExeCharge;
            private float timer;

            public void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    //INDICES PEOPLE, INDICES :SOB: -N
                    if (CharacterBody.baseNameToken != "SS2_EXECUTIONER2_NAME" || CharacterBody.HasBuff(SS2Content.Buffs.bdExeMuteCharge))
                        return;
                    else
                        timer += Time.fixedDeltaTime;

                    if (timer >= 1.2f && CharacterBody.skillLocator.secondary.stock < CharacterBody.skillLocator.secondary.maxStock)
                    {
                        timer = 0f;

                        CharacterBody.skillLocator.secondary.AddOneStock();

                        if (CharacterBody.skillLocator.secondary.stock < CharacterBody.skillLocator.secondary.maxStock)
                        {
                            Util.PlaySound("ExecutionerGainCharge", gameObject);
                            EffectManager.SimpleMuzzleFlash(plumeEffect, gameObject, "ExhaustL", true);
                            EffectManager.SimpleMuzzleFlash(plumeEffect, gameObject, "ExhaustR", true);
                        }
                        if (CharacterBody.skillLocator.secondary.stock >= CharacterBody.skillLocator.secondary.maxStock)
                        {
                            Util.PlaySound("ExecutionerMaxCharge", gameObject);
                            EffectManager.SimpleMuzzleFlash(plumeEffectLarge, gameObject, "ExhaustL", true);
                            EffectManager.SimpleMuzzleFlash(plumeEffectLarge, gameObject, "ExhaustR", true);
                            EffectManager.SimpleEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/LightningFlash.prefab").WaitForCompletion(), CharacterBody.corePosition, Quaternion.identity, true);
                        }

                        CharacterBody.SetAimTimer(timer);
                    }
                }
            }
        }

        public static ReadOnlyCollection<BodyIndex> BodiesThatGiveSuperCharge { get; private set; }
        private static HashSet<string> bodiesThatGiveSuperCharge = new HashSet<string>
        {
                "BrotherHurtBody",
                "ScavLunar1Body",
                "ScavLunar2Body",
                "ScavLunar3Body",
                "ScavLunar4Body",
                "ShopkeeperBody"
        };

        public static void AddBodyToSuperChargeCollection(string body)
        {
            bodiesThatGiveSuperCharge.Add(body);
            UpdateSuperChargeList();
        }

        public override void Initialize()
        {
            BodyCatalog.availability.CallWhenAvailable(UpdateSuperChargeList);
            Hook();
            ModifyPrefab();
        }

        private static void UpdateSuperChargeList()
        {
            List<BodyIndex> indices = new List<BodyIndex>();
            foreach(var bodyName in bodiesThatGiveSuperCharge)
            {
                BodyIndex index = BodyCatalog.FindBodyIndex(bodyName);
                if (index == BodyIndex.None)
                    continue;

                indices.Add(index);
            }
            BodiesThatGiveSuperCharge = new ReadOnlyCollection<BodyIndex>(indices);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * GameObject - "Executioner2Body" - Executioner2
             * GameObject - "Executioner2Master" - Executioner2
             * SurvivorDef - "SurvivorExecutioner2" - Executioner2
             * BuffDef - "bdExeCharge" - Executioner2
             * BuffDef - "BuffExecutionerArmor" - Executioenr2
             * GameObject - "exePlume" - Executioner2
             * GameObject - "exePlumeBig" - Executioner2
             * GameObject - "ExecutionerIonOrbEffect" - Executioner2
             * GameObject - "ExecutionerIonOrbEffectMastery" - Executioner2
             * GameObject - "ExecutionerIonSuperOrbEffect" - Executioner2
             */
            yield break;
        }

        public void ModifyPrefab()
        {
            var cb = _prefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }

        public void Hook()
        {
            SetupFearExecute();
            On.RoR2.MapZone.TeleportBody += MarkOOB;
            On.RoR2.TeleportHelper.TeleportBody += HelpOOB;
        }

        private void MarkOOB(On.RoR2.MapZone.orig_TeleportBody orig, MapZone self, CharacterBody characterBody)
        {
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
                hbv.cullFraction += 0.15f;//might stack too crazy if it's 30% like Freeze
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
                    SS2Log.Fatal("Starstorm 2: Fear Execute IL Hook failed.");
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
                    SS2Log.Fatal("Starstorm 2: Fear VFX IL Hook failed.");
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

            switch (body.hullClassification)
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