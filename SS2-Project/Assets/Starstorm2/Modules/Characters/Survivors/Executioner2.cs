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
using MSU.Config;
using RoR2.Skills;
using System.Runtime.CompilerServices;
namespace SS2.Survivors
{
    public sealed class Executioner2 : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acExecutioner2", SS2Bundle.Executioner2);

        static string path = "Prefabs/Effects/OrbEffects/LightningOrbEffect";

        public static GameObject plumeEffect;
        public static GameObject plumeEffectLarge;
        public static GameObject taserVFX;
        public static GameObject taserVFXMastery;
        public static GameObject fearEffectPrefab;
        public static GameObject fearEffectPrefabMastery;
        public static GameObject executeEffectPrefab;
        public static GameObject executeEffectPrefabMastery;
        public static GameObject crippleEffectPrefabMastery;
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

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR, configDescOverride = "Use Ion Manipulators' alternate camera position by default.")]
        public static bool DefaultAltCameraConfig = false;

        private static float fearExecuteThresholdAdditive = (1f / 0.85f) - 1f;   //0.15f with ExecuteAPI

        public static void AddBodyToSuperChargeCollection(string body)
        {
            bodiesThatGiveSuperCharge.Add(body);
            UpdateSuperChargeList();
        }

        public override void Initialize()
        {
            plumeEffect = AssetCollection.FindAsset<GameObject>("exePlume");
            plumeEffectLarge = AssetCollection.FindAsset<GameObject>("exePlumeBig");
            fearEffectPrefab = AssetCollection.FindAsset<GameObject>("ExecutionerFearEffect");
            fearEffectPrefabMastery = AssetCollection.FindAsset<GameObject>("ExecutionerFearEffectMastery");
            executeEffectPrefab = AssetCollection.FindAsset<GameObject>("ExecutionerExecuteEffect");
            executeEffectPrefabMastery = AssetCollection.FindAsset<GameObject>("ExecutionerExecuteEffectMastery");
            crippleEffectPrefabMastery = AssetCollection.FindAsset<GameObject>("ExecutionerCrippleMastery");

            BodyCatalog.availability.CallWhenAvailable(UpdateSuperChargeList);
            R2API.RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
            SetupFearExecute();
            ModifyPrefab();

            taserVFX = AssetCollection.FindAsset<GameObject>("TaserOrbEffect");
            taserVFXMastery = AssetCollection.FindAsset<GameObject>("TaserOrbEffectMastery");

            IL.RoR2.Orbs.OrbEffect.Reset += OrbEffect_Reset; // :3

            if (SS2Main.ScepterInstalled)
            {
                ScepterCompat();
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void ScepterCompat()
        {
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("sdExe2SlamScepter", SS2Bundle.Executioner2), "Executioner2Body", SkillSlot.Special, 0);
        }
        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if(sender.HasBuff(SS2Content.Buffs.BuffExecutionerArmor))
            {
                args.armorAdd += 60f;
            }
            if(sender.HasFearBuff())
            {
                args.moveSpeedReductionMultAdd += 0.33f;
            }

            int consecrationStack = sender.GetBuffCount(SS2Content.Buffs.bdConsecration);
            if(consecrationStack > 0)
            {
                //args.attackSpeedMultAdd += 0.08f * consecrationStack;
                args.moveSpeedMultAdd += 0.05f * consecrationStack;
            }

            int bloodRushStack = sender.GetBuffCount(SS2Content.Buffs.bdBloodRush);
            if (bloodRushStack > 0)
            {
                args.attackSpeedMultAdd += 0.08f * bloodRushStack;
                args.moveSpeedMultAdd += 0.08f * bloodRushStack;
            }
        }

        private void OrbEffect_Reset(MonoMod.Cil.ILContext il)
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
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(OrbEffect).GetFieldCached(nameof(OrbEffect._effectComponent)));
                cursor.EmitDelegate<Func<Vector3, Transform, EffectComponent, Vector3>>((startPosition, targetTransform, effectComponent) =>
                {
                    if (targetTransform == null)
                    {
                        startPosition = effectComponent.effectData.start;
                    }
                    return startPosition;
                });
                SS2Log.Info("Added OrbEffect_Reset hook :D");
            }
            else
            {
                SS2Log.Fatal("OrbEffect_Reset hook failed.");
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

        public void ModifyPrefab()
        {
            SetupDefaultBody(CharacterPrefab);
        }


        private static float fearExecutionThreshold = 0.5f;
        private HealthComponent.HealthBarValues FearExecuteHealthbar(On.RoR2.HealthComponent.orig_GetHealthBarValues orig, HealthComponent self)
        {
            var hbv = orig(self);
            if (!self.body.bodyFlags.HasFlag(CharacterBody.BodyFlags.ImmuneToExecutes) && self.body.HasFearBuff())
            {
                hbv.cullFraction += fearExecutionThreshold;//might stack too crazy if it's 30% like Freeze
            }
            return hbv;
        }

        private void FearExecuteThreshold(CharacterBody victimBody, ref float executeFraction)
        {
            if ((victimBody.HasBuff(SS2Content.Buffs.BuffFear) || victimBody.HasBuff(SS2Content.Buffs.BuffFearRed)) && executeFraction <= fearExecutionThreshold) 
            {
                executeFraction = fearExecutionThreshold;
            }
        }

        private void FearExecuteThresholdAdditive(CharacterBody victimBody, ref float executeFractionAdd)
        {
            if (victimBody.HasBuff(SS2Content.Buffs.BuffFear))
            {
                executeFractionAdd += fearExecuteThresholdAdditive;
            }
        }

        private void SetupFearExecute() //thanks moffein, hope you're doing well ★
        {
        
            On.RoR2.HealthComponent.GetHealthBarValues += FearExecuteHealthbar;

            // Thanks Moffein for the code!
            // Taken from: https://github.com/Moffein/Starstorm2Unofficial/blob/9e3bca7b23277dd942de1198f1bc0b8c55649db6/Starstorm%202/Survivors/Executioner/ExecutionerCore.cs#L632
            if (SS2Main.SS2UInstalled)
            {
                SS2Log.Info("Video Game Mod 2 Detected. Adding Additive execute hook");
                R2API.ExecuteAPI.CalculateAdditiveExecuteThreshold += FearExecuteThresholdAdditive;
            }
            else
            {
                R2API.ExecuteAPI.CalculateExecuteThreshold += FearExecuteThreshold;
            }


            // TODO: This should be good to remove with new ExecuteAPI
            //IL.RoR2.HealthComponent.TakeDamageProcess += (il) =>
            //{
            //    bool error = true;
            //    ILCursor c = new ILCursor(il);

            //    if (c.TryGotoNext(x => x.MatchLdloc(74), x => x.MatchLdcR4(0)))
            //    {
            //        c.Index++;
            //        c.Emit(OpCodes.Ldarg_0);//self
            //        c.EmitDelegate<Func<float, HealthComponent, float>>((executeFraction, self) =>
            //        {
            //            if (self.body.HasFearBuff())
            //            {
            //                if (executeFraction < 0f) executeFraction = 0f;
            //                executeFraction += fearExecutionThreshold;
            //            }
            //            return executeFraction;
            //        });

            //        if (c.TryGotoNext(x => x.MatchLdloc(74)))
            //        {
            //            c.Index++;
            //            c.Emit(OpCodes.Ldarg_0);//self
            //            c.EmitDelegate<Func<float, HealthComponent, float>>((executeFraction, self) =>
            //            {
            //                if (self.body.HasFearBuff())
            //                {
            //                    if (executeFraction < 0f) executeFraction = 0f;
            //                    executeFraction += fearExecutionThreshold;
            //                }
            //                return executeFraction;
            //            });
            //            error = false;
            //        }
            //    }

            //    if (error)
            //    {
            //        SS2Log.Fatal("Starstorm 2: Fear Execute IL Hook failed.");
            //    }

            //};

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
                        return hasBuff || self.HasFearBuff();
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
            if (BodiesThatGiveSuperCharge.Contains(body.bodyIndex))
                return 100;

            if (body == null) return 1;
            if (body.bodyIndex == BodyIndex.None) return 1;

            int value = 1;
            

            switch (body.hullClassification)
            {
                case HullClassification.Human:
                    value = 1;
                    break;
                case HullClassification.Golem:
                    value = 2;
                    break;
                case HullClassification.BeetleQueen:
                    value = 2;
                    break;
                default:
                    value = 1;
                    break;
            }
            if (body.isChampion)
            {
                value = 5;
            }
            if (body.isElite)
            {
                value *= 2;
            }
            return value;
        }
        public sealed class FearBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffFear;

            private GameObject effectInstance;
            private static string activationSoundString = "Play_voidman_R_pop";
            private Collider bodyCollider;
            private int previousBuffCount;
            private bool hasDied;
            public bool inMasterySkin;
            private void OnEnable()
            {
                previousBuffCount = buffCount;
                OnStackGained();
                if(!bodyCollider)
                    bodyCollider = base.GetComponent<Collider>();
            }
            private void OnDisable()
            {
                Destroy(effectInstance);
            }

            private void FixedUpdate()
            {
                if(buffCount > previousBuffCount)
                {
                    OnStackGained();
                }
                if (!base.characterBody.healthComponent.alive && !hasDied)
                {
                    hasDied = true;
                    // im DUMB and LAZY and FORGOT HOW TO ILHOOK
                    EffectManager.SpawnEffect(
                        inMasterySkin ? executeEffectPrefabMastery : executeEffectPrefab,
                        new EffectData() { origin = characterBody.corePosition, scale = characterBody.radius }, false);

                    Destroy(this.effectInstance);
                }
                    
                previousBuffCount = buffCount;
            }

            private void OnStackGained()
            {
                if (effectInstance) Destroy(effectInstance);
                
                effectInstance = Instantiate(inMasterySkin ? fearEffectPrefabMastery : fearEffectPrefab, characterBody.coreTransform.position, Quaternion.identity);
                Util.PlaySound(activationSoundString, gameObject); //?????????????
            }

            private void OnDestroy()
            {
                if (hasAnyStacks && !hasDied && characterBody && !characterBody.healthComponent.alive) // fuck wisps!!
                {
                    EffectManager.SpawnEffect(
                        inMasterySkin ? executeEffectPrefabMastery : executeEffectPrefab,
                        new EffectData() { origin = characterBody.corePosition, scale = characterBody.radius }, false);
                }
            }

            private void Update()
            {
                if (effectInstance)
                {
                    Vector3 a = base.transform.position;
                    if (this.bodyCollider)
                    {
                        a = this.bodyCollider.bounds.center + new Vector3(0f, this.bodyCollider.bounds.extents.y, 0f);
                    }
                    effectInstance.transform.position = a;
                }
            }
        }

        public sealed class ExeSuperChargeBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffExecutionerSuperCharged;
            private float timer;

            public void FixedUpdate()
            {
                if (NetworkServer.active && hasAnyStacks)
                {
                    timer += Time.fixedDeltaTime;

                    if (timer >= 0.2f && characterBody.skillLocator.secondary.stock < characterBody.skillLocator.secondary.maxStock)
                    {
                        timer -= 0.2f;

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
                    }
                }
            }
        }
    }
}
