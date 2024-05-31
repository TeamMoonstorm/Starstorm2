using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using static RoR2.CombatDirector;

namespace SS2.Components
{
    //This is a basic director for a custom elite to attempt to spawn.
    //Currently, it's planned to have unique directors for certain elites, due to intended scaling and overlap.


    //TO-DO:

    //Ethereals- unique director, tied to Ethereal completion count, can overlap with other types
    //DONE!!!!!!!!!!!!!!

    //Condemned
    //Gilded
    //Twisted?- all of these will be like ethereals BUT they share a check. ethereal gets its own for overlap.

    //Empyreans- hijack vanilla director to essentially add as T3

    //Ultras- mix of above + ethereal? post-ethereal tier 4? honestly, idk entirely :')


    //I feel like this should maybe be separate director cmponents inheriting from one class but will figure that out & possibly regret it when I get there!!!
    //idk if i can make this an abstract and then inherit the il hook so its a monolith.
    //SORRY!!!!!!!!!!

    public class CustomEliteDirector : MonoBehaviour
    {
        public float directorTickRate = 4f;
        //numbers currently based off of Instinct.
        [Header("Elite Director Values")]
        public float eliteCredit;
        public float minEliteCreditPerTick = 1.6f;
        public float maxEliteCreditPerTick = 5.2f;

        [Header("Minion Director Values")]
        public float minionCredit;
        public float minMinionCreditPerTick = 0.2f;
        public float maxMinionCreditPerTick = 4.6f;
        public float followerCost = 28f;
        private bool followerStage = false;

        private Xoroshiro128Plus rng;

        private DirectorCardCategorySelection dccs = null;
        private Dictionary<GameObject, float> cardCosts = new Dictionary<GameObject, float>();
        private List<EquipmentDef> eliteEquips = new List<EquipmentDef>();
        private bool hasMadeCardCosts = false;
        private CombatDirector fastCombatDirector;
        private CombatDirector slowCombatDirector;
        private float timer = 0;

        [Header("Ethereal-Related")]
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
                fastCombatDirector = director.GetComponents<CombatDirector>()[0];
                //the first director spams his shit
                slowCombatDirector = director.GetComponents<CombatDirector>()[1];
                //the second director loves to build big guys 
                if (!fastCombatDirector || !slowCombatDirector)
                {
                    SS2Log.Fatal("No combat director found. Killing custom director.");
                    Destroy(this);
                }
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

                        /*foreach (var key in cardCosts)
                        {
                            Debug.Log("PAIR: " + key.Key.name + " | " + key.Value);
                        }*/
                    }
                }
                else
                {
                    SS2Log.Fatal("No ClassicStageInfo found. Killing custom director.");
                    Destroy(this);
                }

            }
        }

        public void Start()
        {
            //CharacterBody.onBodyStartGlobal += TryEliteSpawn;
            IL.RoR2.CombatDirector.Spawn += SpawnEliteIL;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
        }

        public void OnDestroy()
        {
            //CharacterBody.onBodyStartGlobal -= TryEliteSpawn;
            IL.RoR2.CombatDirector.Spawn -= SpawnEliteIL;
            On.RoR2.SceneDirector.Start -= SceneDirector_Start;
        }

        public void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (timer > directorTickRate)
            {
                timer = 0f;
                eliteCredit += rng.RangeFloat(minEliteCreditPerTick, maxEliteCreditPerTick);
                minionCredit += rng.RangeFloat(minMinionCreditPerTick, maxMinionCreditPerTick);
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
                        ModifySpawn(spawnResult);
                    });
                });
            }
            else
                SS2Log.Fatal("Custom Elite IL Hook Failed ! ! !");
        }

        public void ModifySpawn(SpawnCard.SpawnResult spawnResult)
        {
            Debug.Log(spawnResult.spawnedInstance.name);
            CharacterMaster cm = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
            if (cm == null)
                return;
            CharacterBody cb = cm.bodyInstanceObject.GetComponent<CharacterBody>();
            if (cb == null)
                return;
            cardCosts.TryGetValue(cm.bodyPrefab, out float baseCost);
            Debug.Log(cm.name + " base cost: " + baseCost);
            if (cb.eliteBuffCount > 0)
            {
                baseCost *= baseEliteCostMultiplier;
                Debug.Log(cm.name + " elite cost: " + baseCost);
            }

            if (baseCost * 2.5f <= fastCombatDirector.monsterCredit)
            {
                if (followerStage && followerCost <= minionCredit)
                {
                    var followerSummon = new MasterSummon();
                    followerSummon.position = cb.corePosition + (Vector3.up * 3);
                    followerSummon.masterPrefab = Monsters.Lamp._masterPrefab;
                    followerSummon.summonerBodyObject = cb.gameObject;
                    var followerMaster = followerSummon.Perform();
                    fastCombatDirector.monsterCredit -= followerCost;
                    minionCredit -= followerCost;
                    if (followerMaster)
                    {
                        var masterEquip = cb.inventory.GetEquipmentIndex();
                        if (masterEquip != null)
                        {
                            followerMaster.inventory.SetEquipmentIndex(masterEquip);
                            fastCombatDirector.monsterCredit -= followerCost * 2f;
                            minionCredit -= followerCost * 2f;
                        }
                    }
                }
            }

            if (ethInstance.etherealsCompleted >= 1)
            {
                float baseEtherealCost = (baseCost * etherealMultiplier) / ethInstance.etherealsCompleted; //possibly too mean?? lol

                if (etherealEliteCost <= eliteCredit && baseCost * baseEtherealCost <= fastCombatDirector.monsterCredit)
                {
                    MakeEthereal(cb);
                    fastCombatDirector.monsterCredit -= baseCost * baseEtherealCost * 1.25f; //fuck you go broke
                    Debug.Log(cm.name + " ethereal monster cost: " + baseCost);
                    eliteCredit -= etherealEliteCost;
                }
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

        public void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);

            var currStage = SceneManager.GetActiveScene().name;

            if (currStage == "foggyswamp" || currStage == "stadiajungle" || currStage == "skymeadow")
            {
                followerStage = true;
            }
        }
    }
}
