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
using RoR2.Orbs;
using R2API.Utils;

namespace SS2.Survivors
{
    public sealed class Executioner2 : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acExecutioner2", SS2Bundle.Executioner2);

        static string path = "Prefabs/Effects/OrbEffects/LightningOrbEffect";

        public static GameObject plumeEffect;
        public static GameObject plumeEffectLarge;
        public static GameObject taserVFX;
        public static GameObject taserVFXFade;

        public static BuffDef _buffDefFear;
        public static BuffDef _buffExeMuteCharge;
        public static BuffDef _buffExeSuperCharged;


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
            plumeEffect = AssetCollection.FindAsset<GameObject>("exePlume");
            plumeEffectLarge = AssetCollection.FindAsset<GameObject>("exePlumeBig");
            _buffDefFear = AssetCollection.FindAsset<BuffDef>("BuffFear");
            _buffExeMuteCharge = AssetCollection.FindAsset<BuffDef>("bdExeMuteCharge");
            _buffExeSuperCharged = AssetCollection.FindAsset<BuffDef>("BuffExecutionerSuperCharged");

            BodyCatalog.availability.CallWhenAvailable(UpdateSuperChargeList);
            Hook();
            ModifyPrefab();

            taserVFX = LegacyResourcesAPI.Load<GameObject>(path);

            IL.RoR2.Orbs.OrbEffect.Start += OrbEffect_Start; // i dont know what this is but its failing

            //On.RoR2.UI.CharacterSelectController.RebuildStrip += CheckForSwitches;
            //On.RoR2.UI.CharacterSelectController.BuildSkillStripDisplayData += CheckForDisplaySwitch;
        }

        private void CheckForDisplaySwitch(On.RoR2.UI.CharacterSelectController.orig_BuildSkillStripDisplayData orig, RoR2.UI.CharacterSelectController self, Loadout loadout, ValueType bodyInfo, object dest)
        {
            orig(self, loadout, bodyInfo, dest);
        }

        private void OrbEffect_Start(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            bool ILFound = cursor.TryGotoNext(MoveType.After,
                instruction => instruction.MatchBrtrue(out _),
                instruction => instruction.MatchLdarg(0),
                instruction => instruction.MatchLdfld<OrbEffect>(nameof(OrbEffect.startPosition))
                );
            if (ILFound)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(OrbEffect).GetFieldCached(nameof(OrbEffect.targetTransform)));
                cursor.Emit(OpCodes.Ldloc_0);
                cursor.EmitDelegate<Func<Vector3, Transform, EffectComponent, Vector3>>((startPosition, targetTransform, effectComponent) =>
                {
                    if (targetTransform == null)
                    {
                        startPosition = effectComponent.effectData.start;
                    }
                    return startPosition;
                });
                SS2Log.Info("Added OrbEffect_Start hook :D");
                //Debug.Log("Added OrbEffect_Start hook :D");
            }
            else
            {
                SS2Log.Error("OrbEffect_Start hook failed.");
                //Debug.Log("ah shit");  soulless but informative
            }
        }

        private static void UpdateSuperChargeList()
        {
            List<BodyIndex> indices = new List<BodyIndex>();
            foreach (var bodyName in bodiesThatGiveSuperCharge)
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


        public void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }

        public void Hook()
        {
            SetupFearExecute();
            On.RoR2.MapZone.TeleportBody += MarkOOB;
            On.RoR2.TeleportHelper.TeleportBody_CharacterBody_Vector3 += HelpOOB;
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

        private void HelpOOB(On.RoR2.TeleportHelper.orig_TeleportBody_CharacterBody_Vector3 orig, CharacterBody body, Vector3 targetFootPosition)
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

            IL.RoR2.HealthComponent.TakeDamageProcess += (il) =>
            {
                bool error = true;
                ILCursor c = new ILCursor(il);

                if (c.TryGotoNext(x => x.MatchLdloc(73), x => x.MatchLdcR4(0)))
                {
                    c.Index++;
                    c.Emit(OpCodes.Ldarg_0);//self
                    c.EmitDelegate<Func<float, HealthComponent, float>>((executeFraction, self) =>
                    {
                        if (self.body.HasBuff(SS2Content.Buffs.BuffFear))
                        {
                            if (executeFraction < 0f) executeFraction = 0f;
                            executeFraction += 0.15f;
                        }
                        return executeFraction;
                    });

                    if (c.TryGotoNext(x => x.MatchLdloc(73)))
                    {
                        c.Index++;
                        c.Emit(OpCodes.Ldarg_0);//self
                        c.EmitDelegate<Func<float, HealthComponent, float>>((executeFraction, self) =>
                        {
                            if (self.body.HasBuff(SS2Content.Buffs.BuffFear))
                            {
                                if (executeFraction < 0f) executeFraction = 0f;
                                executeFraction += 0.15f;
                            }
                            return executeFraction;
                        });
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
                return 5;

            switch (body.hullClassification)
            {
                case HullClassification.Human:
                    return 1;
                case HullClassification.Golem:
                    return 2;
                case HullClassification.BeetleQueen:
                    return 3;
                default:
                    return 1;
            }
        }
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

        public sealed class FearDebuffBehavior : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => _buffDefFear;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedReductionMultAdd += 0.5f;
            }
        }

        public sealed class ExeSuperChargeBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => _buffExeSuperCharged;
            private float timer;

            public void FixedUpdate()
            {
                if (NetworkServer.active && hasAnyStacks)
                {
                    if (characterBody.baseNameToken != "SS2_EXECUTIONER2_NAME" || characterBody.HasBuff(_buffExeMuteCharge))
                        return;
                    else
                        timer += Time.fixedDeltaTime;

                    if (timer >= 0.2f && characterBody.skillLocator.secondary.stock < characterBody.skillLocator.secondary.maxStock)
                    {
                        timer = 0f;

                        characterBody.skillLocator.secondary.AddOneStock();

                        if (characterBody.skillLocator.secondary.stock < characterBody.skillLocator.secondary.maxStock)
                        {
                            Util.PlaySound("ExecutionerGainCharge", gameObject);
                            EffectManager.SimpleMuzzleFlash(plumeEffect, gameObject, "ExhaustL", true);
                            EffectManager.SimpleMuzzleFlash(plumeEffect, gameObject, "ExhaustR", true);
                        }
                        if (characterBody.skillLocator.secondary.stock >= characterBody.skillLocator.secondary.maxStock)
                        {
                            Util.PlaySound("ExecutionerMaxCharge", gameObject);
                            EffectManager.SimpleMuzzleFlash(plumeEffectLarge, gameObject, "ExhaustL", true);
                            EffectManager.SimpleMuzzleFlash(plumeEffectLarge, gameObject, "ExhaustR", true);
                            EffectManager.SimpleEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/LightningFlash.prefab").WaitForCompletion(), characterBody.corePosition, Quaternion.identity, false);
                        }

                        characterBody.SetAimTimer(1.6f);
                    }
                }
            }
        }
    }
}