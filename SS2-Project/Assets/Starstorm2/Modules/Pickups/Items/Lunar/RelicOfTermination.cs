using RoR2;
using RoR2.Items;
using RoR2.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using MSU;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace SS2.Items
{
    public sealed class RelicOfTermination : SS2Item, IContentPackModifier
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acRelicOfTermination", SS2Bundle.Items);

        private const string token = "SS2_ITEM_RELICOFTERMINATION_DESC";

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Time, in seconds, to kill the marked enemy before going on cooldown.")]
        [FormatToken(token, 0)]
        [FormatToken("SS2_ITEM_RELICOFTERMINATION_PICKUP", 0)]
        public static float maxTime = 30f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Percent reduction in time to kill per stack. (1 = 100% reduction, .1 = 10% reduction)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float timeReduction = .1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Damage multiplier which is added to the marked enemy. (1 = 100% more damage).")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float damageMult = 1.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Health multiplier which is added to the marked enemy. (1 = 100% more health).")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float hpMult = 2.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Flat additional health which is added to the marked enemy. (1 = 100% more health).")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 7)]
        public static float hpAdd = 225;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Speed multiplier which is added to the marked enemy. (1 = 100% more speed).")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 4)]
        public static float speedMult = .5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Attack speed multiplier which is added to the marked enemy. (1 = 100% more attack speed).")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 5)]
        public static float atkSpeedMult = 1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Scale multiplier applied to marked enemies. (1 = 100% of normal scale (no change)).")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 6)]
        public static float scaleMod = 1.5f;

        private static List<BodyIndex> illegalMarks = new List<BodyIndex>();

        public static GameObject globalMarkEffectTwo;
        public static GameObject spawnRock1VFX;
        public static GameObject spawnRock2VFX;

        public static GameObject deathHalo;

        public static GameObject markEffect;
        public static GameObject failEffect;
        public static GameObject buffEffect;

        public static Material overlayMaterial; // SS2Assets.LoadAsset<Material>("matTerminationOverlay");
        public BuffDef _buffCooldown;
        public BuffDef _buffFailed;
        public BuffDef _buffReady;//{ get; } = SS2Assets.LoadAsset<BuffDef>("BuffTerminationReady", SS2Bundle.Items);
        public BuffDef _buffVfx;//{ get; } = SS2Assets.LoadAsset<BuffDef>("BuffTerminationVFX", SS2Bundle.Items);


        public static Xoroshiro128Plus terminationRNG;

        TerminationDropTable dropTable;

        List<PickupIndex> bossOptions;

        public override void Initialize()
        {
            markEffect = AssetCollection.FindAsset<GameObject>("RelicOfTerminationTargetMark");
            //failEffect = AssetCollection.FindAsset<GameObject>("RelicOfTerminationTargetMark");
            buffEffect = AssetCollection.FindAsset<GameObject>("RelicOfTerminationBuffEffect");
            deathHalo = AssetCollection.FindAsset<GameObject>("TerminationDeathHalo");
            spawnRock1VFX = AssetCollection.FindAsset<GameObject>("TerminationDebris1");
            spawnRock2VFX = AssetCollection.FindAsset<GameObject>("TerminationDebris2");
            globalMarkEffectTwo = AssetCollection.FindAsset<GameObject>("TerminationPositionInidcator");
            overlayMaterial = AssetCollection.FindAsset<Material>("matTerminationOverlay");
            _buffCooldown = AssetCollection.FindAsset<BuffDef>("BuffTerminationCooldown");
            _buffFailed = AssetCollection.FindAsset<BuffDef>("BuffTerminationFailed");
            _buffReady = AssetCollection.FindAsset<BuffDef>("BuffTerminationReady");
            _buffVfx = AssetCollection.FindAsset<BuffDef>("BuffTerminationVFX");

            CharacterBody.onBodyStartGlobal += TerminationSpawnHook;
            GlobalEventManager.onCharacterDeathGlobal += TerminationDeathHook;
            On.RoR2.Util.GetBestBodyName += AddTerminalName;
            RoR2.Inventory.onInventoryChangedGlobal += CheckTerminationBuff;
            On.RoR2.TeamComponent.SetupIndicator += OverrideTerminalBossMarker;

            dropTable = ScriptableObject.CreateInstance<TerminationDropTable>();
            bossOptions = new List<PickupIndex>();

            BuffOverlays.AddBuffOverlay(_buffCooldown, overlayMaterial);
            BuffOverlays.AddBuffOverlay(_buffFailed, overlayMaterial);
            BuffOverlays.AddBuffOverlay(_buffReady, overlayMaterial);
            BuffOverlays.AddBuffOverlay(_buffVfx, overlayMaterial);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
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
                    if (obj.GetItemCount(SS2Content.Items.RelicOfTermination) == 0)
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

                if (obj.attackerTeamIndex == TeamIndex.None)
                    return;

                var inital = token.initalTime;
                var now = Time.time;
                if (body.HasBuff(SS2Content.Buffs.BuffTerminationFailed))
                {
                    body.RemoveBuff(SS2Content.Buffs.BuffTerminationFailed);
                }
                var timeLimit = token.owner.target.timeLimit;
                token.owner.target = null; // :)

                var pointerToken = obj.victimBody.transform.Find("TerminationPositionIndicator(Clone)");
                if (pointerToken)
                {
                    var posind = pointerToken.GetComponent<PositionIndicator>();
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

                    if (dropTable == null)
                    {
                        dropTable = ScriptableObject.CreateInstance<TerminationDropTable>();
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

        public sealed class TerminationBehavior : BaseItemBodyBehavior
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
                if (EntranceTimer > 0)
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
                        if (holderToken.target != null && !body.HasBuff(SS2Content.Buffs.BuffTerminationFailed))
                        {
                            body.AddBuff(SS2Content.Buffs.BuffTerminationFailed);

                        }
                        else if (holderToken.target == null && body.HasBuff(SS2Content.Buffs.BuffTerminationFailed))
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                body.AddTimedBuffAuthority(SS2Content.Buffs.BuffTerminationCooldown.buffIndex, i + 1);
                            }
                        }
                        else if (holderToken.target == null)
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