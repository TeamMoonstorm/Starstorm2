using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    public sealed class RelicOfTermination: ItemBase
    {

        private const string token = "SS2_ITEM_RELICOFTERMINATION_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("RelicOfTermination");

        public sealed class Behavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfTermination;

            [ConfigurableField(ConfigDesc = "Time, in seconds, to kill the marked enemy.")]
            [TokenModifier(token, StatTypes.Default, 0)]
            public static float maxTime = 35f;

            [ConfigurableField(ConfigDesc = "Percent reduction in time to kill per stack.")]
            [TokenModifier(token, StatTypes.Percentage, 1)]
            public static float timeReduction = .1f;

            [ConfigurableField(ConfigDesc = "Damage multiplier added to marked enemy if not killed in time (1 = 100% more damage).")]
            [TokenModifier(token, StatTypes.Percentage, 2)]
            public static float damageMult = 1f;

            [ConfigurableField(ConfigDesc = "Health multiplier added to marked enemy if not killed in time (1 = 100% more health).")]
            [TokenModifier(token, StatTypes.Percentage, 3)]
            public static float healthMult = 2f;

            [ConfigurableField(ConfigDesc = "Speed multiplier added to marked enemy if not killed in time (1 = 100% more speed).")]
            [TokenModifier(token, StatTypes.Percentage, 4)]
            public static float speedMult = 1f;

            [ConfigurableField(ConfigDesc = "Attack speed multiplier added to marked enemy if not killed in time (1 = 100% more attack speed).")]
            [TokenModifier(token, StatTypes.Percentage, 5)]
            public static float atkSpeedMult = 1f;

            //[ConfigurableField(ConfigDesc = "Health multiplier grantd to marked enemy if not killed in time (1 = 100% health).")]
            //[TokenModifier(token, StatTypes.Percentage, 3)]
            //public static float effectiveRadius = 100f;

            public Xoroshiro128Plus terminationRNG;

            public GameObject markEffect;
            public GameObject globalMarkEffect;
            public GameObject failEffect;
            public GameObject buffEffect;

            private GameObject markEffectInstance;

            //private GameObject prolapsedInstance;
            //public bool shouldFollow = true;
            float time = maxTime/6f;
            CharacterMaster target = null;
            public new void Awake()
            {
                base.Awake();
                markEffect = SS2Assets.LoadAsset<GameObject>("RelicOfTerminationTargetMark");
                failEffect = SS2Assets.LoadAsset<GameObject>("NemmandoScepterSlashAppear");
                buffEffect = SS2Assets.LoadAsset<GameObject>("RelicOfTerminationBuffEffect");

                globalMarkEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/BossPositionIndicator.prefab").WaitForCompletion();

                terminationRNG = new Xoroshiro128Plus(Run.instance.seed);

                // use body.radius / bestfitradius to scale effects
            }

            public void FixedUpdate()
            {
                time -= Time.deltaTime;
                if (time < 0 && !target)
                {
                    MarkNewEnemy();
                    float timeMult = Mathf.Pow(1 - timeReduction, stack - 1);
                    time = maxTime * timeMult;

                }
                else if (time < 0 && target)
                {
                    var targetBody = target.GetBody();
                    if (targetBody.inventory)
                    {
                        var healthFrac = targetBody.healthComponent.combinedHealthFraction;
                        SS2Log.Debug(healthFrac);
                        targetBody.inventory.GiveItem(SS2Content.Items.TerminationHelper);
                        var token = targetBody.gameObject.GetComponent<TerminationToken>();
                        //failEffect = failEffect.transform.localScale * targetBody.transform.localScale;
                        //markEffect = SS2Assets.LoadAsset<GameObject>("NemmandoScepterSlashAppear");
                        markEffectInstance = Object.Instantiate(failEffect, targetBody.transform);
                        if (markEffectInstance)
                        {
                            markEffectInstance.transform.localScale *= targetBody.radius;
                        }

                        //markEffect = SS2Assets.LoadAsset<GameObject>("RelicOfTerminationBuffEffect");
                        markEffectInstance = Object.Instantiate(buffEffect, targetBody.transform);
                        if (markEffectInstance)
                        {
                            markEffectInstance.transform.localScale *= targetBody.radius;
                        }

                        targetBody.healthComponent.health = targetBody.healthComponent.health * healthFrac;

                        token.hasFailed = true;
                    }

                    MarkNewEnemy();
                    float timeMult = Mathf.Pow(1 - timeReduction, stack - 1);
                    time = maxTime * timeMult;
                }
            }

            public void OnKilledOtherServer(DamageReport damageReport)
            {
                var token = damageReport.victimBody.gameObject.GetComponent<TerminationToken>();
                if (token)
                {
                    if (!token.hasFailed)
                    {
                        int count = token.PlayerOwner.inventory.GetItemCount(SS2Content.Items.RelicOfTermination.itemIndex);
                        Vector3 vector = Quaternion.AngleAxis(0, Vector3.up) * (Vector3.up * 20f);
                        List<PickupIndex> dropList;

                        float tierMult = MSUtil.InverseHyperbolicScaling(1, .25f, 5, count);
                        float tier = tierMult * terminationRNG.RangeInt(0, 100);
                        if (tier < 60f)
                        {
                            dropList = Run.instance.availableTier1DropList;
                        }
                        else if (tier < 90)
                        {
                            dropList = Run.instance.availableTier2DropList;
                        }
                        else if (tier < 97)
                        {
                            dropList = Run.instance.availableTier3DropList;
                        }
                        else
                        {
                            dropList = Run.instance.availableBossDropList;
                        }

                        int item = Run.instance.treasureRng.RangeInt(0, dropList.Count);
                        //SS2Log.Debug("dropping reward");
                        PickupDropletController.CreatePickupDroplet(dropList[item], damageReport.victim.transform.position, vector);
                    }
                }
            }

            private void MarkNewEnemy()
            {
                //SS2Log.Debug("function called");
                List<CharacterMaster> CharMasters(bool playersOnly = false)
                {
                    return CharacterMaster.readOnlyInstancesList.Where(x => x.hasBody && x.GetBody().healthComponent.alive && (x.GetBody().teamComponent.teamIndex != body.teamComponent.teamIndex)).ToList();
                }

                if(CharMasters().Count == 0)
                {
                    target = null;
                    time = maxTime/4f;
                    return;
                }
                int index = terminationRNG.RangeInt(0, CharMasters().Count);
                target = CharMasters().ElementAt(index);
                //SS2Log.Debug("found target " + target.name);
                if (target.name.Contains("Mithrix"))
                {
                    if (CharMasters().Count != index + 1)
                    {
                        target = CharMasters().ElementAt(index + 1);
                    }
                    else if (CharMasters().Count == index + 1)
                    {
                        if (index == 0)
                        {
                            target = null;
                            time = maxTime/4f;
                            return;
                        }
                        target = CharMasters().ElementAt(index - 1);
                    }
                    else
                    {
                        target = null;
                        time = maxTime/4f;
                        return;
                    }
                }

                var targetBody = target.GetBody();
                markEffectInstance = Object.Instantiate(markEffect, targetBody.transform);
                if (markEffectInstance)
                {
                    markEffectInstance.transform.localScale *= targetBody.radius;
                }

                // RoR2 / Base / Common / BossPositionIndicator.prefab

                var token = targetBody.gameObject.AddComponent<TerminationToken>();
                token.PlayerOwner = body;

                //markEffectInstance = Object.Instantiate(globalMarkEffect, targetBody.transform);
                targetBody.teamComponent.RequestDefaultIndicator(globalMarkEffect);

                //time = maxTime;
            }
        }

        public class TerminationToken : MonoBehaviour
        {
            //helps keep track of the target and player responsible
            public CharacterBody PlayerOwner;
            public bool hasFailed = false;
        }
    }
}
