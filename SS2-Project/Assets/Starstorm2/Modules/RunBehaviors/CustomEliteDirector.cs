using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static RoR2.CombatDirector;

namespace SS2.Components
{
    //This is a basic director for a custom elite to attempt to spawn.
    //Currently, it's planned to have unique directors for certain elites, due to intended scaling and overlap.
    //In my head right now-
    //Ethereals- unique director, tied to Ethereal completion count, can overlap with other types
    //Condemned, Gilded, future elites that represent explicit affiliation- shared director with no overlap, added as made available
    //Empyreans- hijack vanilla director to essentially add as T3
    //Ultras- mix of above + ethereal? post-ethereal tier 4? honestly, idk entirely
    //I feel like this should maybe be separate directors inheriting from one class but will figure that out & possibly regret it when I get there!!!
    public class CustomEliteDirector : MonoBehaviour
    {
        [Header("Core Director Values")]
        public float eliteCredit;
        public float expRewardCoefficient;
        public float goldRewardCoefficient;
        public float minCreditPerTick = 2.2f;
        public float maxCreditPerTick = 5.2f;
        private Xoroshiro128Plus rng;

        private DirectorCardCategorySelection dccs = null;
        private Dictionary<GameObject, float> cardCosts = new Dictionary<GameObject, float>();
        private List<EquipmentDef> eliteEquips = new List<EquipmentDef>();
        private bool hasMadeCardCosts = false;
        private CombatDirector combatDirector;
        private float timer = 0;

        [Header("Ethereal-Related")]
        public EliteDef etherealEliteDef;
        private float etherealEliteCost = 80f;
        private float etherealMultiplier = 4f;
        private EtherealBehavior ethInstance;

        public BindingFlags allFlags = (BindingFlags)(-1);

        public void Awake()
        {
            ethInstance = EtherealBehavior.instance;

            rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);

            GameObject director = GameObject.Find("Director");
            if (director != null)
            {
                combatDirector = director.GetComponent<CombatDirector>();
                //im pretty sure this always gets the first instance of a component it finds?? in this case, aggressive director.
            }

            GameObject sceneInfo = GameObject.Find("SceneInfo");

            //Debug.Log("making card pool");
            if (sceneInfo)
            {
                ClassicStageInfo csi = sceneInfo.GetComponent<ClassicStageInfo>();
                if (csi)
                {
                    //dccs = combatDirector.monsterCards;
                    dccs = csi.monsterCategories;

                    if (!dccs)
                    {
                        //Debug.Log("dccs null!");
                    }
                    else
                    {
                        hasMadeCardCosts = true;
                        for (int i = 0; i < dccs.categories.Length; i++)
                        {
                            if (dccs.categories[i].cards == null)
                                return;

                            for (int j = 0; j < dccs.categories[i].cards.Length; j++)
                            {
                                SpawnCard sc = dccs.categories[i].cards[j].spawnCard;
                                cardCosts.Add(sc.prefab.GetComponent<CharacterMaster>().bodyPrefab, sc.directorCreditCost);
                            }
                        }

                        foreach (var key in cardCosts)
                        {
                            Debug.Log("PAIR: " + key.Key.name + " | " + key.Value);
                        }
                    }
                }

            }
        }

        public void Start()
        {
            Debug.Log("CED Start");
            //CharacterBody.onBodyStartGlobal += TryEliteSpawn;
            IL.RoR2.CombatDirector.Spawn += SpawnEliteIL;
        }

        public void OnDestroy()
        {
            //CharacterBody.onBodyStartGlobal -= TryEliteSpawn;
            IL.RoR2.CombatDirector.Spawn -= SpawnEliteIL;
        }

        public void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (timer > 3f)
            {
                timer = 0f;
                eliteCredit += rng.RangeFloat(minCreditPerTick, maxCreditPerTick);
            }
        }


        private void SpawnEliteIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            var allFlags = (BindingFlags)(-1);
            var cdMethod = typeof(CombatDirector).GetNestedTypes(allFlags).FirstOrDefault(type => type.Name.Contains("<>") && type.Name.Contains("DisplayClass")).GetMethods(allFlags).FirstOrDefault(method => method.Name.Contains("OnCardSpawned"));

            bool ILFound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(0),
                x => x.MatchLdftn(cdMethod),
                x => x.MatchNewobj(typeof(Action<SpawnCard.SpawnResult>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) 
                })));

            if (ILFound)
            {
                c.EmitDelegate<Func<Action<SpawnCard.SpawnResult>, Action<SpawnCard.SpawnResult>>>((ogMethod) =>
                {
                    return new Action<SpawnCard.SpawnResult>((spawnResult) =>
                    {
                        ogMethod(spawnResult);
                        ModifySpawnCard(spawnResult);
                    });
                });
            }
            else
                SS2Log.Fatal("Custom Elite IL Hook Failed. :(");
        }

        public void ModifySpawnCard(SpawnCard.SpawnResult spawnResult)
        {
            Debug.Log(spawnResult.spawnedInstance.name);
            CharacterMaster cm = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
            CharacterBody cb = cm.bodyInstanceObject.GetComponent<CharacterBody>();
            cardCosts.TryGetValue(cm.bodyPrefab, out float baseCost);
            Debug.Log(cm.name + " base cost: " + baseCost);
            if (cb.isElite)
            {
                baseCost *= baseEliteCostMultiplier;
            }

            float baseEtherealCost = baseCost * (etherealMultiplier / (ethInstance.etherealsCompleted + 1f));

            if (etherealEliteCost <= eliteCredit && baseCost * baseEtherealCost <= combatDirector.monsterCredit)
            {
                MakeEthereal(cb);
                combatDirector.monsterCredit -= baseCost * baseEtherealCost;
                Debug.Log(baseEtherealCost);
                eliteCredit -= etherealEliteCost;
            }
        }

        public void MakeEthereal(CharacterBody body)
        {
            var inventory = body.inventory;
            Debug.Log("making ethereal");

            inventory.GiveItem(RoR2Content.Items.BoostHp, (int)(30 + (30 * ethInstance.etherealsCompleted)));
            inventory.GiveItem(SS2Content.Items.BoostCooldowns, 30);
            inventory.GiveItem(RoR2Content.Items.BoostDamage, (int)(10 + (10 * ethInstance.etherealsCompleted)));
            inventory.GiveItem(SS2Content.Items.EtherealItemAffix);

            DeathRewards rewards = body.GetComponent<DeathRewards>();
            if (rewards)
            {
                rewards.expReward *= (uint)(2 + (2 * ethInstance.etherealsCompleted));
                rewards.goldReward *= (uint)(2 + (2 * ethInstance.etherealsCompleted));
            }
        }
    }
}
