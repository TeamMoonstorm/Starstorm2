/*using BepInEx.Configuration;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using RoR2.UI;
using Moonstorm.Starstorm2.Components;
using Moonstorm.Starstorm2.Cores;
using Moonstorm.Starstorm2.States.Nucleator;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;


namespace Moonstorm.Starstorm2.Survivors
{
    internal class Nucleator : SurvivorBaseOld
    {
        internal override string bodyName { get; set; } = "Nucleator";

        internal override GameObject bodyPrefab { get; set; }
        internal override GameObject displayPrefab { get; set; }

        internal override float sortPosition { get; set; } = 5.002f;

        internal override ConfigEntry<bool> characterEnabled { get; set; }

        internal override StarstormBodyInfo bodyInfo { get; set; } = new StarstormBodyInfo
        {
            armor = 30f,
            bodyName = "NucleatorBody",
            bodyNameToken = "NUCLEATOR_NAME",
            bodyColor = new Color(0.8039216f, 0.482352942f, 0.843137264f),
            //characterPortrait = Assets.LoadCharacterIcon("Nucleator"),
            //crosshair = Assets.LoadCrosshair("ToolbotGrenadeLauncherCrosshair"),
            damage = 14f,
            healthGrowth = 40f,
            healthRegen = 2f,
            jumpCount = 1,
            maxHealth = 400f,
            subtitleNameToken = "NUCLEATOR_SUBTITLE",
            podPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),
            //cameraParams = CameraParams.NewCameraParams("ccpNucleator", new Vector3(0f, 2.8f, -12.2f))
        };

        internal static Material nucleatorMat = null;
        internal override int mainRendererIndex { get; set; } = 0;
        internal override Type characterMainState { get; set; } = null;//typeof(States.Nemmando.NemmandoMain);

        // item display stuffs
        internal override ItemDisplayRuleSet itemDisplayRuleSet { get; set; }
        internal override List<ItemDisplayRuleSet.KeyAssetRuleGroup> itemDisplayRules { get; set; }

        internal override UnlockableDef characterUnlockableDef { get; set; }
        private static UnlockableDef masterySkinUnlockableDef;
        private static UnlockableDef grandMasterySkinUnlockableDef;

        internal static GameObject chargeCrosshair { get; set; }
        internal static GameObject primaryProjectile { get; set; }

        internal override void InitializeCharacter()
        {
            //Assets.LoadNucleatorEffects();

            base.InitializeCharacter();

            if (characterEnabled.Value)
            {
                bodyPrefab.AddComponent<CustomEffectComponent>();

                CreateUiChargeWidget();
                CreatePrimaryProjectile();
            }
        }

        internal override void InitializeUnlockables()
        {
            //masterySkinUnlockableDef = Unlockables.AddUnlockable<Cores.Unlockables.Achievements.NucleatorMasteryAchievement>(true);
            //grandMasterySkinUnlockableDef = Unlockables.AddUnlockable<Cores.Unlockables.Achievements.NucleatorGrandMasteryAchievement>(true);
        }

        private void CreateUiChargeWidget()
        {
            //chargeCrosshair = Assets.LoadCrosshair("Sniper");

            UnityEngine.Object.Destroy(chargeCrosshair.GetComponent<RawImage>());
            UnityEngine.Object.Destroy(chargeCrosshair.GetComponent<SniperScopeChargeIndicatorController>());
            UnityEngine.Object.Destroy(chargeCrosshair.GetComponent<SniperRangeIndicator>());
            UnityEngine.Object.Destroy(chargeCrosshair.GetComponent<DisplayStock>());
            var chargeController = chargeCrosshair.AddComponent<ChargeController>();

            foreach (Transform child in chargeCrosshair.transform)
            {
                if (child.name == "ChargeIndicator")
                    chargeController.image = child.GetComponent<Image>();
                else if (child.name != "Image")
                {
                    child.SetParent(null);
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }
        }

        private void CreatePrimaryProjectile()
        {
            primaryProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/DaggerProjectile"), "NucleatorProjectile");

            UnityEngine.Object.Destroy(primaryProjectile.GetComponent<ProjectileSingleTargetImpact>());
            UnityEngine.Object.Destroy(primaryProjectile.GetComponent<AwakeEvent>());
            UnityEngine.Object.Destroy(primaryProjectile.GetComponent<DaggerController>());
            UnityEngine.Object.Destroy(primaryProjectile.GetComponent<AwakeEvent>());
            primaryProjectile.AddComponent<NucleatorProjectile>();

            primaryProjectile.GetComponent<ProjectileController>().ghostPrefab = Resources.Load<GameObject>("prefabs/projectileghosts/BeetleQueenSpitGhost");

            var impact = primaryProjectile.AddComponent<ProjectileImpactExplosion>();
            impact.blastAttackerFiltering = AttackerFiltering.Default;
            impact.blastDamageCoefficient = 1f;
            impact.blastProcCoefficient = 1;
            impact.blastRadius = 0f;
            impact.childrenCount = 0;
            impact.childrenDamageCoefficient = 0;
            impact.childrenProjectilePrefab = null;
            impact.destroyOnEnemy = true;
            impact.destroyOnWorld = true;
            impact.falloffModel = BlastAttack.FalloffModel.None;
            impact.fireChildren = false;
            impact.impactEffect = Resources.Load<GameObject>("prefabs/effects/impacteffects/BeetleAcidImpact");
            impact.lifetime = 10;
            impact.lifetimeAfterImpact = 0;
            impact.lifetimeRandomOffset = 0;

            var targetFinder = primaryProjectile.GetComponent<ProjectileDirectionalTargetFinder>();
            targetFinder.lookCone = 90;
            targetFinder.lookRange = 45f;
            targetFinder.allowTargetLoss = true;
            targetFinder.enabled = false;

            var targetTracker = primaryProjectile.GetComponent<ProjectileSteerTowardTarget>();
            targetTracker.rotationSpeed = 45f;
            targetTracker.enabled = false;

            if (primaryProjectile.gameObject.GetComponent<ProjectileController>())
            {
                //Prefabs.projectilePrefabs.Add(primaryProjectile);
            }
        }

        internal override void InitializeSkills()
        {
            Skills.CreateSkillFamilies(bodyPrefab);

            #region Primary
            Skills.AddPrimarySkill(bodyPrefab, Skills.CreatePrimarySkillDef(new SerializableEntityStateType(typeof(ChargeIrradiate)), "Weapon", "NUCLEATOR_PRIMARY_NAME", "NUCLEATOR_PRIMARY_DESCRIPTION", Assets.Instance.MainAssetBundle.LoadAsset<Sprite>("texNucleatorPrimary"), false));
            #endregion

            #region Secondary
            SkillDef secondarySkillDef = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "NUCLEATOR_SECONDARY_NAME",
                skillNameToken = "NUCLEATOR_SECONDARY_NAME",
                skillDescriptionToken = "NUCLEATOR_SECONDARY_DESCRIPTION",
                skillIcon = Assets.Instance.MainAssetBundle.LoadAsset<Sprite>("texNucleatorSecondary"),
                activationState = new SerializableEntityStateType(typeof(ChargeQuarantine)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = 3f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = false,
                interruptPriority = InterruptPriority.Skill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = false,
                cancelSprintingOnActivation = true,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });

            Skills.AddSecondarySkills(bodyPrefab, secondarySkillDef);
            #endregion

            #region Utility
            SkillDef utilitySkillDef = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "NUCLEATOR_UTILITY_NAME",
                skillNameToken = "NUCLEATOR_UTILITY_NAME",
                skillDescriptionToken = "NUCLEATOR_UTILITY_DESCRIPTION",
                skillIcon = Assets.Instance.MainAssetBundle.LoadAsset<Sprite>("texNucleatorUtility"),
                activationState = new SerializableEntityStateType(typeof(ChargeFissionImpulse)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 6f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });

            Skills.AddUtilitySkills(bodyPrefab, utilitySkillDef);
            #endregion

            #region Special
            SkillDef specialSkillDef = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "NUCLEATOR_SPECIAL_NAME",
                skillNameToken = "NUCLEATOR_SPECIAL_NAME",
                skillDescriptionToken = "NUCLEATOR_SPECIAL_DESCRIPTION",
                skillIcon = Assets.Instance.MainAssetBundle.LoadAsset<Sprite>("texNucleatorSpecial"),
                activationState = new SerializableEntityStateType(typeof(ApplyRadionuclideSurge)),
                activationStateMachineName = "Slide",
                baseMaxStock = 1,
                baseRechargeInterval = 12f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });

            Skills.AddSpecialSkills(bodyPrefab, specialSkillDef);
            #endregion
        }

        internal override void RegisterTokens()
        {
            LanguageAPI.Add("NUCLEATOR_NAME", "Nucleator");
            LanguageAPI.Add("NUCLEATOR_SUBTITLE", "Walking Fallout");
            LanguageAPI.Add("NUCLEATOR_DESCRIPTION", "The Nucleator is a radioactive juggernaut with rad-proof armor, which allows him to manipulate nuclear components for long periods of time.\n\n" +
                "<color=#CCD3E0> < ! > Nucleator can charge his skills for maximum output, however, be careful as overcharging them may lead to self-harm!\n\n" +
                " < ! > x\n\n" +
                " < ! > y\n\n" +
                " < ! > z\n");
            LanguageAPI.Add("NUCLEATOR_OUTRO_FLAVOR", "..and so he left, soul and bones crackling with radiation.");
            LanguageAPI.Add("NUCLEATOR_OUTRO_FAILURE", "..and so he vanished, an uninhabitable wasteland in his wake.");
            LanguageAPI.Add("NUCLEATOR_LORE", "laugh and grow fat");
            LanguageAPI.Add("NUCLEATOR_DEFAULT_SKIN_NAME", "Default");
            LanguageAPI.Add("NUCLEATOR_MASTERY_SKIN_NAME", "Blastproof");
            LanguageAPI.Add("NUCLEATOR_KNIGHT_SKIN_NAME", "Gladiator");

            LanguageAPI.Add("NUCLEATOR_PRIMARY_NAME", "Irradiate");
            LanguageAPI.Add("NUCLEATOR_PRIMARY_DESCRIPTION", $"Charge and fire a bullet for up to <style=cIsDamage>{500}% damage</style>, <style=cIsDamage>{900}% damage</style> on <style=cIsUtility>overcharge</style>. The bullet's damage increases <style=cIsUtility>the farther it travels</style>.");

            LanguageAPI.Add("NUCLEATOR_SECONDARY_NAME", "Quarantine");
            LanguageAPI.Add("NUCLEATOR_SECONDARY_DESCRIPTION", $"Push enemies in front of you for <style=cIsDamage>{500}% piercing damage</style>.");

            LanguageAPI.Add("NUCLEATOR_UTILITY_NAME", "Fission Impulse");
            LanguageAPI.Add("NUCLEATOR_UTILITY_DESCRIPTION", $"<style=cIsUtility>Hold</style> to <style=cIsUtility>launch yourself</style>, dealing style=cIsDamage>{550}% damage</style>. Control the direction using the movement keys.");

            LanguageAPI.Add("NUCLEATOR_SPECIAL_NAME", "Radionuclide Surge");
            LanguageAPI.Add("NUCLEATOR_SPECIAL_DESCRIPTION", $"Enter a nuclear state for <style=cIsUtility>6 seconds</style>, adding <style=cIsHealing>radiation</style> to every attack while becoming <style=cIsUtility>invulnerable to overcharging skills</style>.");

            LanguageAPI.Add("NUCLEATOR_UNLOCKUNLOCKABLE_ACHIEVEMENT_NAME", "Overkill");
            LanguageAPI.Add("NUCLEATOR_UNLOCKUNLOCKABLE_ACHIEVEMENT_DESC", "Collect 5 Legendary items in one run.");
            LanguageAPI.Add("NUCLEATOR_UNLOCKUNLOCKABLE_UNLOCKABLE_NAME", "Overkill");

            // todo: make a base class for mastery achievements and simply inherit from it for each character 
            LanguageAPI.Add("NUCLEATOR_MASTERYUNLOCKABLE_ACHIEVEMENT_NAME", "Nucleator: Mastery");
            LanguageAPI.Add("NUCLEATOR_MASTERYUNLOCKABLE_ACHIEVEMENT_DESC", "As Nucleator, beat the game or obliterate on Monsoon or harder.");
            LanguageAPI.Add("NUCLEATOR_MASTERYUNLOCKABLE_UNLOCKABLE_NAME", "Nucleator: Mastery");

            LanguageAPI.Add("NUCLEATOR_GRANDMASTERYUNLOCKABLE_ACHIEVEMENT_NAME", "Nucleator: Grand Mastery");
            LanguageAPI.Add("NUCLEATOR_GRANDMASTERYUNLOCKABLE_ACHIEVEMENT_DESC", "As Nucleator, beat the game or obliterate on Typhoon.");
            LanguageAPI.Add("NUCLEATOR_GRANDMASTERYUNLOCKABLE_UNLOCKABLE_NAME", "Nucleator: Grand Mastery");
        }

        internal override void Hook()
        {

        }

        internal override void InitializeSkins()
        {
            GameObject model = bodyPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            SkinDef defaultSkin = Skins.CreateSkinDef("NUCLEATOR_DEFAULT_SKIN_NAME",
                Assets.Instance.MainAssetBundle.LoadAsset<Sprite>("texNucleatorSkin"),
                defaultRenderers,
                mainRenderer,
                model);

            skins.Add(defaultSkin);
            #endregion
            /*
            #region MasterySkin
            string materialName = UnityEngine.Random.value < 0.01f ? "matNucleatorMastery" : "matNucleatorMasteryAlt";
            Material masteryMat = Assets.CreateMaterial(materialName, 1f, Color.white);
            CharacterModel.RendererInfo[] masteryRendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
            {
                masteryMat,
                masteryMat,
                masteryMat
            });

            SkinDef masterySkin = Skins.CreateSkinDef("Nucleator_MASTERY_SKIN_NAME",
                Assets.Instance.MainAssetBundle.LoadAsset<Sprite>("texNucleatorSkinMaster"),
                masteryRendererInfos,
                mainRenderer,
                model,
                masterySkinUnlockableDef);

            masterySkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Assets.Instance.MainAssetBundle.LoadAsset<Mesh>("meshNucleatorMastery"),
                    renderer = defaultRenderers[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Assets.Instance.MainAssetBundle.LoadAsset<Mesh>("meshNucleatorGun"),
                    renderer = defaultRenderers[1].renderer
                }
            };

            skins.Add(masterySkin);
            #endregion

            #region GrandMasterySkin
            Material grandMasteryMat = Assets.CreateMaterial("matNucleatorKnight");
            CharacterModel.RendererInfo[] grandMasteryRendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
            {
                grandMasteryMat,
                grandMasteryMat,
                grandMasteryMat
            });

            SkinDef grandMasterySkin = Skins.CreateSkinDef("Nucleator_KNIGHT_SKIN_NAME",
                Assets.Instance.MainAssetBundle.LoadAsset<Sprite>("texNucleatorSkinGrandMaster"),
                grandMasteryRendererInfos,
                mainRenderer,
                model,
                grandMasterySkinUnlockableDef);

            grandMasterySkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Assets.Instance.MainAssetBundle.LoadAsset<Mesh>("meshNucleatorKnight"),
                    renderer = defaultRenderers[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Assets.Instance.MainAssetBundle.LoadAsset<Mesh>("meshNucleatorGun"),
                    renderer = defaultRenderers[1].renderer
                }
            };

            skins.Add(grandMasterySkin);
            #endregion
            

            skinController.skins = skins.ToArray();
        }

        internal override void SetItemDisplays()
        {
            instance.itemDisplayRules = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();

            #region Item Displays
            #region Essential
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.Jetpack,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBugWings"),
childName = "Chest",
localPos = new Vector3(0.0053F, 1.3482F, -2.9772F),
localAngles = new Vector3(21.499F, 4.7947F, 357.4174F),
localScale = new Vector3(0.6021F, 0.6021F, 0.6021F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.GoldGat,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGoldGat"),
childName = "Chest",
localPos = new Vector3(-0.0638F, 3.7798F, -1.753F),
localAngles = new Vector3(3.8203F, 86.9802F, 327.499F),
localScale = new Vector3(0.1829F, 0.1829F, 0.1829F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.BFG,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBFG"),
childName = "Chest",
localPos = new Vector3(1.5265F, 1.5996F, -0.1235F),
localAngles = new Vector3(356.8932F, 355.109F, 334.0596F),
localScale = new Vector3(0.8184F, 0.8184F, 0.8184F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            #endregion

            if (1 == 1)
            {
                #region NonEssential
                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.CritGlasses,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGlasses"),
childName = "Head",
localPos = new Vector3(-0.0231F, 0.6639F, 0.7902F),
localAngles = new Vector3(330F, 0F, 0F),
localScale = new Vector3(1.186F, 1.3483F, 1.3483F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Syringe,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySyringeCluster"),
childName = "UpperArmL",
localPos = new Vector3(0.1589F, 1.6573F, 0.3847F),
localAngles = new Vector3(276.5039F, 228.6428F, 357.9287F),
localScale = new Vector3(0.4975F, 0.4975F, 0.4975F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Behemoth,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBehemoth"),
childName = "HandR",
localPos = new Vector3(1.0744F, 0.93F, -0.2922F),
localAngles = new Vector3(282.6243F, 358.5435F, 283.4764F),
localScale = new Vector3(0.5628F, 0.2395F, 0.4802F),
                           limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Missile,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMissileLauncher"),
childName = "Chest",
localPos = new Vector3(-2.138F, 2.9379F, -0.2796F),
localAngles = new Vector3(4.2267F, 3.1961F, 21.8907F),
localScale = new Vector3(0.614F, 0.614F, 0.614F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Dagger,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDagger"),
childName = "Chest",
localPos = new Vector3(1.3626F, 1.6212F, -0.5288F),
localAngles = new Vector3(290.2635F, 115.2523F, 237.3611F),
localScale = new Vector3(2.3772F, 2.3772F, 2.3526F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Hoof,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayHoof"),
childName = "CalfL",
localPos = new Vector3(-0.1161F, 1.002F, -0.6076F),
localAngles = new Vector3(52.8763F, 346.0106F, 342.9214F),
localScale = new Vector3(0.653F, 0.7991F, 0.2944F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ChainLightning,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayUkulele"),
childName = "Chest",
localPos = new Vector3(-0.3694F, 1.7039F, -3.43F),
localAngles = new Vector3(1.3333F, 183.0056F, 44.8382F),
localScale = new Vector3(3.3472F, 3.3472F, 3.3472F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.GhostOnKill,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMask"),
childName = "Head",
localPos = new Vector3(-0.0014F, 0.5076F, 0.556F),
localAngles = new Vector3(327.1712F, 359.9561F, 359.9553F),
localScale = new Vector3(2.6343F, 2.6343F, 2.6343F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Mushroom,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMushroom"),
childName = "FootR",
localPos = new Vector3(0.1287F, 1.2552F, -0.2734F),
localAngles = new Vector3(30.6293F, 268.4641F, 82.9897F),
localScale = new Vector3(0.1604F, 0.1604F, 0.1604F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.AttackSpeedOnCrit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWolfPelt"),
childName = "Head",
localPos = new Vector3(-0.0014F, 0.7701F, 0.2259F),
localAngles = new Vector3(36.6505F, 179.2319F, 0.4507F),
localScale = new Vector3(2.0275F, 1.9701F, -2.0711F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BleedOnHit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTriTip"),
childName = "UpperArmR",
localPos = new Vector3(-1.2159F, 0.1127F, -0.0792F),
localAngles = new Vector3(349.322F, 99.0736F, 52.6522F),
localScale = new Vector3(1.7108F, 1.7108F, 1.5476F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.WardOnLevel,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWarbanner"),
childName = "Pelvis",
localPos = new Vector3(0.0168F, 0.4191F, 1.3507F),
localAngles = new Vector3(0F, 0F, 90F),
localScale = new Vector3(1.1358F, 1.1358F, 1.1358F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.HealOnCrit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayScythe"),
childName = "Head",
localPos = new Vector3(0.0622F, 0.0729F, 0.3255F),
localAngles = new Vector3(5.034F, 54.2752F, 7.3106F),
localScale = new Vector3(1.1661F, 1.1661F, 1.1661F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.HealWhileSafe,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySnail"),
childName = "UpperArmR",
localPos = new Vector3(-0.2237F, 1.4262F, 0.229F),
localAngles = new Vector3(306.0659F, 156.4631F, 330.0789F),
localScale = new Vector3(0.2543F, 0.2543F, 0.2543F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Clover,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayClover"),
childName = "Head",
localPos = new Vector3(0.7378F, 0.7287F, 0.5115F),
localAngles = new Vector3(58.6951F, 302.0862F, 71.7473F),
localScale = new Vector3(1.1769F, 1.1769F, 1.1769F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BarrierOnOverHeal,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAegis"),
childName = "LowerArmL",
localPos = new Vector3(0.6174F, 0.7663F, 0.3212F),
localAngles = new Vector3(284.9145F, 37.6427F, 51.7821F),
localScale = new Vector3(1.5069F, 1.5069F, 1.5069F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.GoldOnHit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBoneCrown"),
childName = "Head",
localPos = new Vector3(0.0276F, 0.6767F, 0.2063F),
localAngles = new Vector3(349.8091F, 359.9842F, 359.9839F),
localScale = new Vector3(3.1775F, 3.1775F, 3.1775F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.WarCryOnMultiKill,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayPauldron"),
childName = "UpperArmL",
localPos = new Vector3(0.4794F, 0.1269F, 0.3454F),
localAngles = new Vector3(79.0243F, 108.4299F, 59.3549F),
localScale = new Vector3(4.903F, 4.903F, 4.903F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.SprintArmor,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBuckler"),
childName = "LowerArmR",
localPos = new Vector3(-0.6995F, 0.704F, 0.0849F),
localAngles = new Vector3(0F, 275.0831F, 0F),
localScale = new Vector3(0.6882F, 0.6882F, 0.6882F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.IceRing,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayIceRing"),
childName = "HandR",
localPos = new Vector3(0.0704F, 0.6405F, -0.205F),
localAngles = new Vector3(2.3306F, 104.4551F, 308.8382F),
localScale = new Vector3(2.8721F, 2.8721F, 2.8721F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.FireRing,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFireRing"),
childName = "HandL",
localPos = new Vector3(-0.1803F, 0.9227F, -0.1527F),
localAngles = new Vector3(346.1743F, 261.1052F, 74.983F),
localScale = new Vector3(2.7042F, 2.7042F, 2.7042F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.UtilitySkillMagazine,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAfterburnerShoulderRing"),
                            childName = "Chest",
                            localPos = new Vector3(-1.3682F, 1.2975F, 0.2418F),
                            localAngles = new Vector3(320.8235F, 179.8961F, 180.1247F),
                            localScale = new Vector3(1.6562F, 1.6562F, 1.6562F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAfterburnerShoulderRing"),
                            childName = "Chest",
                            localPos = new Vector3(1.2383F, 1.1319F, 0.0239F),
                            localAngles = new Vector3(312.1512F, 179.8941F, 180.1228F),
                            localScale = new Vector3(1.7514F, 1.7514F, 1.7514F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.JumpBoost,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWaxBird"),
childName = "Chest",
localPos = new Vector3(1.2283F, 3.1027F, -2.4703F),
localAngles = new Vector3(24.419F, 0F, 0F),
localScale = new Vector3(1.6226F, 1.6226F, 1.6226F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ArmorReductionOnHit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWarhammer"),
childName = "Pelvis",
localPos = new Vector3(1.3766F, 0.968F, -0.4432F),
localAngles = new Vector3(281.3977F, 63.3855F, 243.3858F),
localScale = new Vector3(0.3725F, 0.3725F, 0.3725F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.NearbyDamageBonus,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDiamond"),
childName = "HandR",
localPos = new Vector3(0.6571F, 0.8275F, -0.2468F),
localAngles = new Vector3(8.5242F, 16.2455F, 3.6275F),
localScale = new Vector3(0.6825F, 0.6825F, 0.6825F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ArmorPlate,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayRepulsionArmorPlate"),
childName = "ThighR",
localPos = new Vector3(-0.1927F, 1.2396F, -0.5353F),
localAngles = new Vector3(85.1644F, 323.0438F, 316.1555F),
localScale = new Vector3(1.2378F, 1.1699F, 1.235F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.CommandMissile,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMissileRack"),
childName = "Chest",
localPos = new Vector3(0F, 0.2985F, -0.0663F),
localAngles = new Vector3(90F, 180F, 0F),
localScale = new Vector3(0.3362F, 0.3362F, 0.3362F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Feather,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFeather"),
childName = "LowerArmR",
localPos = new Vector3(0.3169F, -0.3752F, 0.6695F),
localAngles = new Vector3(330.0292F, 279.5677F, 271.7176F),
localScale = new Vector3(0.1092F, 0.1092F, 0.1092F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Crowbar,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayCrowbar"),
childName = "Chest",
localPos = new Vector3(2.0687F, 1.8656F, -2.3103F),
localAngles = new Vector3(317.3768F, 187.5196F, 353.1285F),
localScale = new Vector3(1.2944F, 1.2944F, 1.2944F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.FallBoots,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGravBoots"),
childName = "CalfL",
localPos = new Vector3(-0.0038F, 0.3729F, -0.0046F),
localAngles = new Vector3(358.007F, 346.1812F, 351.3452F),
localScale = new Vector3(1.4466F, 1.4466F, 1.4466F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGravBoots"),
childName = "CalfR",
localPos = new Vector3(-0.0038F, 0.3729F, -0.0046F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1.5392F, 1.5392F, 1.5392F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ExecuteLowHealthElite,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGuillotine"),
childName = "ThighR",
localPos = new Vector3(0.2178F, 0.8037F, -0.8158F),
localAngles = new Vector3(47.9527F, 126.6104F, 132.7923F),
localScale = new Vector3(0.528F, 0.528F, 0.528F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.EquipmentMagazine,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBattery"),
childName = "Chest",
localPos = new Vector3(0F, 0.0791F, 0.0726F),
localAngles = new Vector3(0F, 270F, 292.8411F),
localScale = new Vector3(0.0773F, 0.0773F, 0.0773F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.NovaOnHeal,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDevilHorns"),
childName = "Head",
localPos = new Vector3(0.094F, 0.5587F, 0.1396F),
localAngles = new Vector3(343.399F, 359.9753F, 359.975F),
localScale = new Vector3(2.8585F, 2.8585F, 2.8585F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDevilHorns"),
childName = "Head",
localPos = new Vector3(-0.094F, 0.5587F, 0.1396F),
localAngles = new Vector3(343.399F, 359.9753F, 359.975F),
localScale = new Vector3(2.8585F, 2.8585F, -2.8585F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Infusion,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayInfusion"),
childName = "Pelvis",
localPos = new Vector3(1.3458F, 0.5542F, -0.5381F),
localAngles = new Vector3(11.0337F, 289.4426F, 156.9838F),
localScale = new Vector3(1.734F, 1.734F, 1.734F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Medkit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMedkit"),
childName = "Chest",
localPos = new Vector3(-1.7765F, 1.2154F, -3.4836F),
localAngles = new Vector3(284.6032F, 329.5478F, 258.9214F),
localScale = new Vector3(1.6035F, 1.6035F, 1.6035F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Bandolier,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBandolier"),
childName = "Chest",
localPos = new Vector3(0.2366F, -0.292F, -0.6368F),
localAngles = new Vector3(42.0503F, 54.4216F, 263.4034F),
localScale = new Vector3(4.3681F, 5.8241F, 7.2802F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BounceNearby,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayHook"),
childName = "Chest",
localPos = new Vector3(-3.5781F, 3.2209F, -3.0955F),
localAngles = new Vector3(309.2372F, 352.3436F, 177.2255F),
localScale = new Vector3(1.1472F, 1.1472F, 1.1472F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.IgniteOnKill,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGasoline"),
childName = "Pelvis",
localPos = new Vector3(-1.0668F, 0.687F, 1.3548F),
localAngles = new Vector3(66.3947F, 222.9386F, 212.7688F),
localScale = new Vector3(2.0854F, 2.0854F, 2.0854F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.StunChanceOnHit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayStunGrenade"),
childName = "Chest",
localPos = new Vector3(0.0751F, -2.0409F, 1.7598F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(-2.3144F, -2.3144F, -2.3144F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Firework,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFirework"),
childName = "Chest",
localPos = new Vector3(0.0086F, 0.0069F, 0.0565F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.1194F, 0.1194F, 0.1194F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.LunarDagger,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayLunarDagger"),
childName = "Chest",
localPos = new Vector3(-1.9787F, 1.6226F, -2.3267F),
localAngles = new Vector3(299.6609F, 317.0937F, 303.8001F),
localScale = new Vector3(1.5148F, 1.5148F, 1.5148F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Knurl,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayKnurl"),
childName = "LowerArmL",
localPos = new Vector3(-0.244F, -0.6586F, -0.0539F),
localAngles = new Vector3(78.4995F, 338.379F, 31.9825F),
localScale = new Vector3(0.4693F, 0.6335F, 0.6335F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BeetleGland,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBeetleGland"),
childName = "Pelvis",
localPos = new Vector3(-1.4093F, 0.6146F, -0.3213F),
localAngles = new Vector3(16.0486F, 347.5013F, 144.888F),
localScale = new Vector3(0.3968F, 0.3968F, 0.3968F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.SprintBonus,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySoda"),
childName = "Pelvis",
localPos = new Vector3(1.3994F, 0.3703F, 0.0613F),
localAngles = new Vector3(64.8751F, 178.4553F, 249.472F),
localScale = new Vector3(1.3941F, 1.3941F, 1.3941F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.SecondarySkillMagazine,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDoubleMag"),
childName = "HandL",
localPos = new Vector3(0.075F, 0.1001F, -0.0586F),
localAngles = new Vector3(327.2019F, 238.0695F, 15.7064F),
localScale = new Vector3(0.4325F, 0.4325F, 0.4325F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.StickyBomb,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayStickyBomb"),
childName = "Chest",
localPos = new Vector3(0.0714F, 3.6169F, -1.975F),
localAngles = new Vector3(85.5113F, 132.8514F, 116.2868F),
localScale = new Vector3(0.6207F, 0.6207F, 0.6207F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.TreasureCache,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayKey"),
childName = "Pelvis",
localPos = new Vector3(0.0589F, 0.1056F, -0.0174F),
localAngles = new Vector3(0.2454F, 195.0205F, 89.0854F),
localScale = new Vector3(0.4092F, 0.4092F, 0.4092F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BossDamageBonus,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAPRound"),
childName = "HandL",
localPos = new Vector3(-0.2735F, 1.1891F, -0.894F),
localAngles = new Vector3(280.8712F, 36.5325F, 134.1051F),
localScale = new Vector3(3.8678F, 3.8678F, 3.8678F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.SlowOnHit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBauble"),
childName = "Chest",
localPos = new Vector3(1.0131F, -0.0969F, 2.3571F),
localAngles = new Vector3(5.4525F, 249.766F, 6.0882F),
localScale = new Vector3(1.2451F, 1.2451F, 1.2451F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ExtraLife,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayHippo"),
childName = "Chest",
localPos = new Vector3(-0.0763F, 4.1502F, -1.0302F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1.479F, 1.479F, 1.479F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.KillEliteFrenzy,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBrainstalk"),
childName = "Head",
localPos = new Vector3(0F, 0.1882F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1.0932F, 1.0932F, 1.0932F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.RepeatHeal,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayCorpseFlower"),
childName = "Chest",
localPos = new Vector3(-0.5483F, -0.8844F, 1.2658F),
localAngles = new Vector3(273.9301F, 121.7874F, 25.4336F),
localScale = new Vector3(0.742F, 0.742F, 0.742F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.AutoCastEquipment,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFossil"),
childName = "Chest",
localPos = new Vector3(0F, -1.1424F, 1.4252F),
localAngles = new Vector3(0F, 90F, 350.954F),
localScale = new Vector3(2.3358F, 2.3358F, 2.3358F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.IncreaseHealing,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAntler"),
childName = "Head",
localPos = new Vector3(0.4324F, 0.6207F, 0.2771F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(1.1387F, 1.1387F, 1.1387F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAntler"),
childName = "Head",
localPos = new Vector3(-0.4324F, 0.6207F, 0.2771F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(1.1387F, 1.1387F, -1.1387F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.TitanGoldDuringTP,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGoldHeart"),
childName = "Chest",
localPos = new Vector3(1.3722F, 1.1989F, 1.2017F),
localAngles = new Vector3(335.0033F, 343.2951F, 0F),
localScale = new Vector3(0.6772F, 0.6772F, 0.6772F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.SprintWisp,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBrokenMask"),
childName = "UpperArmR",
localPos = new Vector3(-0.4241F, 0.8134F, 0.5495F),
localAngles = new Vector3(328.8594F, 316.049F, 1.269F),
localScale = new Vector3(1.2023F, 1.2023F, 1.2023F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BarrierOnKill,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBrooch"),
childName = "Chest",
localPos = new Vector3(0.5536F, 0.612F, 1.0976F),
localAngles = new Vector3(83.2556F, 0F, 0F),
localScale = new Vector3(2.4432F, 5.413F, 2.4432F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.TPHealingNova,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGlowFlower"),
childName = "Chest",
localPos = new Vector3(-1.3266F, 2.4523F, 0.9116F),
localAngles = new Vector3(351.1796F, 341.9114F, 8.3329F),
localScale = new Vector3(0.9157F, 0.9157F, 0.0915F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.LunarUtilityReplacement,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBirdFoot"),
childName = "Chest",
localPos = new Vector3(0.0126F, 2.6579F, -3.0142F),
localAngles = new Vector3(302.5751F, 292.0297F, 33.2432F),
localScale = new Vector3(2.359F, 2.359F, 2.359F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Thorns,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayRazorwireLeft"),
childName = "UpperArmL",
localPos = new Vector3(-4.8571F, -5.2138F, 5.0705F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(5.8866F, 6.28F, 7.0162F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.LunarPrimaryReplacement,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBirdEye"),
childName = "Head",
localPos = new Vector3(-0.0201F, 0.7425F, 0.9408F),
localAngles = new Vector3(305.6389F, 179.871F, 180.0667F),
localScale = new Vector3(0.437F, 0.437F, 0.437F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.LunarSecondaryReplacement,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBirdClaw"),
childName = "LowerArmR",
localPos = new Vector3(0.3025F, -0.7974F, 0.1147F),
localAngles = new Vector3(1.7554F, 309.2843F, 142.7481F),
localScale = new Vector3(1.5271F, 1.5271F, 1.5271F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.LunarSpecialReplacement,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBirdHeart"),
childName = "Base",
localPos = new Vector3(-2.5244F, 1.3072F, 9.8724F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(0.2866F, 0.2866F, 0.2866F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ParentEgg,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayParentEgg"),
childName = "Head",
localPos = new Vector3(0.0001F, 0.0109F, 0.2528F),
localAngles = new Vector3(344.878F, 359.9763F, 0.172F),
localScale = new Vector3(0.8641F, 0.8641F, 0.9067F),
                            limbMask = LimbFlags.None
                        }
        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.LightningStrikeOnHit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayChargedPerforator"),
childName = "HandR",
localPos = new Vector3(0.7302F, 1.3364F, -0.2174F),
localAngles = new Vector3(304.989F, 75.8218F, 31.3188F),
localScale = new Vector3(3.7596F, 3.7595F, 3.7595F),
                            limbMask = LimbFlags.None
                        }
        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.NovaOnLowHealth,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayJellyGuts"),
childName = "Head",
localPos = new Vector3(-1.2555F, 0.0974F, -1.3461F),
localAngles = new Vector3(336.5116F, 11.2809F, 338.5165F),
localScale = new Vector3(0.4811F, 0.4811F, 0.4811F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.LunarTrinket,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBeads"),
childName = "LowerArmL",
localPos = new Vector3(-0.1152F, 0.9603F, 0.7459F),
localAngles = new Vector3(24.2259F, 131.142F, 38.498F),
localScale = new Vector3(6.3109F, 6.3109F, 6.3109F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Plant,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayInterstellarDeskPlant"),
childName = "Chest",
localPos = new Vector3(-1.1108F, 3.9663F, -1.8617F),
localAngles = new Vector3(287.3414F, 338.7322F, 37.8519F),
localScale = new Vector3(0.2145F, 0.2145F, 0.2145F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Bear,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBear"),
childName = "Chest",
localPos = new Vector3(-0.0022F, 0.4868F, 1.1796F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1.2653F, 1.2653F, 1.2653F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.DeathMark,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDeathMark"),
childName = "UpperArmL",
localPos = new Vector3(0.7408F, 0.9469F, 0.3757F),
localAngles = new Vector3(329.0903F, 269.7474F, 320.2151F),
localScale = new Vector3(-0.0943F, -0.0943F, -0.0943F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ExplodeOnDeath,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWilloWisp"),
childName = "Pelvis",
localPos = new Vector3(-1.4326F, 0.0951F, 2.1433F),
localAngles = new Vector3(350.6892F, 81.0076F, 184.2473F),
localScale = new Vector3(0.2285F, 0.2285F, 0.2285F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Seed,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySeed"),
childName = "UpperArmL",
localPos = new Vector3(-0.2552F, 0.1589F, 0.747F),
localAngles = new Vector3(344.0657F, 196.8238F, 275.5892F),
localScale = new Vector3(0.1185F, 0.1185F, 0.1185F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.SprintOutOfCombat,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWhip"),
childName = "ThighR",
localPos = new Vector3(0.5961F, 1.0433F, 0.0965F),
localAngles = new Vector3(26.3066F, 4.8232F, 355.689F),
localScale = new Vector3(0.9379F, 0.9379F, 0.9379F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.CooldownOnCrit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySkull"),
childName = "Head",
localPos = new Vector3(-0.0433F, 0.6357F, 0.7063F),
localAngles = new Vector3(298.951F, 179.877F, 180.073F),
localScale = new Vector3(0.911F, 0.911F, 0.911F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Phasing,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayStealthkit"),
childName = "CalfL",
localPos = new Vector3(0.0444F, -0.6525F, -0.0288F),
localAngles = new Vector3(39.0883F, 6.8139F, 358.9713F),
localScale = new Vector3(0.7934F, 1.3091F, 0.8731F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.PersonalShield,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayShieldGenerator"),
childName = "Chest",
localPos = new Vector3(-1.3821F, 0.0792F, 0.9353F),
localAngles = new Vector3(304.1204F, 90F, 270F),
localScale = new Vector3(0.5086F, 0.5086F, 0.5086F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ShockNearby,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTeslaCoil"),
childName = "Chest",
localPos = new Vector3(0.0076F, 2.4602F, -3.0751F),
localAngles = new Vector3(297.6866F, 1.3864F, 358.5596F),
localScale = new Vector3(1.3158F, 1.3158F, 1.3158F),
                          limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ShieldOnly,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayShieldBug"),
childName = "Head",
localPos = new Vector3(0.0868F, 0.3114F, 0F),
localAngles = new Vector3(0.0723F, 263.8974F, 28.0623F),
localScale = new Vector3(1.0726F, 1.0726F, 1.0726F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayShieldBug"),
childName = "Head",
localPos = new Vector3(-0.0868F, 0.3114F, 0F),
localAngles = new Vector3(0.0723F, 263.8974F, 28.0623F),
localScale = new Vector3(1.0726F, 1.0726F, -1.0726F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.AlienHead,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAlienHead"),
childName = "Pelvis",
localPos = new Vector3(-0.3944F, -1.2984F, -1.3968F),
localAngles = new Vector3(90F, 90F, 0F),
localScale = new Vector3(3.4557F, 3.4557F, 3.4557F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.HeadHunter,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySkullCrown"),
childName = "Pelvis",
localPos = new Vector3(0F, 0.6875F, -0.0108F),
localAngles = new Vector3(0.0023F, 0.2417F, 358.9149F),
localScale = new Vector3(-4.1194F, -1.2883F, -1.2883F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.EnergizedOnEquipmentUse,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWarHorn"),
childName = "Pelvis",
localPos = new Vector3(-1.6078F, 0.2061F, 0.0312F),
localAngles = new Vector3(8.6708F, 85.58F, 192.6967F),
localScale = new Vector3(1.2506F, 1.2506F, 1.2506F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.FlatHealth,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySteakCurved"),
childName = "Chest",
localPos = new Vector3(1.5333F, 1.0264F, 1.09F),
localAngles = new Vector3(343.3672F, 350.1884F, 154.508F),
localScale = new Vector3(0.4972F, 0.4613F, 0.4613F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Tooth,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayToothMeshLarge"),
childName = "Head",
localPos = new Vector3(-0.0012F, -0.2492F, 1.1374F),
localAngles = new Vector3(344.9017F, 0F, 0F),
localScale = new Vector3(16.4269F, 16.4269F, 16.4269F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Pearl,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayPearl"),
childName = "LowerArmR",
localPos = new Vector3(-0.0606F, 1.3665F, 0.1727F),
localAngles = new Vector3(62.8835F, 356.566F, 156.5178F),
localScale = new Vector3(0.4281F, 0.4281F, 0.4281F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ShinyPearl,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayShinyPearl"),
childName = "LowerArmL",
localPos = new Vector3(-0.1002F, 1.6855F, 0.4207F),
localAngles = new Vector3(287.5002F, 192.2082F, 138.7554F),
localScale = new Vector3(0.3901F, 0.3901F, 0.3901F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BonusGoldPackOnKill,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTome"),
childName = "ThighR",
localPos = new Vector3(0.2135F, 0.0966F, 0.9252F),
localAngles = new Vector3(353.1937F, 37.2378F, 13.7757F),
localScale = new Vector3(0.398F, 0.398F, 0.398F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Squid,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySquidTurret"),
childName = "Chest",
localPos = new Vector3(0.0371F, -0.788F, -2.1849F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(0.1319F, 0.1319F, 0.1319F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Icicle,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFrostRelic"),
childName = "Root",
localPos = new Vector3(-1.5475F, 2.1073F, 6.0828F),
localAngles = new Vector3(10.9042F, 180F, 180F),
localScale = new Vector3(0, 0, 0),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Talisman,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTalisman"),
childName = "Root",
localPos = new Vector3(2.2785F, 0.9182F, 5.9034F),
localAngles = new Vector3(84.8818F, 180F, 180F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.LaserTurbine,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayLaserTurbine"),
childName = "Chest",
localPos = new Vector3(1.2514F, 1.5626F, 0.5178F),
localAngles = new Vector3(295.9611F, 28.8779F, 340.5884F),
localScale = new Vector3(0.6323F, 0.6323F, 0.6323F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.FocusConvergence,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFocusedConvergence"),
childName = "Root",
localPos = new Vector3(3.0207F, 4.4731F, 4.8772F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Incubator,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAncestralIncubator"),
childName = "Chest",
localPos = new Vector3(0F, 0.3453F, 0F),
localAngles = new Vector3(353.0521F, 317.2421F, 69.6292F),
localScale = new Vector3(0.0528F, 0.0528F, 0.0528F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.FireballsOnHit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFireballsOnHit"),
childName = "Chest",
localPos = new Vector3(2.9299F, -0.3379F, 2.738F),
localAngles = new Vector3(314.1453F, 353.6355F, 182.9236F),
localScale = new Vector3(0.2965F, 0.2965F, 0.2965F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.SiphonOnLowHealth,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySiphonOnLowHealth"),
childName = "Chest",
localPos = new Vector3(1.4656F, 2.3486F, -3.1538F),
localAngles = new Vector3(357.6962F, 352.2416F, 356.3309F),
localScale = new Vector3(0.3059F, 0.3059F, 0.3059F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BleedOnHitAndExplode,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBleedOnHitAndExplode"),
childName = "Stomach",
localPos = new Vector3(0.4776F, 0.9076F, 1.3978F),
localAngles = new Vector3(0.6431F, 356.3326F, 19.931F),
localScale = new Vector3(0.2782F, 0.2782F, 0.2782F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.MonstersOnShrineUse,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMonstersOnShrineUse"),
childName = "ThighL",
localPos = new Vector3(-0.7476F, 0.3422F, 0.0891F),
localAngles = new Vector3(309.4294F, 357.554F, 22.8222F),
localScale = new Vector3(0.2557F, 0.2138F, 0.2137F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.RandomDamageZone,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayRandomDamageZone"),
childName = "Chest",
localPos = new Vector3(1.3512F, 2.1826F, 0.7823F),
localAngles = new Vector3(28.6543F, 185.9949F, 181.1955F),
localScale = new Vector3(0.418F, 0.3067F, 0.4389F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Fruit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFruit"),
childName = "ThighR",
localPos = new Vector3(0.4837F, 2.1204F, 0.7174F),
localAngles = new Vector3(342.6497F, 167.0954F, 54.0983F),
localScale = new Vector3(-0.6938F, -0.6938F, -0.6938F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.AffixRed,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteHorn"),
childName = "Head",
localPos = new Vector3(0.1502F, 0.5764F, 0.4795F),
localAngles = new Vector3(0.1537F, 354.9407F, 3.4341F),
localScale = new Vector3(0.3271F, 0.3271F, 0.3271F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteHorn"),
childName = "Head",
localPos = new Vector3(-0.1214F, 0.5677F, 0.4531F),
localAngles = new Vector3(0.0024F, 359.8404F, 0.1538F),
localScale = new Vector3(-0.3352F, 0.3352F, 0.3352F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.AffixBlue,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteRhinoHorn"),
childName = "Head",
localPos = new Vector3(0F, 0.2648F, 0.1528F),
localAngles = new Vector3(315F, 0F, 0F),
localScale = new Vector3(0.9631F, 0.9631F, 0.9631F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteRhinoHorn"),
childName = "Head",
localPos = new Vector3(0F, 0.2648F, 0.1528F),
localAngles = new Vector3(315F, 0F, 0F),
localScale = new Vector3(0.9631F, 0.9631F, 0.9631F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.AffixWhite,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteIceCrown"),
childName = "Head",
localPos = new Vector3(-0.0009F, 0.8312F, 0.0003F),
localAngles = new Vector3(300.4418F, 178.6883F, 180.5195F),
localScale = new Vector3(0.0985F, 0.0985F, 0.0985F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.AffixPoison,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteUrchinCrown"),
childName = "Head",
localPos = new Vector3(-0.0011F, 0.8269F, 0.0984F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(0.179F, 0.179F, 0.179F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.AffixHaunted,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteStealthCrown"),
childName = "Head",
localPos = new Vector3(-0.0007F, 0.5068F, 0.0962F),
localAngles = new Vector3(290.717F, 187.4582F, 170.3772F),
localScale = new Vector3(0.2341F, 0.2341F, 0.2341F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.CritOnUse,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayNeuralImplant"),
childName = "Head",
localPos = new Vector3(-0.0034F, 1.0192F, 1.4139F),
localAngles = new Vector3(337.6601F, 359.9681F, 359.9677F),
localScale = new Vector3(1.1417F, 1.1417F, 1.1417F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.DroneBackup,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayRadio"),
childName = "Pelvis",
localPos = new Vector3(0.8246F, -1.716F, -3.124F),
localAngles = new Vector3(14.3406F, 190.862F, 207.5474F),
localScale = new Vector3(0.6193F, 0.6193F, 0.6193F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Lightning,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayLightningArmRight"),
childName = "UpperArmR",
localPos = new Vector3(0F, 0F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(3.8297F, 3.8297F, 3.8297F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.BurnNearby,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayPotion"),
childName = "Pelvis",
localPos = new Vector3(1.3077F, 0.7245F, -0.5077F),
localAngles = new Vector3(17.3374F, 326.1839F, 311.975F),
localScale = new Vector3(-0.1555F, -0.1555F, -0.1555F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.CrippleWard,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEffigy"),
childName = "Pelvis",
localPos = new Vector3(0.0768F, -0.0002F, 0F),
localAngles = new Vector3(0F, 270F, 0F),
localScale = new Vector3(0.2812F, 0.2812F, 0.2812F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.QuestVolatileBattery,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBatteryArray"),
childName = "Chest",
localPos = new Vector3(-0.0005F, 2.3731F, -3.2294F),
localAngles = new Vector3(351.1683F, 357.0253F, 271.8003F),
localScale = new Vector3(1.621F, 1.3809F, 1.249F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.GainArmor,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayElephantFigure"),
childName = "CalfR",
localPos = new Vector3(0.3053F, 1.0591F, 0.6344F),
localAngles = new Vector3(77.5634F, 0F, 0F),
localScale = new Vector3(1.9311F, 1.9311F, 1.9311F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Recycle,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayRecycler"),
childName = "Pelvis",
localPos = new Vector3(-1.4793F, 0.8417F, 0.0442F),
localAngles = new Vector3(13.4906F, 356.7064F, 3.2662F),
localScale = new Vector3(-0.2228F, -0.2228F, -0.2228F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.FireBallDash,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEgg"),
childName = "Pelvis",
localPos = new Vector3(0.9463F, 1.1411F, -0.4573F),
localAngles = new Vector3(73.5631F, 0F, 0F),
localScale = new Vector3(0.2132F, 0.2132F, 0.2132F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Cleanse,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWaterPack"),
childName = "Chest",
localPos = new Vector3(0.0073F, 2.238F, -3.3104F),
localAngles = new Vector3(0F, 180F, 0F),
localScale = new Vector3(0.2927F, 0.2927F, 0.2927F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Tonic,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTonic"),
childName = "Pelvis",
localPos = new Vector3(-1.2796F, 0.5832F, -0.5212F),
localAngles = new Vector3(5.3119F, 34.7754F, 173.4319F),
localScale = new Vector3(0.4725F, 0.4725F, 0.4725F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Gateway,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayVase"),
childName = "Pelvis",
localPos = new Vector3(0.0807F, 0.0877F, 0F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(0.0982F, 0.0982F, 0.0982F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Meteor,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMeteor"),
childName = "Root",
localPos = new Vector3(0F, 4.3711F, 5.1939F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Saw,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySawmerang"),
childName = "Root",
localPos = new Vector3(0.1043F, 4.8556F, 4.5026F),
localAngles = new Vector3(350.2157F, 180F, 180F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Blackhole,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGravCube"),
childName = "Root",
localPos = new Vector3(3.9731F, 1.2052F, 5.1563F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Scanner,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayScanner"),
childName = "Stomach",
localPos = new Vector3(-0.8126F, 2.8985F, 0.1982F),
localAngles = new Vector3(291.1373F, 6.0699F, 340.2449F),
localScale = new Vector3(0.4961F, 0.4961F, 0.4961F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.DeathProjectile,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDeathProjectile"),
childName = "Pelvis",
localPos = new Vector3(0F, 0.3258F, -1.4342F),
localAngles = new Vector3(346.461F, 1.5249F, 359.3125F),
localScale = new Vector3(-0.2267F, -0.2267F, -0.2267F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.LifestealOnHit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayLifestealOnHit"),
childName = "Head",
localPos = new Vector3(-0.4786F, 0.8659F, -0.2539F),
localAngles = new Vector3(44.0939F, 33.5151F, 43.5058F),
localScale = new Vector3(0.4444F, 0.4444F, 0.4444F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.TeamWarCry,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTeamWarCry"),
childName = "Pelvis",
localPos = new Vector3(-2.2411F, 2.2061F, -0.218F),
localAngles = new Vector3(357.7572F, 259.1322F, 167.6706F),
localScale = new Vector3(0.1233F, 0.1233F, 0.1233F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });
                #endregion
            }
            #endregion

            SetModdedItemDisplays();

            itemDisplayRuleSet.keyAssetRuleGroups = instance.itemDisplayRules.ToArray();
            itemDisplayRuleSet.GenerateRuntimeValues();
        }

        private static void SetModdedItemDisplays()
        {
            #region Aetherium
            if (ItemDisplayCore.aetheriumInstalled)
            {
            }
            #endregion
        }

        private static CharacterModel.RendererInfo[] SkinRendererInfos(CharacterModel.RendererInfo[] defaultRenderers, Material[] materials)
        {
            CharacterModel.RendererInfo[] newRendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            defaultRenderers.CopyTo(newRendererInfos, 0);

            newRendererInfos[0].defaultMaterial = materials[0];

            return newRendererInfos;
        }
    }
}
*/