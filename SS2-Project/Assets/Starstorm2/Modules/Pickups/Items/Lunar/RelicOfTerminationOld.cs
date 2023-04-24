using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]

    public sealed class RelicOfTerminationOld : ItemBase
    {
        private const string token = "SS2_ITEM_RELICOFTERMINATION_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("RelicOfTerminationOld", SS2Bundle.Items);

        [ConfigurableField(ConfigDesc = "Time, in seconds, to kill the marked enemy before going on cooldown.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        [TokenModifier("SS2_ITEM_RELICOFTERMINATION_PICKUP", StatTypes.Default, 0)]
        public static float maxTime = 25f;

        [ConfigurableField(ConfigDesc = "Time between marks in seconds.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float downTime = 30f;

        [ConfigurableField(ConfigDesc = "Percent reduction in time to kill per stack.")]
        [TokenModifier(token, StatTypes.MultiplyByN, 1, "100")]
        public static float timeReduction = .1f;

        [ConfigurableField(ConfigDesc = "Damage multiplier which is added to the marked enemy if not killed in time (1 = 100% more damage).")]
        [TokenModifier(token, StatTypes.MultiplyByN, 2, "100")]
        public static float damageMult = 2f;

        [ConfigurableField(ConfigDesc = "Health multiplier which is added to the marked enemy if not killed in time (1 = 100% more health).")]
        [TokenModifier(token, StatTypes.MultiplyByN, 3, "100")]
        public static float healthMult = 4f;

        [ConfigurableField(ConfigDesc = "Speed multiplier which is added to the marked enemy if not killed in time (1 = 100% more speed).")]
        [TokenModifier(token, StatTypes.MultiplyByN, 4, "100")]
        public static float speedMult = 1.5f;

        [ConfigurableField(ConfigDesc = "Attack speed multiplier which is added to the marked enemy if not killed in time (1 = 100% more attack speed).")]
        [TokenModifier(token, StatTypes.MultiplyByN, 5, "100")]
        public static float atkSpeedMult = 1.75f;

        public sealed class Behavior : BaseItemBodyBehavior
        {

            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfTerminationOld;

            //[ConfigurableField(ConfigDesc = "Health multiplier grantd to marked enemy if not killed in time (1 = 100% health).")]
            //[TokenModifier(token, StatTypes.Percentage, 3)]
            //public static float effectiveRadius = 100f;

            public Xoroshiro128Plus terminationRNG;

            public GameObject markEffect;
            public GameObject globalMarkEffect;
            public GameObject failEffect;
            public GameObject buffEffect;

            private GameObject markEffectInstance;

            private static List<BodyIndex> illegalMarks = new List<BodyIndex>();

            //private GameObject prolapsedInstance;
            //public bool shouldFollow = true;
            float time = maxTime / 6f;
            CharacterMaster target = null;
            //public new void Awake()
            //{
            //    base.Awake();
            //    markEffect = SS2Assets.LoadAsset<GameObject>("RelicOfTerminationTargetMark");
            //    failEffect = SS2Assets.LoadAsset<GameObject>("NemmandoScepterSlashAppear");
            //    buffEffect = SS2Assets.LoadAsset<GameObject>("RelicOfTerminationBuffEffect");
            //
            //    globalMarkEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/BossPositionIndicator.prefab").WaitForCompletion();
            //
            //    terminationRNG = new Xoroshiro128Plus(Run.instance.seed);
            //
            //    InitializeIllegalMarkList();
            //
            //    // use body.radius / bestfitradius to scale effects
            //}
            //
            //public void FixedUpdate()
            //{
            //    time -= Time.deltaTime;
            //    if (time < 0 && !target)
            //    {
            //        MarkNewEnemy();
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
            //        MarkNewEnemy();
            //        float timeMult = Mathf.Pow(1 - timeReduction, stack - 1);
            //        time = maxTime * timeMult;
            //    }
            //}
            //
            //public void OnKilledOtherServer(DamageReport damageReport)
            //{
            //    var token = damageReport.victimBody.gameObject.GetComponent<TerminationToken>();
            //    if (token)
            //    {
            //        if (!token.hasFailed)
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
            //    }
            //}
            //
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
            //
            //
            //private static void InitializeIllegalMarkList()
            //{
            //    List<string> defaultBodyNames = new List<string>
            //{
            //    "BrotherGlassBody",
            //    "BrotherHurtBody",
            //    "ShopkeeperBody",
            //    "MiniVoidRaidCrabPhase1",
            //    "MiniVoidRaidCrabPhase2",
            //    "MiniVoidRaidCrabPhase3",
            //    "VoidRaidCrabJoint",
            //    "VoidRaidCrab",
            //    "ScavLunar1Body",
            //    "ScavLunar2Body",
            //    "ScavLunar3Body",
            //    "ScavLunar4Body",
            //    "ArtifactShell",
            //};
            //
            //    foreach (string bodyName in defaultBodyNames)
            //    {
            //        BodyIndex index = BodyCatalog.FindBodyIndexCaseInsensitive(bodyName);
            //        if (index != BodyIndex.None)
            //        {
            //            AddBodyToIllegalTerminationList(index);
            //        }
            //    }
            //}
            //
            //public static void AddBodyToIllegalTerminationList(BodyIndex bodyIndex)
            //{
            //    if (bodyIndex == BodyIndex.None)
            //    {
            //        //SS2Log.Debug($"Tried to add a body to the illegal termination list, but it's index is none");
            //        return;
            //    }
            //
            //    if (illegalMarks.Contains(bodyIndex))
            //    {
            //        GameObject prefab = BodyCatalog.GetBodyPrefab(bodyIndex);
            //        //SS2Log.Debug($"Body prefab {prefab} is already in the illegal termination list.");
            //        return;
            //    }
            //    illegalMarks.Add(bodyIndex);
            //    //BodiesThatGiveSuperCharge = new ReadOnlyCollection<BodyIndex>(bodiesThatGiveSuperCharge);
            //}


        }

        //public class TerminationToken : MonoBehaviour
        //{
        //    //helps keep track of the target and player responsible
        //    //public CharacterBody PlayerOwner;
        //    public int itemCount;
        //    public bool hasFailed = false;
        //}
    }
}