using RoR2.ExpansionManagement;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using MSU;
using RoR2;

namespace SS2
{
    public class SS2Content : IContentPackProvider
    {
        public string identifier => SS2Main.GUID;
        public static ReadOnlyContentPack ReadOnlyContentPack => new ReadOnlyContentPack(SS2ContentPack);
        internal static ContentPack SS2ContentPack { get; private set; } = new ContentPack();

        internal static ParallelMultiStartCoroutine _parallelPreLoadDispatchers = new ParallelMultiStartCoroutine();
        private static Func<IEnumerator>[] _loadDispatchers;
        internal static ParallelMultiStartCoroutine _parallelPostLoadDispatchers = new ParallelMultiStartCoroutine();

        private static Func<IEnumerator>[] _fieldAssignDispatchers;
        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            var enumerator = SS2Assets.Initialize();
            while (enumerator.MoveNext())
                yield return null;

            //Loadbearing Spike
            if (!SS2Assets.LoadAsset<Texture2D>("spike", SS2Bundle.Main))
            {
                SS2Log.Fatal("MISSSING LOAD BEARING SPIKE");
                yield break;
            }

            _parallelPreLoadDispatchers.Start();
            while (!_parallelPreLoadDispatchers.IsDone()) yield return null;

            for (int i = 0; i < _loadDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _loadDispatchers.Length, 0.1f, 0.2f));
                enumerator = _loadDispatchers[i]();

