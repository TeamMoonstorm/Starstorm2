using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    public sealed class RelicOfTermination: ItemBase
    {

        private const string token = "SS2_ITEM_RELICOFTERMINATION_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("RelicOfTermination");

        private static readonly SphereSearch terminationSearch = new SphereSearch();

        public sealed class Behavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfTermination;

            [ConfigurableField(ConfigDesc = "Time, in seconds, to kill the marked enemy.")]
            [TokenModifier(token, StatTypes.Default, 0)]
            public static float maxTime = 60f;

            [ConfigurableField(ConfigDesc = "Damage multiplier added to marked enemy if not killed in time (1 = 100% more damage).")]
            [TokenModifier(token, StatTypes.Percentage, 1)]
            public static float damageMult = 2f;

            [ConfigurableField(ConfigDesc = "Health multiplier added to marked enemy if not killed in time (1 = 100% more health).")]
            [TokenModifier(token, StatTypes.Percentage, 2)]
            public static float healthMult = 2f;

            [ConfigurableField(ConfigDesc = "Speed multiplier added to marked enemy if not killed in time (1 = 100% more speed).")]
            [TokenModifier(token, StatTypes.Percentage, 3)]
            public static float speedMult = 2f;

            //[ConfigurableField(ConfigDesc = "Health multiplier grantd to marked enemy if not killed in time (1 = 100% health).")]
            //[TokenModifier(token, StatTypes.Percentage, 3)]
            //public static float effectiveRadius = 100f;

            public Xoroshiro128Plus terminationRNG;

            public GameObject markEffect;

            private GameObject markEffectInstance;

            //private GameObject prolapsedInstance;
            //public bool shouldFollow = true;
            float time = maxTime/6f;
            CharacterMaster target = null;
            public void FixedUpdate()
            {
                //SS2Log.Debug("time: " + time);
                time -= Time.deltaTime;
                if (time < 0 && !target)
                {
                    SS2Log.Debug("time hit 0, enemy is dead, marking new");
                    MarkNewEnemy();
                    time = maxTime;

                    //List<CharacterMaster> CharMasters(bool playersOnly = false)
                    //{
                    //    return CharacterMaster.readOnlyInstancesList.Where(x => x.hasBody && x.GetBody().healthComponent.alive && (x.GetBody().teamComponent.teamIndex != body.teamComponent.teamIndex)).ToList();
                    //}
                    //
                    //if (terminationRNG == null)
                    //{
                    //    terminationRNG = new Xoroshiro128Plus(Run.instance.seed);
                    //}
                    ////terminationRNG.RangeInt(0, CharMasters().Count);
                    //int index = terminationRNG.RangeInt(0, CharMasters().Count);
                    //target = CharMasters().ElementAt(index);
                    //
                    //if (target.name.Contains("Mithrix"))
                    //{
                    //    if (CharMasters().Count != index + 1)
                    //    {
                    //        target = CharMasters().ElementAt(index + 1);
                    //    }
                    //    else if(CharMasters().Count == index + 1)
                    //    {
                    //        if(index == 0)
                    //        {
                    //            target = null;
                    //            time = 15;
                    //            return;
                    //        }
                    //        target = CharMasters().ElementAt(index - 1);
                    //    }
                    //    else
                    //    {
                    //        target = null;
                    //        time = 15;
                    //        return;
                    //    }
                    //}
                    //
                    //var targetBody = target.GetBody();
                    //markEffectInstance = Object.Instantiate(markEffect, targetBody.transform);
                    //var token = targetBody.gameObject.AddComponent<TerminationToken>();
                    //
                    //token.PlayerOwner = body;
                    ////target.gameObject.AddComponent<>
                    //time = 60;

                }
                else if (time < 0 && target)
                {
                    SS2Log.Debug("time hit zero, enemy is alive, buffing");
                    var targetBody = target.GetBody();
                    if (targetBody.inventory)
                    {
                        targetBody.inventory.GiveItem(SS2Content.Items.TerminationHelper);
                        var token = targetBody.gameObject.GetComponent<TerminationToken>();
                        SS2Log.Debug("destroying component");
                        Destroy(token); 
                    }

                    MarkNewEnemy();
                    time = maxTime;
                }
            }

            public void OnKilledOtherServer(DamageReport damageReport)
            {
                var token = damageReport.victimBody.gameObject.GetComponent<TerminationToken>();
                if (token)
                {
                    SS2Log.Debug("killed enemy with token, rolling reward");
                    int count = token.PlayerOwner.inventory.GetItemCount(SS2Content.Items.RelicOfTermination.itemIndex);
                    Vector3 vector = Quaternion.AngleAxis(0, Vector3.up) * (Vector3.up * 15f);
                    List<PickupIndex> dropList;

                    float tierMult = MSUtil.InverseHyperbolicScaling(1, .25f, 5, count);
                    if (terminationRNG == null)
                    {
                        terminationRNG = new Xoroshiro128Plus(Run.instance.seed);
                    }
                    float tier = tierMult * terminationRNG.RangeInt(0, 100);
                    if(tier < 60f)
                    {
                        dropList = Run.instance.availableTier1DropList;
                    }
                    else if (tier < 90)
                    {
                        dropList = Run.instance.availableTier2DropList;
                    }else if(tier < 97)
                    {
                        dropList = Run.instance.availableTier3DropList;
                    }
                    else
                    {
                        dropList = Run.instance.availableBossDropList;
                    }

                    int item = Run.instance.treasureRng.RangeInt(0, dropList.Count);
                    SS2Log.Debug("dropping reward");
                    PickupDropletController.CreatePickupDroplet(dropList[item], damageReport.victim.transform.position, vector);
                }
            }

            private void MarkNewEnemy()
            {
                SS2Log.Debug("function called");
                List<CharacterMaster> CharMasters(bool playersOnly = false)
                {
                    return CharacterMaster.readOnlyInstancesList.Where(x => x.hasBody && x.GetBody().healthComponent.alive && (x.GetBody().teamComponent.teamIndex != body.teamComponent.teamIndex)).ToList();
                }

                if (terminationRNG == null)
                {
                    terminationRNG = new Xoroshiro128Plus(Run.instance.seed);
                }
                //terminationRNG.RangeInt(0, CharMasters().Count);
                if(CharMasters().Count == 0)
                {
                    target = null;
                    time = maxTime/4f;
                    return;
                }
                int index = terminationRNG.RangeInt(0, CharMasters().Count);
                target = CharMasters().ElementAt(index);
                SS2Log.Debug("found target " + target.name);
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
                markEffect = SS2Assets.LoadAsset<GameObject>("TerminationRelicTemp");

                var targetBody = target.GetBody();
                markEffectInstance = Object.Instantiate(markEffect, targetBody.transform);
                var token = targetBody.gameObject.AddComponent<TerminationToken>();
                SS2Log.Debug("applyied token");
                token.PlayerOwner = body;
                //target.gameObject.AddComponent<>
                time = maxTime;
            }
        }

        public class TerminationToken : MonoBehaviour
        {
            //keep track of the target and player responsible
            public CharacterBody PlayerOwner;
        }
    }
}
