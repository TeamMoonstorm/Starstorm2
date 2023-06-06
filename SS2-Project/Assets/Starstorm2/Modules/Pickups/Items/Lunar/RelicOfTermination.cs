using KinematicCharacterController;
using RoR2;
using RoR2.EntitlementManagement;
using RoR2.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class RelicOfTermination : ItemBase
    {
        private const string token = "SS2_ITEM_RELICOFTERMINATION_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("RelicOfTermination", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Time, in seconds, to kill the marked enemy before going on cooldown.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        [TokenModifier("SS2_ITEM_RELICOFTERMINATION_PICKUP", StatTypes.Default, 0)]
        public static float maxTime = 30f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Percent reduction in time to kill per stack. (1 = 100% reduction, .1 = 10% reduction)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 1, "100")]
        public static float timeReduction = .1f;

        //[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Time between marks in seconds.")]
        //[TokenModifier(token, StatTypes.Default, 0)]
        //public static float downTime = 30f;

        
        //[ConfigurableField(ConfigDesc = "Time between marks in seconds.")]
        //[TokenModifier(token, StatTypes.Default, 0)]
        //public static float downTime = 30f;

        //[ConfigurableField(ConfigDesc = "Percent reduction in time to kill per stack.")]
        //[TokenModifier(token, StatTypes.MultiplyByN, 1, "100")]
        //public static float timeReduction = .1f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Damage multiplier which is added to the marked enemy. (1 = 100% more damage).")]
        [TokenModifier(token, StatTypes.MultiplyByN, 2, "100")]
        public static float damageMult = 1.5f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Health multiplier which is added to the marked enemy. (1 = 100% more health).")]
        [TokenModifier(token, StatTypes.MultiplyByN, 3, "100")]
        public static float healthMult = 6f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Speed multiplier which is added to the marked enemy. (1 = 100% more speed).")]
        [TokenModifier(token, StatTypes.MultiplyByN, 4, "100")]
        public static float speedMult = .5f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Attack speed multiplier which is added to the marked enemy. (1 = 100% more attack speed).")]
        [TokenModifier(token, StatTypes.MultiplyByN, 5, "100")]
        public static float atkSpeedMult = 1f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Scale multiplier applied to marked enemies. (1 = 100% of normal scale (no change)).")]
        [TokenModifier(token, StatTypes.MultiplyByN, 6, "100")]
        public static float scaleMod = 1.5f;

        private static List<BodyIndex> illegalMarks = new List<BodyIndex>();

        //public static GameObject globalMarkEffect;

        public static GameObject globalMarkEffectTwo;
        public static GameObject spawnRock1VFX;
        public static GameObject spawnRock2VFX;
        //
        //SS2Config.IDItem, ConfigDesc = "Health multiplier grantd to marked enemy if not killed in time (1 = 100% health).")]
        //[TokenModifier(token, StatTypes.Percentage, 3)]
        //public static float effectiveRadius = 100f;
        public static GameObject markEffect;
        public static GameObject failEffect;
        public static GameObject buffEffect;

        public static Xoroshiro128Plus terminationRNG;

        //private GameObject markEffectInstance;

        override public void Initialize()
        {
            CharacterBody.onBodyStartGlobal += TerminationSpawnHook;
            GlobalEventManager.onCharacterDeathGlobal += TerminationDeathHook;
            //On.RoR2.Util.GetBestBodyName += AddTerminalName;
            On.RoR2.Util.GetBestBodyName += AddTerminalName2;
            RoR2.Inventory.onInventoryChangedGlobal += CheckTerminationBuff;

            markEffect = SS2Assets.LoadAsset<GameObject>("RelicOfTerminationTargetMark", SS2Bundle.Items);
            failEffect = SS2Assets.LoadAsset<GameObject>("NemmandoScepterSlashAppear", SS2Bundle.Nemmando);
            buffEffect = SS2Assets.LoadAsset<GameObject>("RelicOfTerminationBuffEffect", SS2Bundle.Items);

            //globalMarkEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/BossPositionIndicator.prefab").WaitForCompletion();

            globalMarkEffectTwo = SS2Assets.LoadAsset<GameObject>("TerminationPositionIndicator", SS2Bundle.Items);
            spawnRock1VFX = SS2Assets.LoadAsset<GameObject>("TerminationDebris1", SS2Bundle.Items);
            spawnRock2VFX = SS2Assets.LoadAsset<GameObject>("TerminationDebris2", SS2Bundle.Items);
        }

        private void CheckTerminationBuff(Inventory obj)
        {
            var master = obj.GetComponent<CharacterMaster>();
            if (master)
            {
                var body = master.GetBody();
                if (body)
                {
                    if(obj.GetItemCount(SS2Content.Items.RelicOfTermination) == 0)
                    {
                        if (body.HasBuff(SS2Content.Buffs.BuffTerminationReady))
                        {
                            body.RemoveBuff(SS2Content.Buffs.BuffTerminationReady);
                            var token = body.GetComponent<TerminationHolderToken>();
                            if (token)
                            {
                                //SS2Log.Info("Destroying token");

                                GameObject.Destroy(token);
                            }
                        }
                    }
                }
            }
        }

        private void TerminationDeathHook(DamageReport obj)
        {
            var token = obj.victimBody.gameObject.GetComponent<TerminationToken>();
            if (token)
            {
                var body = token.owner.body;
                
                var inital = token.initalTime;
                var now = Time.time;
                if (body.HasBuff(SS2Content.Buffs.BuffTerminationFailed))
                {
                    body.RemoveBuff(SS2Content.Buffs.BuffTerminationFailed);
                }
                token.owner.target = null;  // :)
                                            //SS2Log.Info("time")

                float timeMult = Mathf.Pow(1 - timeReduction, token.itemCount - 1);
                //inital = maxTime * timeMult;
                float compmaxTime = maxTime * timeMult;



                if (!(now - inital > compmaxTime))
                {
                    //int count = token.PlayerOwner.inventory.GetItemCount(SS2Content.Items.RelicOfTermination.itemIndex);
                    int count = token.itemCount;
                    Vector3 vector = Quaternion.AngleAxis(0, Vector3.up) * (Vector3.up * 20f);
                    List<PickupIndex> dropList;

                    if (terminationRNG == null)
                    {
                        terminationRNG = new Xoroshiro128Plus(Run.instance.seed);
                    }

                    float tierMult = MSUtil.InverseHyperbolicScaling(1, .1f, 2.5f, count);
                    float tier = tierMult * terminationRNG.RangeFloat(0, 100);
                    if (tier < 70f)
                    {
                        dropList = Run.instance.availableTier1DropList;
                    }
                    else if (tier < 95)
                    {
                        dropList = Run.instance.availableTier2DropList;
                    }
                    else if (tier < 99.9)
                    {
                        dropList = Run.instance.availableTier3DropList;
                    }
                    else
                    {
                        dropList = Run.instance.availableBossDropList;
                    }

                    int item = Run.instance.treasureRng.RangeInt(0, dropList.Count);
                    //SS2Log.Debug("dropping reward");
                    PickupDropletController.CreatePickupDroplet(dropList[item], obj.victim.transform.position, vector);

                    EffectData effectData = new EffectData
                    {
                        origin = obj.victimBody.transform.position,
                        scale = .5f// * (float)obj.victimBody.hullClassification
                    };
                    EffectManager.SpawnEffect(spawnRock1VFX, effectData, transmit: true);
                }

            }
        }

        //private void OnEnable()
        //{
        //    CharacterBody.onBodyStartGlobal += TerminationSpawnHook;
        //    On.RoR2.Util.GetBestBodyName += AddTerminalName;
        //    InitItem();
        //}
        //private void OnDisable()
        //{
        //    CharacterBody.onBodyStartGlobal -= TerminationSpawnHook;
        //    On.RoR2.Util.GetBestBodyName -= AddTerminalName;
        //}

        private void TerminationSpawnHook(CharacterBody obj)
        {
            //SS2Log.Info("Spawn hook activated");
            //if (!NetworkServer.active)
            //{
            //    //SS2Log.Info("TerminationSpawnHook called on Client");
            //    return;
            //}
            if (illegalMarks.Contains(obj.bodyIndex) || obj.isPlayerControlled || obj.teamComponent.teamIndex == TeamIndex.Player || obj.teamComponent.teamIndex == TeamIndex.Neutral)
            {
                //SS2Log.Info("bad mark");
                return;
            }
            if (obj.inventory)
            {
                if (obj.inventory.GetItemCount(SS2Content.Items.Cognation) > 0)
                {
                    //SS2Log.Info("cognation");
                    return;
                }
            }
            //int a = 0;
            //foreach(var player in PlayerCharacterMasterController.instances)
            //{
            //    SS2Log.Info(++a + ": player" + player + " | " + player.name);
            //}
            //a = 0;
            foreach (var player in PlayerCharacterMasterController.instances)
            {
                //++a;
                //SS2Log.Info(a + ": player: " + player + " | " + player.transform);
                if (player)
                {
                    //SS2Log.Info("no player");
                    //return;
                    if (player.master)
                    {
                        var playerbody = player.master.GetBody();
                        if (playerbody)
                        {
                            if (playerbody.HasBuff(SS2Content.Buffs.BuffTerminationReady))
                            {
                                //SS2Log.Info("player " + a + " DID have buff!!!!!!!!!!!");
                                var holderToken = playerbody.GetComponent<TerminationHolderToken>();
                                if (!holderToken)
                                {
                                    holderToken = playerbody.gameObject.AddComponent<TerminationHolderToken>();
                                }
                                else
                                {
                                    //SS2Log.Debug("token found in spawn hook");
                                }
                                //Debug.Log("spawning - has buff");
                                if (obj.inventory && !holderToken.target)
                                {
                                    int count = playerbody.GetItemCount(SS2Content.Items.RelicOfTermination);
                                    if (count == 0)
                                    {
                                        count = 1; //doing this so that if for some reason if get this buff (aetherium potion) it still does something
                                    }

                                    obj.inventory.GiveItem(SS2Content.Items.TerminationHelper);
                                    var token = obj.gameObject.AddComponent<TerminationToken>();

                                    token.itemCount = count;
                                    token.initalTime = Time.time;
                                    token.owner = holderToken;
                                    

                                    holderToken.target = token;
                                    holderToken.body = playerbody;

                                    float timeMult = Mathf.Pow(1 - timeReduction, token.itemCount - 1);
                                    float compmaxTime = maxTime * timeMult;

                                    //SS2Log.Info("max time: " + compmaxTime);
                                    token.timeLimit = compmaxTime;

                                    if (!NetworkServer.active)
                                    {
                                        return;
                                    }
                                    //obj.modelLocator.modelTransform.localScale *= scaleMod;
                                    //
                                    //if (obj.isFlying)
                                    //{
                                    //    KinematicCharacterMotor[] list = obj.GetComponentsInChildren<KinematicCharacterMotor>();
                                    //    foreach(KinematicCharacterMotor motor in list)
                                    //    {
                                    //        if (motor)
                                    //        {
                                    //            motor.SetCapsuleDimensions(motor.Capsule.radius * scaleMod, motor.CapsuleHeight * scaleMod, scaleMod);
                                    //        }
                                    //    }
                                    //   // obj.characterMotor.
                                    //}
                                    //mdlTransform.localScale *= sizeCoefficient;

                                    obj.AddBuff(SS2Content.Buffs.BuffTerminationVFX);

                                    EffectData effectData = new EffectData
                                    {
                                        origin = obj.transform.position,
                                        scale = .5f
                                    
                                    };
                                    EffectManager.SpawnEffect(spawnRock1VFX, effectData, transmit: true);
                                    //var printController = obj.modelLocator.modelTransform.GetComponent<PrintController>();
                                    ////var printController = obj.GetComponent<PrintController>();
                                    ////SS2Log.Info("trying to find controller: " + printController);
                                    //if (printController)
                                    //{
                                    //    //SS2Log.Info("found controler");
                                    //    printController.printTime = 25f;
                                    //    printController.maxPrintHeight = 100;
                                    //}

                                    //obj.teamComponent.RequestDefaultIndicator(globalMarkEffectTwo);

                                    for (int i = 0; i < Mathf.Ceil(compmaxTime); i++)
                                    {
                                        playerbody.AddTimedBuff(SS2Content.Buffs.BuffTerminationCooldown.buffIndex, i + 1);
                                    }
                                    //player.body.AddBuff(SS2Content.Buffs.BuffTerminationCooldown);
                                    playerbody.RemoveBuff(SS2Content.Buffs.BuffTerminationReady);
                                    //SS2Log.Info("finished for player " + a);
                                    break;
                                }
                                //SS2Log.Info("finished for player " + a);
                                //break;
                            }
                            else
                            {
                                //SS2Log.Info("player " + a + " did not have buff");
                            }
                        }
                        else
                        {
                            //SS2Log.Info("player did not have a body");
                        }
                    }
                    else
                    {
                        //SS2Log.Info("player didn't have a master?? " + player);
                    }

                }
                else
                {
                    //SS2Log.Info("player didn't " + player);
                }
            }
        }

        private string AddTerminalName(On.RoR2.Util.orig_GetBestBodyName orig, GameObject bodyObject)
        {
            var body = bodyObject.GetComponent<CharacterBody>();
            //SS2Log.Debug("terminal begin");
            if (body)
            {
                //SS2Log.Debug("body found");
                if (body.inventory)
                {
                    //SS2Log.Debug("inventory");
                    if (body.inventory.GetItemCount(SS2Content.Items.TerminationHelper) > 0)
                    {
                        //SS2Log.Debug("epic");
                        return "Terminal " + orig(bodyObject);
                    }
                }
            }

            return orig(bodyObject);
        }

        private string AddTerminalName2(On.RoR2.Util.orig_GetBestBodyName orig, GameObject bodyObject) //i love stealing
        {
            var result = orig(bodyObject);
            CharacterBody characterBody = bodyObject?.GetComponent<CharacterBody>();
            if (characterBody && characterBody.inventory && characterBody.inventory.GetItemCount(SS2Content.Items.TerminationHelper) > 0)
            {
                result = Language.GetStringFormatted("SS2_ITEM_RELICOFTERMINATION_PREFIX", result);
            }
            return result;
        }

        //private void InitItem()
        //{
        //    markEffect = SS2Assets.LoadAsset<GameObject>("RelicOfTerminationTargetMark", SS2Bundle.Items);
        //    failEffect = SS2Assets.LoadAsset<GameObject>("NemmandoScepterSlashAppear", SS2Bundle.Items);
        //    buffEffect = SS2Assets.LoadAsset<GameObject>("RelicOfTerminationBuffEffect", SS2Bundle.Items);
        //
        //    globalMarkEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/BossPositionIndicator.prefab").WaitForCompletion();
        //
        //    terminationRNG = new Xoroshiro128Plus(Run.instance.seed);
        //    InitializeIllegalMarkList();
        //}


        public sealed class Behavior : BaseItemBodyBehavior//, IOnKilledServerReceiver//, IOnKilledOtherServerReceiver 
        {

            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfTermination;

            private float EntranceTimer;

            //public static Xoroshiro128Plus terminationRNG;

            public bool hasInitalizedList = false;

            //[ConfigurableField(ConfigDesc = "Health multiplier grantd to marked enemy if not killed in time (1 = 100% health).")]
            //[TokenModifier(token, StatTypes.Percentage, 3)]
            //public static float effectiveRadius = 100f;

            //private GameObject prolapsedInstance;
            //public bool shouldFollow = true;
            //float time = maxTime / 6f;
            //CharacterMaster target = null;

            public new void Awake()
            {
                base.Awake();
                //SS2Log.Info("awoken!");
                EntranceTimer = 2.5f;

                //if (terminationRNG == null)
                //{
                //    terminationRNG = new Xoroshiro128Plus(Run.instance.seed);
                //}
                if (!hasInitalizedList)
                {
                    InitializeIllegalMarkList();
                    hasInitalizedList = true;
                }
                //markEffect = SS2Assets.LoadAsset<GameObject>("RelicOfTerminationTargetMark", SS2Bundle.Items);
                //failEffect = SS2Assets.LoadAsset<GameObject>("NemmandoScepterSlashAppear", SS2Bundle.Items);
                //buffEffect = SS2Assets.LoadAsset<GameObject>("RelicOfTerminationBuffEffect", SS2Bundle.Items);
                //
                //globalMarkEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/BossPositionIndicator.prefab").WaitForCompletion();
                //
                //terminationRNG = new Xoroshiro128Plus(Run.instance.seed);
                //InitializeIllegalMarkList();
                ////string fuck = "ahhh 2";
                ////SS2Log.Debug(fuck);
                //SS2Log.Info("Awakened!");
                //InitializeIllegalMarkList();

                // use body.radius / bestfitradius to scale effects
            }

            public void FixedUpdate()
            {
                if(EntranceTimer > 0)
                {
                    EntranceTimer -= Time.deltaTime;
                    return;
                }
                //moffein approved if statement
                //body.GetComponent<TerminationHolderToken>();
                //SS2Log.Info("Attempting giving buff");
                if (!body.HasBuff(SS2Content.Buffs.BuffTerminationCooldown) && !body.HasBuff(SS2Content.Buffs.BuffTerminationReady) && NetworkServer.active && body.isPlayerControlled)
                {
                    var holderToken = body.GetComponent<TerminationHolderToken>();
                    if (holderToken)
                    {
                        //SS2Log.Info("found token for buff");
                        if(holderToken.target != null && !body.HasBuff(SS2Content.Buffs.BuffTerminationFailed))
                        {
                            //SS2Log.Info("giving failed buff");
                            body.AddBuff(SS2Content.Buffs.BuffTerminationFailed);

                        }
                        else if(holderToken.target == null && body.HasBuff(SS2Content.Buffs.BuffTerminationFailed))
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                body.AddTimedBuffAuthority(SS2Content.Buffs.BuffTerminationCooldown.buffIndex, i + 1);
                            }
                        }
                        else if(holderToken.target == null)
                        {
                            //SS2Log.Info("giving success from failed path");
                            body.AddBuff(SS2Content.Buffs.BuffTerminationReady);
                        }
                    }
                    else
                    {
                        //SS2Log.Info("Gifts Ungiven");
                        body.AddBuff(SS2Content.Buffs.BuffTerminationReady);
                    }
                    //body.AddBuff(SS2Content.Buffs.BuffTerminationReady);
                }
            }

            private static void InitializeIllegalMarkList()
            {
                List<string> defaultBodyNames = new List<string>
            {
                "BrotherBody",
                "BrotherGlassBody",
                "BrotherHurtBody",
                "BrotherHauntBody",
                "ShopkeeperBody",
                "MiniVoidRaidCrabBodyBase",
                "MiniVoidRaidCrabBodyPhase1",
                "MiniVoidRaidCrabBodyPhase2",
                "MiniVoidRaidCrabBodyPhase3",
                "VoidRaidCrabJointBody",
                "VoidRaidCrabBody",
                "ScavLunar1Body",
                "ScavLunar2Body",
                "ScavLunar3Body",
                "ScavLunar4Body",
                "ScavSackProjectile",
                "ArtifactShellBody",
                "VagrantTrackingBomb",
                "LunarWispTrackingBomb",
                "GravekeeperTrackingFireball",
                "BeetleWard",
                "AffixEarthHealerBody",
                "AltarSkeletonBody",
                "DeathProjectile",
                "TimeCrystalBody",
                "SulfurPodBody",
                "FusionCellDestructibleBody",
                "ExplosivePotDestructibleBody",
                "SMInfiniteTowerMaulingRockLarge",
                "SMInfiniteTowerMaulingRockMedium",
                "SMInfiniteTowerMaulingRockSmall",
                "SMMaulingRockLarge",
                "SMMaulingRockMedium",
                "SMMaulingRockSmall",
                "VoidInfestorBody",
            };

                //SS2Log.Debug("all bodies: " + BodyCatalog.allBodyPrefabs.ToString());
                //GameObject[] list = BodyCatalog.allBodyPrefabs.ToArray();
                //int i = 0;
                //foreach(GameObject obj in list)
                //{
                //    SS2Log.Debug(i + " - obj name: " + obj.name);
                //    ++i;
                //}
                foreach (string bodyName in defaultBodyNames)
                {
                    BodyIndex index = BodyCatalog.FindBodyIndexCaseInsensitive(bodyName);
                    if (index != BodyIndex.None)
                    {
                        //Debug.Log("found body " + bodyName + " with index " + index + ", adding them");
                        //SS2Log.Info("found body " + bodyName + " with index " + index + ", adding them");
                        AddBodyToIllegalTerminationList(index);
                    }
                }
            }

            public static void AddBodyToIllegalTerminationList(BodyIndex bodyIndex)
            {
                if (bodyIndex == BodyIndex.None)
                {
                    SS2Log.Info($"Tried to add a body to the illegal termination list, but it's index is none");
                    return;
                }

                if (illegalMarks.Contains(bodyIndex))
                {
                    GameObject prefab = BodyCatalog.GetBodyPrefab(bodyIndex);
                    //SS2Log.Info($"Body prefab {prefab} is already in the illegal termination list.");
                    return;
                }
                illegalMarks.Add(bodyIndex);
                //BodiesThatGiveSuperCharge = new ReadOnlyCollection<BodyIndex>(bodiesThatGiveSuperCharge);
            }

            //public void FixedUpdate()
            //{
            //    time -= Time.deltaTime;
            //    if (time < 0 && !target)
            //    {
            //        //MarkNewEnemy();
            //        float timeMult = Mathf.Pow(1 - timeReduction, stack - 1);
            //        time = maxTime * timeMult;
            //
            //    }
            //    else if (time < 0 && target)
            //    {
            //        var targetBody = target.GetBody();
            //        if (targetBody.inventory)
            //        {
            //            var healthFrac = targetBody.healthComponent.combinedHealthFraction;
            //            //SS2Log.Debug(healthFrac);
            //            targetBody.inventory.GiveItem(SS2Content.Items.TerminationHelper);
            //            var token = targetBody.gameObject.GetComponent<TerminationToken>();
            //            //failEffect = failEffect.transform.localScale * targetBody.transform.localScale;
            //            //markEffect = SS2Assets.LoadAsset<GameObject>("NemmandoScepterSlashAppear");
            //            markEffectInstance = Object.Instantiate(failEffect, targetBody.transform);
            //            if (markEffectInstance)
            //            {
            //                markEffectInstance.transform.localScale *= targetBody.radius;
            //            }
            //
            //            //markEffect = SS2Assets.LoadAsset<GameObject>("RelicOfTerminationBuffEffect");
            //            markEffectInstance = Object.Instantiate(buffEffect, targetBody.transform);
            //            if (markEffectInstance)
            //            {
            //                markEffectInstance.transform.localScale *= targetBody.radius;
            //            }
            //
            //            targetBody.healthComponent.health = targetBody.healthComponent.health * healthFrac;
            //            targetBody.AddBuff(SS2Content.Buffs.BuffTerminationVFX);
            //            //Vector3 scaleIncrease = new Vector3(1.25f, 1.25f, 1.25f);
            //            //targetBody.transform.localScale *= 1.5f;
            //            targetBody.modelLocator.modelTransform.localScale *= 1.5f;
            //            token.hasFailed = true;
            //        }
            //
            //        //MarkNewEnemy();
            //        float timeMult = Mathf.Pow(1 - timeReduction, stack - 1);
            //        time = maxTime * timeMult;
            //    }
            //}

            //public void OnKilledOtherServer(DamageReport damageReport)
            //{
            //    var token = damageReport.victimBody.gameObject.GetComponent<TerminationToken>();
            //    if (token)
            //    {
            //        var time = token.initalTime;
            //        var now = Time.time;
            //        if (body.HasBuff(SS2Content.Buffs.BuffTerminationFailed))
            //        {
            //            token.owner.body.RemoveBuff(SS2Content.Buffs.BuffTerminationFailed);
            //        }
            //        token.owner.target = null;  // :)
            //
            //        if (!(now - time > 30))
            //        {
            //            //int count = token.PlayerOwner.inventory.GetItemCount(SS2Content.Items.RelicOfTermination.itemIndex);
            //            int count = token.itemCount;
            //            Vector3 vector = Quaternion.AngleAxis(0, Vector3.up) * (Vector3.up * 20f);
            //            List<PickupIndex> dropList;
            //
            //            float tierMult = MSUtil.InverseHyperbolicScaling(1, .1f, 2.5f, count);
            //            float tier = tierMult * terminationRNG.RangeFloat(0, 100);
            //            if (tier < 70f)
            //            {
            //                dropList = Run.instance.availableTier1DropList;
            //            }
            //            else if (tier < 95)
            //            {
            //                dropList = Run.instance.availableTier2DropList;
            //            }
            //            else if (tier < 99.9)
            //            {
            //                dropList = Run.instance.availableTier3DropList;
            //            }
            //            else
            //            {
            //                dropList = Run.instance.availableBossDropList;
            //            }
            //
            //            int item = Run.instance.treasureRng.RangeInt(0, dropList.Count);
            //            //SS2Log.Debug("dropping reward");
            //            PickupDropletController.CreatePickupDroplet(dropList[item], damageReport.victim.transform.position, vector);
            //        }
            //
            //    }
            //}

            //public void OnKilledServer(DamageReport damageReport)
            //{
            //    var token = damageReport.victimBody.gameObject.GetComponent<TerminationToken>();
            //    if (token)
            //    {
            //        var time = token.initalTime;
            //        var now = Time.time;
            //        if (body.HasBuff(SS2Content.Buffs.BuffTerminationFailed))
            //        {
            //            token.owner.body.RemoveBuff(SS2Content.Buffs.BuffTerminationFailed);
            //        }
            //        token.owner.target = null;  // :)
            //        //SS2Log.Info("time")
            //        if (!(now - time > maxTime))
            //        {
            //            //int count = token.PlayerOwner.inventory.GetItemCount(SS2Content.Items.RelicOfTermination.itemIndex);
            //            int count = token.itemCount;
            //            Vector3 vector = Quaternion.AngleAxis(0, Vector3.up) * (Vector3.up * 20f);
            //            List<PickupIndex> dropList;
            //
            //            float tierMult = MSUtil.InverseHyperbolicScaling(1, .1f, 2.5f, count);
            //            float tier = tierMult * terminationRNG.RangeFloat(0, 100);
            //            if (tier < 70f)
            //            {
            //                dropList = Run.instance.availableTier1DropList;
            //            }
            //            else if (tier < 95)
            //            {
            //                dropList = Run.instance.availableTier2DropList;
            //            }
            //            else if (tier < 99.9)
            //            {
            //                dropList = Run.instance.availableTier3DropList;
            //            }
            //            else
            //            {
            //                dropList = Run.instance.availableBossDropList;
            //            }
            //
            //            int item = Run.instance.treasureRng.RangeInt(0, dropList.Count);
            //            //SS2Log.Debug("dropping reward");
            //            PickupDropletController.CreatePickupDroplet(dropList[item], damageReport.victim.transform.position, vector);
            //        }
            //
            //    }
            //    //throw new System.NotImplementedException();
            //}

            //private void MarkNewEnemy()
            //{
            //    //SS2Log.Debug("function called");
            //    List<CharacterMaster> CharMasters(bool playersOnly = false)
            //    {
            //        return CharacterMaster.readOnlyInstancesList.Where(x => x.hasBody && x.GetBody().healthComponent.alive && (x.GetBody().teamComponent.teamIndex != body.teamComponent.teamIndex)).ToList();
            //    }
            //
            //    if (CharMasters().Count == 0)
            //    {
            //        target = null;
            //        time = maxTime / 4f;
            //        return;
            //    }
            //    int index = terminationRNG.RangeInt(0, CharMasters().Count);
            //    target = CharMasters().ElementAt(index);
            //    if (illegalMarks.Contains(target.GetBody().bodyIndex))
            //    {
            //        target = null;
            //        time = 30f; //i'm being lazy here
            //        return;
            //    }
            //    //SS2Log.Debug("found target " + target.name);
            //    if (target.GetComponent<TerminationToken>())
            //    {
            //        if (CharMasters().Count != index + 1)
            //        {
            //            target = CharMasters().ElementAt(index + 1);
            //        }
            //        else if (CharMasters().Count == index + 1)
            //        {
            //            if (index == 0)
            //            {
            //                target = null;
            //                time = 5f; //maxTime / 6f;
            //                return;
            //            }
            //            target = CharMasters().ElementAt(index - 1);
            //        }
            //        else
            //        {
            //            target = null;
            //            time = 5f; //maxTime / 6f;
            //            return;
            //        }
            //    }
            //
            //    var targetBody = target.GetBody();
            //    markEffectInstance = Object.Instantiate(markEffect, targetBody.transform);
            //    if (markEffectInstance)
            //    {
            //        markEffectInstance.transform.localScale *= targetBody.radius;
            //    }
            //
            //    // RoR2 / Base / Common / BossPositionIndicator.prefab
            //
            //    var token = targetBody.gameObject.AddComponent<TerminationToken>();
            //    //token.PlayerOwner = body;
            //    token.itemCount = body.inventory.GetItemCount(SS2Content.Items.RelicOfTermination.itemIndex);
            //    //markEffectInstance = Object.Instantiate(globalMarkEffect, targetBody.transform);
            //    targetBody.teamComponent.RequestDefaultIndicator(globalMarkEffect);
            //
            //    //time = maxTime;
            //}





        }

        public class TerminationToken : MonoBehaviour
        {
            //helps keep track of the target and player responsible
            //public CharacterBody PlayerOwner;
            public int itemCount;
            public bool hasFailed = false;
            public float initalTime;
            public TerminationHolderToken owner;
            public float timeLimit = 30;

        }
        public class TerminationHolderToken : MonoBehaviour
        {
            //helps keep track of the target and player responsible
            //public CharacterBody PlayerOwner;
            public TerminationToken target;
            public CharacterBody body;
            public PlayerCharacterMasterController player;

            //private void FixedUpdate()
            //{
            //    if(body.GetItemCount(SS2Content.Items.RelicOfTermination) < 1)
            //    {
            //        int ready = body.GetBuffCount(SS2Content.Buffs.BuffTerminationReady);
            //        if(ready > 0)
            //        {
            //            body.RemoveBuff(SS2Content.Buffs.BuffTerminationReady);
            //        } //all other buffs should time out and decay
            //
            //        Destroy(this);
            //
            //    }
            //}
        }
    }
}