using Moonstorm.Loaders;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Linq;
using UnityEngine;

namespace Moonstorm.Starstorm2
{
    public class SS2Content : ContentLoader<SS2Content>
    {
        public static class Artifacts
        {
            public static ArtifactDef Cognation;
        }
        public static class Items
        {
            public static ItemDef ArmedBackpack;

            public static ItemDef CoffeeBag;

            public static ItemDef Cognation;

            public static ItemDef DetritiveTrematode;

            public static ItemDef Diary;

            public static ItemDef DormantFungus;

            public static ItemDef Fork;

            public static ItemDef Malice;

            public static ItemDef MoltenCoin;

            public static ItemDef Needles;

            public static ItemDef NemBossHelper;

            public static ItemDef BloodTester;

            public static ItemDef FieldAccelerator;

            public static ItemDef HottestSauce;

            public static ItemDef HuntersSigil;

            public static ItemDef JetBoots;

            public static ItemDef Roulette;

            public static ItemDef StrangeCan;

            public static ItemDef WatchMetronome;

            public static ItemDef DroidHead;

            public static ItemDef ErraticGadget;

            public static ItemDef GreenChocolate;

            public static ItemDef NkotasHeritage;

            public static ItemDef PortableReactor;

            public static ItemDef SwiftSkateboard;

            public static ItemDef LowQualitySpeakers;

            public static ItemDef Augury;

            public static ItemDef ScavengersFortune;

            public static ItemDef ShackledLamp;

            public static ItemDef StirringSoul;

            public static ItemDef RelicOfDuality;

            public static ItemDef RelicOfExtinction;

            public static ItemDef RelicOfForce;

            public static ItemDef RelicOfMass;

            public static ItemDef RelicOfTermination;

            public static ItemDef RelicOfTermination2;

            public static ItemDef RelicOfEchelon;

            public static ItemDef Insecticide;

            public static ItemDef BabyToys;

            public static ItemDef X4;

            public static ItemDef NemesisBossHelper;

            public static ItemDef TerminationHelper;
        }

        public static class Equipments
        {
            public static EquipmentDef ElitePurpleEquipment;

            public static EquipmentDef BackThruster;

            public static EquipmentDef CloakingHeadband;

            public static EquipmentDef GreaterWarbanner;

            public static EquipmentDef MIDAS;

            public static EquipmentDef PressurizedCanister;
        }

        public static class Buffs
        {
            public static BuffDef BuffAffixVoid;

            public static BuffDef BuffBackThruster;

            public static BuffDef BuffChirrAlly;

            public static BuffDef BuffChirrFly;

            public static BuffDef BuffChirrRoot;

            public static BuffDef BuffChirrSlow;

            public static BuffDef BuffChocolate;

            public static BuffDef BuffExecutionerSuperCharged;

            public static BuffDef BuffExecutionerArmor;

            public static BuffDef BuffFear;

            public static BuffDef BuffGouge;

            public static BuffDef BuffGreaterBanner;

            public static BuffDef BuffIntoxicated;

            public static BuffDef BuffCoffeeBag;

            public static BuffDef BuffNeedle;

            public static BuffDef BuffNeedleBuildup;

            public static BuffDef BuffNucleatorSpecial;

            public static BuffDef BuffReactor;

            public static BuffDef BuffScavengersFortune;

            public static BuffDef BuffSigil;

            public static BuffDef BuffKickflip;

            public static BuffDef BuffStorm;

            public static BuffDef BuffTrematodes;

            public static BuffDef BuffVoidLeech;

            public static BuffDef BuffWatchMetronome;

            public static BuffDef BuffWealth;

            public static BuffDef BuffSurrender;

            public static BuffDef BuffTerminationVFX;

            public static BuffDef BuffX4;

            public static BuffDef bdElitePurple;

            public static BuffDef bdPurplePoison;
			
            public static BuffDef BuffTerminationCooldown;

            public static BuffDef BuffTerminationReady;

            public static BuffDef BuffEchelon;
        }

        public static class Elites
        {
            //public static EliteDef edPurple;
        }

        public static class Survivors
        {
            //public static SurvivorDef SurvivorBorg;

