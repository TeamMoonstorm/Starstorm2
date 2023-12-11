using KinematicCharacterController;
using RoR2;
using RoR2.EntitlementManagement;
using RoR2.Items;
using RoR2.UI;
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

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Damage multiplier which is added to the marked enemy. (1 = 100% more damage).")]
        [TokenModifier(token, StatTypes.MultiplyByN, 2, "100")]
        public static float damageMult = 1.5f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Health multiplier which is added to the marked enemy. (1 = 100% more health).")]
        [TokenModifier(token, StatTypes.MultiplyByN, 3, "100")]
        public static float hpMult = 2.5f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Flat additional health which is added to the marked enemy. (1 = 100% more health).")]
        [TokenModifier(token, StatTypes.MultiplyByN, 7, "100")]
        public static float hpAdd = 225;

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

        public static GameObject globalMarkEffectTwo;
        public static GameObject spawnRock1VFX;
        public static GameObject spawnRock2VFX;

        public static GameObject deathHalo;
        
        public static GameObject markEffect;
        public static GameObject failEffect;
        public static GameObject buffEffect;

        public static Xoroshiro128Plus terminationRNG;

        TerminationDropTable dropTable;

        List<PickupIndex> bossOptions;

        override public void Initialize()
        {
            CharacterBody.onBodyStartGlobal += TerminationSpawnHook;
            GlobalEventManager.onCharacterDeathGlobal += TerminationDeathHook;
            On.RoR2.Util.GetBestBodyName += AddTerminalName;
            RoR2.Inventory.onInventoryChangedGlobal += CheckTerminationBuff;
            On.RoR2.TeamComponent.SetupIndicator += OverrideTerminalBossMarker;

            markEffect = SS2Assets.LoadAsset<GameObject>("RelicOfTerminationTargetMark", SS2Bundle.Items);
            failEffect = SS2Assets.LoadAsset<GameObject>("NemmandoScepterSlashAppear", SS2Bundle.Nemmando);
            buffEffect = SS2Assets.LoadAsset<GameObject>("RelicOfTerminationBuffEffect", SS2Bundle.Items);

            deathHalo = SS2Assets.LoadAsset<GameObject>("TerminationDeathHalo", SS2Bundle.Items);

            globalMarkEffectTwo = SS2Assets.LoadAsset<GameObject>("TerminationPositionIndicator", SS2Bundle.Items);
            spawnRock1VFX = SS2Assets.LoadAsset<GameObject>("TerminationDebris1", SS2Bundle.Items);
            spawnRock2VFX = SS2Assets.LoadAsset<GameObject>("TerminationDebris2", SS2Bundle.Items);

            dropTable = ScriptableObject.CreateInstance<TerminationDropTable>();
            bossOptions = new List<PickupIndex>();
        }

        private void OverrideTerminalBossMarker(On.RoR2.TeamComponent.orig_SetupIndicator orig, TeamComponent self)
        {
            if (self.body)
            {
                if (self.body.inventory)
                {
                    if (self.body.inventory.GetItemCount(SS2Content.Items.TerminationHelper) > 0 && self.gameObject.GetComponent<TerminationMarkerToken>() == null)
                    {
                        //SS2Log.Info("giving termination marker instead of boss marker");
                        self.indicator = UnityEngine.Object.Instantiate<GameObject>(globalMarkEffectTwo, self.transform);
                        self.indicator.GetComponent<PositionIndicator>().targetTransform = self.body.coreTransform;
                        Nameplate np = self.indicator.GetComponent<Nameplate>();
                        if (np)
                        {
                            np.SetBody(self.body);
                        }
                        self.gameObject.AddComponent<TerminationMarkerToken>();
                        //self.indicator = null;
                        return;
                    }
                }
            }
            orig(self);
        }

        private void CheckTerminationBuff(Inventory obj)
        {
            if (!obj)
            {
                return;
            }

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

                //SS2Log.Info("Attacker: " + obj.attacker); //attacker is ArenaMissionController in void fields, none for jellyfish suicide
                //SS2Log.Info("AttackerBody: " + obj.attackerBody);
                //SS2Log.Info("AttackerIndex: " + obj.attackerTeamIndex);

                if(obj.attackerTeamIndex == TeamIndex.None)
                    return;

                var inital = token.initalTime;
                var now = Time.time;
                if (body.HasBuff(SS2Content.Buffs.BuffTerminationFailed))
                {
                    body.RemoveBuff(SS2Content.Buffs.BuffTerminationFailed);
                }
                var timeLimit = token.owner.target.timeLimit;
                token.owner.target = null; // :)
                //SS2Log.Info("time limit: " + timeLimit);
                //float timeMult = Mathf.Pow(1 - timeReduction, token.itemCount - 1);
                //float compmaxTime = maxTime * timeMult;

                var pointerToken = obj.victimBody.transform.Find("TerminationPositionIndicator(Clone)");
                if (pointerToken)
                {
                    var posind = pointerToken.GetComponent<PositionIndicator>();
                    //SS2Log.Info("posind: " + posind);
                    if (posind)
                    {
                        var insobj = posind.insideViewObject;
                        if (insobj)
                        {
                            var transfs = insobj.GetComponentsInChildren<Transform>();
                            foreach (var trans in transfs)
                            {
                                trans.gameObject.SetActive(false);
                            }
                        }

                        var outobj = posind.outsideViewObject;
                        if (outobj)
                        {
                            var transfs = insobj.GetComponentsInChildren<Transform>();
                            foreach (var trans in transfs)
                            {
                                trans.gameObject.SetActive(false);
                            }
                        }
                    }
                }

                if (!(now - inital > timeLimit))
                {
                    int count = token.itemCount;
                    Vector3 vector = Quaternion.AngleAxis(0, Vector3.up) * (Vector3.up * 20f);
                    //List<PickupIndex> dropList;

                    if(dropTable == null)
                    {
                        //dropTable = new TerminationDropTable();
                        dropTable = ScriptableObject.CreateInstance<TerminationDropTable>();
                        //dropTable = (TerminationDropTable)ScriptableObject.CreateInstance("TerminationDropTable");
                    }

                    if (terminationRNG == null)
                    {
                        terminationRNG = new Xoroshiro128Plus(Run.instance.seed);
                    }
                    if (token.isBoss)
                    {
                        var deathRewards = ((obj.victimBody != null) ? obj.victimBody.GetComponent<DeathRewards>() : null);
                        if (deathRewards)
                        {
                            //SS2Log.Info("a: " + deathRewards.bossDropTable.GenerateDrop(terminationRNG) + " | " + deathRewards.bossPickup);
                            PickupDropletController.CreatePickupDroplet(deathRewards.bossDropTable.GenerateDrop(terminationRNG), obj.victim.transform.position, vector);
                        }
                        else
                        {
                            if (bossOptions.Count == 0)
                            {
                                var selection = Run.instance.availableBossDropList;
                                foreach (var item in selection)
                                {
                                    bossOptions.Add(item);
                                    //SS2Log.Info("item: " + item.pickupDef.nameToken);
                                }
                            }
                            Util.ShuffleList<PickupIndex>(bossOptions);
                            PickupDropletController.CreatePickupDroplet(bossOptions[0], obj.victim.transform.position, vector);
                        }
                        
                    }
                    else
                    {
                        PickupIndex ind = dropTable.GenerateDropPreReplacement(terminationRNG, count);
                        PickupDropletController.CreatePickupDroplet(ind, obj.victim.transform.position, vector);
                    }

                    //EffectData effectData = new EffectData
                    //{
                    //    origin = obj.victimBody.transform.position,
                    //    scale = .5f// * (float)obj.victimBody.hullClassification
                    //};
                    //EffectManager.SpawnEffect(deathHalo, effectData, transmit: true);
                }

            }
        }

        private void TerminationSpawnHook(CharacterBody obj)
        {
            if (illegalMarks.Contains(obj.bodyIndex) || obj.isPlayerControlled || obj.teamComponent.teamIndex == TeamIndex.Player || obj.teamComponent.teamIndex == TeamIndex.Neutral)
            {
                return;
            }
            if (obj.inventory)
            {
                if (obj.inventory.GetItemCount(SS2Content.Items.Cognation) > 0)
                {
                    return;
                }
            }
            foreach (var player in PlayerCharacterMasterController.instances)
            {
                if (player)
                {
                    if (player.master)
                    {
                        var playerbody = player.master.GetBody();
                        if (playerbody)
                        {
                            if (playerbody.HasBuff(SS2Content.Buffs.BuffTerminationReady))
                            {
                                var holderToken = playerbody.GetComponent<TerminationHolderToken>();
                                if (!holderToken)
                                {
                                    holderToken = playerbody.gameObject.AddComponent<TerminationHolderToken>();
                                }

                                if (obj.inventory && !holderToken.target)
                                {
                                    int count = playerbody.GetItemCount(SS2Content.Items.RelicOfTermination);
                                    if (count == 0)
                                    {
                                        count = 1; //doing this so that if for some reason if get this buff (aetherium potion) it still does something
                                    }

                                    var token = obj.gameObject.AddComponent<TerminationToken>();
                                    
                                    token.itemCount = count;
                                    token.initalTime = Time.time;
                                    token.owner = holderToken;

                                    holderToken.target = token;
                                    holderToken.body = playerbody;

                                    float timeMult = Mathf.Pow(1 - timeReduction, token.itemCount - 1);
                                    float compmaxTime = maxTime * timeMult;

                                    if (obj.isBoss)
                                    {
                                        //SS2Log.Info("object is boss " + obj.isBoss);
                                        token.isBoss = true;
                                        compmaxTime *= 2;
                                    }

                                    token.timeLimit = compmaxTime;
                                    if (!NetworkServer.active)
                                    {
                                        return;
                                    }

                                    obj.AddBuff(SS2Content.Buffs.BuffTerminationVFX);
                                    obj.inventory.GiveItem(SS2Content.Items.TerminationHelper);

                                    EffectData effectData = new EffectData
                                    {
                                        origin = obj.transform.position,
                                        scale = .5f
                                    
                                    };
                                    EffectManager.SpawnEffect(spawnRock1VFX, effectData, transmit: true);


                                    for (int i = 0; i < Mathf.Ceil(compmaxTime); i++)
                                    {
                                        playerbody.AddTimedBuff(SS2Content.Buffs.BuffTerminationCooldown.buffIndex, i + 1);
                                    }
                                    playerbody.RemoveBuff(SS2Content.Buffs.BuffTerminationReady);

                                    break;
                                }
                            }
                        }
                    }
                }
            }
        } //awesome

        private string AddTerminalName(On.RoR2.Util.orig_GetBestBodyName orig, GameObject bodyObject) //i love stealing
        {
            var result = orig(bodyObject);
            CharacterBody characterBody = bodyObject?.GetComponent<CharacterBody>();
            if (characterBody && characterBody.inventory && characterBody.inventory.GetItemCount(SS2Content.Items.TerminationHelper) > 0)
            {
                result = Language.GetStringFormatted("SS2_ITEM_RELICOFTERMINATION_PREFIX", result);
            }
            return result;
        }

        public sealed class TerminationBehavior : BaseItemBodyBehavior//, IOnKilledServerReceiver//, IOnKilledOtherServerReceiver 
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfTermination;

            private float EntranceTimer;

            public bool hasInitalizedList = false;

            public new void Awake()
            {
                base.Awake();
                EntranceTimer = 2.5f;

                if (!hasInitalizedList)
                {
                    InitializeIllegalMarkList();
                    hasInitalizedList = true;
                }
            }

            public void FixedUpdate()
            {
                if(EntranceTimer > 0)
                {
                    EntranceTimer -= Time.deltaTime;
                    return;
                }
                //moffein approved if statement
                if (!body.HasBuff(SS2Content.Buffs.BuffTerminationCooldown) && !body.HasBuff(SS2Content.Buffs.BuffTerminationReady) && NetworkServer.active && body.isPlayerControlled)
                {
                    var holderToken = body.GetComponent<TerminationHolderToken>();
                    if (holderToken)
                    {
                        if(holderToken.target != null && !body.HasBuff(SS2Content.Buffs.BuffTerminationFailed))
                        {
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
                            body.AddBuff(SS2Content.Buffs.BuffTerminationReady);
                        }
                    }
                    else
                    {
                        body.AddBuff(SS2Content.Buffs.BuffTerminationReady);
                    }
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

                foreach (string bodyName in defaultBodyNames)
                {
                    BodyIndex index = BodyCatalog.FindBodyIndexCaseInsensitive(bodyName);
                    if (index != BodyIndex.None)
                    {
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
                    SS2Log.Info($"Body prefab {prefab} is already in the illegal termination list.");
                    return;
                }
                illegalMarks.Add(bodyIndex);
            }
        }

        public class TerminationMarkerToken : MonoBehaviour
        {
            //i swear this makes sense
        }

        public class TerminationToken : MonoBehaviour
        {
            //helps keep track of the target and player responsible
            public int itemCount;
            public bool hasFailed = false;
            public float initalTime;
            public TerminationHolderToken owner;
            public float timeLimit = 30;
            public bool isBoss = false;
        }

        public class TerminationHolderToken : MonoBehaviour
        {
            //helps keep track of the target and player responsible
            public TerminationToken target;
            public CharacterBody body;
            public PlayerCharacterMasterController player;
        }

        public class TerminationDropTable : BasicPickupDropTable
        {
            private void AddNew(List<PickupIndex> sourceDropList, float listWeight)
            {
                if (listWeight <= 0f || sourceDropList.Count == 0)
                {
                    return;
                }
                float weight = listWeight / (float)sourceDropList.Count;
                foreach (PickupIndex value in sourceDropList)
                {
                    selector.AddChoice(value, weight);
                }
            }

            public PickupIndex GenerateDropPreReplacement(Xoroshiro128Plus rng, int count)
            {
                selector.Clear();
                AddNew(Run.instance.availableTier1DropList, tier1Weight);
                AddNew(Run.instance.availableTier2DropList, tier2Weight * (float)count);
                AddNew(Run.instance.availableTier3DropList, tier3Weight * Mathf.Pow((float)count, 1.75f)); //this is basically the shipping request code but with a slightly lower red weight scaling

                return PickupDropTable.GenerateDropFromWeightedSelection(rng, selector);
            }

            public override int GetPickupCount()
            {
                return selector.Count;
            }

            public override PickupIndex[] GenerateUniqueDropsPreReplacement(int maxDrops, Xoroshiro128Plus rng)
            {
                return PickupDropTable.GenerateUniqueDropsFromWeightedSelection(maxDrops, rng, selector);
            }

            new private float tier1Weight = .79f; //.316f;

            new private float tier2Weight = .20f; //.08f;

            new private float tier3Weight = .01f; //.004f;

            new private readonly WeightedSelection<PickupIndex> selector = new WeightedSelection<PickupIndex>(8);
        }

    }
}