                while (enumerator?.MoveNext() ?? false) yield return null;
            }

            _parallelPostLoadDispatchers.Start();
            while (!_parallelPostLoadDispatchers.IsDone()) yield return null;

            for (int i = 0; i < _fieldAssignDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _fieldAssignDispatchers.Length, 0.95f, 0.99f));
                enumerator = _fieldAssignDispatchers[i]();

                while (enumerator?.MoveNext() ?? false) yield return null;
            }
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(SS2ContentPack, args.output);
            args.ReportProgress(1f);
            yield return null;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        private static IEnumerator LoadFromAssetBundles()
        {
            SS2Log.Info($"Populating EntityStateTypes array...");
            SS2ContentPack.entityStateTypes.Clear();
            SS2ContentPack.entityStateTypes.Add(typeof(SS2Content).Assembly.GetTypes().Where(type => typeof(EntityStates.EntityState).IsAssignableFrom(type)).ToArray());

            SS2Log.Info("Populating EntityStateConfiguration array...");
            SS2AssetRequest<EntityStateConfiguration> escRequest = new SS2AssetRequest<EntityStateConfiguration>(SS2Bundle.All);
            escRequest.StartLoad();
            while (!escRequest.IsComplete) yield return null;
            SS2ContentPack.entityStateConfigurations.Clear();
            SS2ContentPack.entityStateConfigurations.Add(escRequest.Assets.ToArray());

            SS2Log.Info($"Populating EffectDefs array...");
            SS2AssetRequest<GameObject> gameObjectRequest = new SS2AssetRequest<GameObject>(SS2Bundle.All);
            gameObjectRequest.StartLoad();
            while (!gameObjectRequest.IsComplete) yield return null;
            SS2ContentPack.effectDefs.Clear();
            SS2ContentPack.effectDefs.Add(gameObjectRequest.Assets.Where(go => go.GetComponent<EffectComponent>()).Select(go => new EffectDef(go)).ToArray());

            SS2Log.Info($"Calling AsyncAssetLoad Attribute Methods...");
            ParallelMultiStartCoroutine asyncAssetLoadCoroutines = AsyncAssetLoadAttribute.CreateCoroutineForMod(SS2Main.Instance);
            asyncAssetLoadCoroutines.Start();
            while (!asyncAssetLoadCoroutines.isDone)
                yield return null;
        }

        private IEnumerator AddSS2ExpansionDef()
        {
            var expansionRequest = SS2Assets.LoadAssetAsync<ExpansionDef>("SS2ExpansionDef", SS2Bundle.Main);
            expansionRequest.StartLoad();

            while (!expansionRequest.IsComplete)
                yield return null;

            SS2ContentPack.expansionDefs.AddSingle(expansionRequest.Asset);
        }

        internal SS2Content()
        {
            ContentManager.collectContentPackProviders += AddSelf;
            SS2Assets.assetsAvailability.CallWhenAvailable(() =>
            {
                _parallelPreLoadDispatchers.Add(AddSS2ExpansionDef);
            });
        }

        static SS2Content()
        {
            SS2Main main = SS2Main.Instance;
            _loadDispatchers = new Func<IEnumerator>[]
            {
                DifficultyModule.Init,
                Events.Init,
                //Bulwark.Init,
                //Components.EtherealBehavior.Init,
                Components.VoidBehavior.Init,
                //Void.Init,
                Storm.Init,
                //RulebookEnabler.Init,
                () =>
                {
                    //new Modules.Scenes().Initialize();
                    return null;
                },
                () =>
                {
                    CharacterModule.AddProvider(main, ContentUtil.CreateGameObjectGenericContentPieceProvider<CharacterBody>(main, SS2ContentPack));
                    return CharacterModule.InitializeCharacters(main);
                },
                () =>
                {
                    ItemTierModule.AddProvider(main, ContentUtil.CreateGenericContentPieceProvider<ItemTierDef>(main, SS2ContentPack));
                    return ItemTierModule.InitializeTiers(main);
                },
                () =>
                {
                    ItemModule.AddProvider(main, ContentUtil.CreateGenericContentPieceProvider<ItemDef>(main, SS2ContentPack));
                    return ItemModule.InitializeItems(main);
                },
                () =>
                {
                    EquipmentModule.AddProvider(main, ContentUtil.CreateGenericContentPieceProvider<EquipmentDef>(main, SS2ContentPack));
                    return EquipmentModule.InitializeEquipments(main);
                },
                () =>
                {
                    ArtifactModule.AddProvider(main, ContentUtil.CreateGenericContentPieceProvider<ArtifactDef>(main, SS2ContentPack));
                    return ArtifactModule.InitializeArtifacts(main);
                },
                () =>
                {
                    InteractableModule.AddProvider(main, ContentUtil.CreateGameObjectGenericContentPieceProvider<IInteractable>(main, SS2ContentPack));
                    return InteractableModule.InitializeInteractables(main);
                },
                () =>
                {
                    SceneModule.AddProvider(main, ContentUtil.CreateGenericContentPieceProvider<SceneDef>(main, SS2ContentPack));
                    return SceneModule.InitializeScenes(main);
                },
                () =>
                {
                    VanillaSurvivorModule.AddProvider(main, ContentUtil.CreateContentPieceProvider<IVanillaSurvivorContentPiece>(main, SS2ContentPack));
                    return VanillaSurvivorModule.InitializeVanillaSurvivorContentPieces(main);
                },
                LoadFromAssetBundles,
            };

            _fieldAssignDispatchers = new Func<IEnumerator>[]
            {
                () =>
                {
                    ContentUtil.PopulateTypeFields(typeof(Artifacts), SS2ContentPack.artifactDefs);
                    return null;
                },
                () =>
                {
                    ContentUtil.PopulateTypeFields(typeof(Items), SS2ContentPack.itemDefs);
                    return null;
                },
                () =>
                {
                    ContentUtil.PopulateTypeFields(typeof(Equipments), SS2ContentPack.equipmentDefs);
                    return null;
                },
                () =>
                {
                    ContentUtil.PopulateTypeFields(typeof(Elites), SS2ContentPack.eliteDefs);
                    return null;
                },
                () =>
                {
                    ContentUtil.PopulateTypeFields(typeof(Buffs), SS2ContentPack.buffDefs);
                    return null;
                },
                () =>
                {
                    ContentUtil.PopulateTypeFields(typeof(Scenes), SS2ContentPack.sceneDefs);
                    return null;
                },
                () =>
                {
                    ContentUtil.PopulateTypeFields(typeof(Survivors), SS2ContentPack.survivorDefs);
                    return null;
                },
                () =>
                {
                    ContentUtil.PopulateTypeFields(typeof(ItemTierDefs), SS2ContentPack.itemTierDefs);
                    return null;
                }
            };
        }

        public static class Artifacts
        {
            public static ArtifactDef Cognation;

            public static ArtifactDef Havoc;

            public static ArtifactDef Deviation;

            public static ArtifactDef Adversity;
        }
        public static class Items
        {
            public static ItemDef AffixStorm;

            public static ItemDef AffixUltra;

            public static ItemDef ArmedBackpack;

            public static ItemDef BoostCharacterSize;

            public static ItemDef BoostCooldowns;

            public static ItemDef BoostMovespeed;

            public static ItemDef CoffeeBag;

            public static ItemDef CognationHelper;

            public static ItemDef CompositeInjector;

            public static ItemDef ChirrFriendHelper;

            public static ItemDef CrypticSource;

            public static ItemDef DetritiveTrematode;

            public static ItemDef Diary;

            public static ItemDef DiaryConsumed;

            public static ItemDef DormantFungus;

            public static ItemDef DoubleAllStats;

            public static ItemDef EtherealItemAffix;

            public static ItemDef Fork;

            public static ItemDef Malice;

            public static ItemDef MoltenCoin;

            public static ItemDef Needles;

            public static ItemDef NemBossHelper;

            public static ItemDef VoidRock;

            public static ItemDef VoidRockTracker;

            public static ItemDef BloodTester;

            public static ItemDef FieldAccelerator;

            public static ItemDef FlowerTurret;

            public static ItemDef HealthDecayWithRegen;

            public static ItemDef HottestSauce;

            public static ItemDef HuntersSigil;

            public static ItemDef JetBoots;

            public static ItemDef Roulette;

            public static ItemDef StrangeCan;

            public static ItemDef WatchMetronome;

            public static ItemDef DroidHead;

            public static ItemDef ErraticGadget;

            public static ItemDef GreenChocolate;

            public static ItemDef GuardingAmulet;

            public static ItemDef NkotasHeritage;

            public static ItemDef PortableReactor;

            public static ItemDef SwiftSkateboard;

            public static ItemDef LightningOnKill;

            public static ItemDef LowQualitySpeakers;

            public static ItemDef Augury;

            public static ItemDef SantaHat;

            public static ItemDef ScavengersFortune;

            public static ItemDef ShackledLamp;

            public static ItemDef StirringSoul;

            public static ItemDef Remuneration;

            public static ItemDef RelicOfDuality;

            public static ItemDef RelicOfExtinction;

            public static ItemDef RelicOfForce;

            public static ItemDef RelicOfMass;

            public static ItemDef RelicOfTermination;

            public static ItemDef RelicOfTerminationOld;

            public static ItemDef RelicOfEchelon;

            public static ItemDef Insecticide;

            public static ItemDef BabyToys;

            public static ItemDef X4;

            public static ItemDef BaneFlask;

            public static ItemDef TerminationHelper;

            public static ItemDef GildedAmulet;

            public static ItemDef ChucklingFungus;

            public static ItemDef UniversalCharger;

            public static ItemDef UraniumHorseshoe;

            public static ItemDef RainbowRoot;

            public static ItemDef Balloon;

            public static ItemDef VoidBalloon;

            public static ItemDef RelicOfEntropy;

            public static ItemDef IceTool;

            // public static ItemDef WickedStaff;
            public static ItemDef WeatherRadio;

            public static ItemDef PrimalBirthright;

            public static ItemDef GalvanicCore;

        }

        public static class Equipments
        {
            public static EquipmentDef EliteSuperLightningEquipment;

            public static EquipmentDef EliteSuperIceEquipment;

            public static EquipmentDef EliteSuperFireEquipment;

            public static EquipmentDef EliteSuperEarthEquipment;

            public static EquipmentDef ElitePurpleEquipment;

            public static EquipmentDef EliteKineticEquipment;

            public static EquipmentDef AffixEthereal;

            public static EquipmentDef AffixEmpyrean;

            public static EquipmentDef equipDivineRight;

            public static EquipmentDef BackThruster;

            public static EquipmentDef CloakingHeadband;

            public static EquipmentDef GreaterWarbanner;

            public static EquipmentDef Magnet;

            public static EquipmentDef MIDAS;

            public static EquipmentDef PressurizedCanister;

            public static EquipmentDef RockFruit;

            public static EquipmentDef WhiteFlag;

            public static EquipmentDef SeismicOscillator;
        }

        public static class Buffs
        {
            public static BuffDef BuffAffixSuperLightning;

            public static BuffDef BuffAffixSuperIce;

            public static BuffDef BuffAffixSuperFire;

            public static BuffDef BuffAffixSuperEarth;

            public static BuffDef BuffAffixStorm;

            public static BuffDef BuffAffixVoid;

            public static BuffDef BuffAffixUltra;

            public static BuffDef BuffBloodTesterRegen;

            public static BuffDef BuffBackThruster;

            public static BuffDef BuffUniversalCharger;

            public static BuffDef BuffChirrConfuse;

            public static BuffDef BuffChirrConvert;

            public static BuffDef BuffChirrFriend;

            public static BuffDef BuffChirrGrabFriend;

            public static BuffDef BuffChirrRegen;

            public static BuffDef BuffCyborgPrimed;

            public static BuffDef BuffChocolate;

            public static BuffDef BuffExecutionerSuperCharged;

            public static BuffDef BuffExecutionerArmor;

            public static BuffDef BuffFear;

            public static BuffDef BuffFearRed;

            public static BuffDef BuffGouge;

            public static BuffDef bdHakai;

            public static BuffDef BuffGreaterBanner;

            public static BuffDef BuffIntoxicated;

            public static BuffDef BuffInsecticide;

            public static BuffDef BuffJetBootsCooldown;

            public static BuffDef BuffJetBootsReady;

            public static BuffDef BuffCoffeeBag;

            public static BuffDef BuffNeedleBuildup;

            public static BuffDef BuffNucleatorSpecial;

            public static BuffDef BuffReactor;

            public static BuffDef BuffScavengersFortune;

            public static BuffDef BuffSigil;

            public static BuffDef BuffSigilStack;

            public static BuffDef BuffKickflip;

            public static BuffDef BuffStorm;

            public static BuffDef BuffTrematodes;

            public static BuffDef BuffWatchMetronome;

            public static BuffDef BuffWealth;

            public static BuffDef BuffSurrender;

            public static BuffDef BuffX4;

            public static BuffDef bdElitePurple;

            public static BuffDef bdPurplePoison;

            public static BuffDef bdEliteKinetic;

            public static BuffDef bdHiddenSlow20;

            public static BuffDef bdHiddenSpeed5;

            public static BuffDef bdMULENet;

            public static BuffDef bdExeCharge;

            public static BuffDef bdCanJump;

            public static BuffDef bdExeMuteCharge;

            public static BuffDef BuffTerminationCooldown;

            public static BuffDef BuffTerminationReady;

            public static BuffDef BuffTerminationFailed;

            public static BuffDef BuffTerminationVFX;

            public static BuffDef BuffEchelon;

            public static BuffDef BuffBane;

            public static BuffDef BuffRiposte;

            public static BuffDef BuffCyborgTeleporter;

            public static BuffDef BuffBloonTrap;

            public static BuffDef bdOverstress;

            public static BuffDef bdNemCapDroneBuff;

            public static BuffDef bdShield;

            public static BuffDef bdParry;

            public static BuffDef bdKnightBuff;

            public static BuffDef bdKnightSpecialPowerBuff;

            public static BuffDef bdKnightSpecialBuff;

            public static BuffDef bdKnightSpecialSlowBuff;

            public static BuffDef bdKnightStunAttack;

            public static BuffDef bdKnightShieldCooldown;

            public static BuffDef bdLampBuff;

            public static BuffDef bdNemCapManaReduction;

            public static BuffDef bdNemCapManaRegen;

            public static BuffDef bdEmpyrean;

            public static BuffDef bdPoisonBuildup;

            public static BuffDef bdEthereal;

            public static BuffDef bdBanditTranquilizer;

            public static BuffDef bdAcridArmorCorrison;

            public static BuffDef bdEngiFocused;

            public static BuffDef bdLunarCurseArmor;

            public static BuffDef bdLunarCurseAttackSpeed;

            public static BuffDef bdLunarCurseCloak;

            public static BuffDef bdLunarCurseCooldownReduction;

            public static BuffDef bdLunarCurseDamage;

            public static BuffDef bdLunarCurseHealth;

            public static BuffDef bdLunarCurseMovementSpeed;

            public static BuffDef bdLunarCurseShield;

            public static BuffDef bdLunarCurseBlind;

            public static BuffDef bdLunarCurseLockSkill;

            public static BuffDef bdLunarCurseNoRegen;

            public static BuffDef bdNukeSpecial;

            public static BuffDef bdNukeSelfDamage;

            public static BuffDef bdIrradiated;

            public static BuffDef dbdNuclearSickness;

            public static BuffDef bdUltra;

            public static BuffDef bdUltraBuff;

            public static BuffDef bdPyroPressure;

            public static BuffDef bdPyroManiac;

            public static BuffDef bdPyroJet;

            public static BuffDef bdOil;

            public static BuffDef bdGalvanized;

            public static BuffDef bdGalvanizedSource;

            public static BuffDef bdConsecration;

            public static BuffDef bdBloodRush;

        }

        public static class Elites
        {
            public static EliteDef edSuperLightning;

            public static EliteDef edSuperIce;

            public static EliteDef edSuperFire;

            public static EliteDef edSuperEarth;

            public static EliteDef edStorm;

            public static EliteDef edPurple;

            public static EliteDef edKinetic;

            public static EliteDef edEmpyrean;

            public static EliteDef edEthereal;
        }
        public static class Scenes
        {
            public static SceneDef ss2_voidshop;
        }
        public static class Survivors
        {
            public static SurvivorDef Chirr;

            public static SurvivorDef survivorExecutioner2;

            public static SurvivorDef survivorNemCommando;

            public static SurvivorDef survivorNemCaptain;

            public static SurvivorDef survivorKnight;

            public static SurvivorDef NemMerc;
        }

        public static class ItemTierDefs
        {
            public static ItemTierDef Sibylline;

            //public static ItemTierDef Relic;
        }
    }
}