            //public static SurvivorDef SurvivorChirr;

            public static SurvivorDef SurvivorExecutioner;

            //public static SurvivorDef SurvivorNemExe;

            public static SurvivorDef SurvivorNemmando;

            public static SurvivorDef survivorNemCommando;

            //public static SurvivorDef SurvivorPyro;
        }

        public static class ItemTierDefs
        {
            public static ItemTierDef Sibylline;
        }
        public override string identifier => Starstorm.guid;

        public override R2APISerializableContentPack SerializableContentPack { get; protected set; } = SS2Assets.LoadAsset<R2APISerializableContentPack>("ContentPack", SS2Bundle.Main);
        public override Action[] LoadDispatchers { get; protected set; }

        public override Action[] PopulateFieldsDispatchers { get; protected set; }

        public override void Init()
        {
            base.Init();

            LoadDispatchers = new Action[]
            {
                delegate
                {
                    new Modules.Scenes().Initialize();
                },
                delegate
                {
                    new Modules.ItemTiers().Initialize();
                },
                delegate
                {
                    new Modules.Items().Initialize();
                },
                delegate
                {
                    new Modules.Equipments().Initialize();
                },
                delegate
                {
                    new Modules.Buffs().Initialize();
                },
                delegate
                {
                    new Modules.DamageTypes().Initialize();
                },
                delegate
                {
                    new Modules.Projectiles().Initialize();
                },
                delegate
                {
                    new Modules.Elites().Initialize();
                },
                delegate
                {
                    Typhoon.Init();
                },
                delegate
                {
                    if(SS2Config.EnableEvents.Value)
                    {
                        Events.Init();
                    }
                },
                delegate
                {
                    new Modules.Characters().Initialize();
                },
                delegate
                {
                    new Modules.Artifacts().Initialize();
                },
                delegate
                {
                    new Modules.Interactables().Initialize();
                },
                delegate
                {
                    new Modules.Unlockables().Initialize();
                },
                delegate
                {
                    SS2Log.Info($"Populating entity state array");
                    GetType().Assembly.GetTypes()
                                      .Where(type => typeof(EntityStates.EntityState).IsAssignableFrom(type))
                                      .ToList()
                                      .ForEach(state => HG.ArrayUtils.ArrayAppend(ref SerializableContentPack.entityStateTypes, new EntityStates.SerializableEntityStateType(state)));
                },
                delegate
                {
                    SS2Log.Info($"Populating EntityStateConfigurations");
                    SerializableContentPack.entityStateConfigurations = SS2Assets.LoadAllAssetsOfType<EntityStateConfiguration>(SS2Bundle.All);
                },
                delegate
                {
                    SS2Log.Info($"Populating effect prefabs");
                    SerializableContentPack.effectPrefabs = SerializableContentPack.effectPrefabs.Concat(SS2Assets.LoadAllAssetsOfType<GameObject>(SS2Bundle.All)
                    .Where(go => go.GetComponent<EffectComponent>()))
                    .ToArray();
                },
                delegate
                {
                    SS2Log.Info($"Swapping material shaders");
                    SS2Assets.Instance.SwapMaterialShaders();
                    SS2Assets.Instance.FinalizeCopiedMaterials();
                }
            };

            PopulateFieldsDispatchers = new Action[]
            {
                delegate
                {
                    PopulateTypeFields(typeof(Artifacts), ContentPack.artifactDefs);
                },
                delegate
                {
                    PopulateTypeFields(typeof(Items), ContentPack.itemDefs);
                },
                delegate
                {
                    PopulateTypeFields(typeof(Equipments), ContentPack.equipmentDefs);
                },
                delegate
                {
                    PopulateTypeFields(typeof(Buffs), ContentPack.buffDefs);
                },
                delegate
                {
                    PopulateTypeFields(typeof(Elites), ContentPack.eliteDefs);
                },
                delegate
                {
                    PopulateTypeFields(typeof(Survivors), ContentPack.survivorDefs);
                },
                delegate
                {
                    PopulateTypeFields(typeof(ItemTierDefs), ContentPack.itemTierDefs);
                }
            };
        }
    }
}