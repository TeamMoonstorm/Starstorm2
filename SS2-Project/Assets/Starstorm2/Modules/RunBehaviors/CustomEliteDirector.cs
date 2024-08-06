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
using R2API;
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
        public static CustomEliteDirector instance;

        [SystemInitializer]
        private static void Init()
        {
            IL.RoR2.CombatDirector.Spawn += SpawnEliteIL;
        }
        
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
        public float minionCooldown = 18f;
        private bool enableMinionDirector = true;
        private float minionCooldownTimer = 0f;
     

        private Xoroshiro128Plus rng;

        private List<EquipmentDef> eliteEquips = new List<EquipmentDef>();

        private CombatDirector fastCombatDirector;
        private CombatDirector slowCombatDirector;
        private float timer = 0;

        [Header("Ethereal-Related")]
        private float etherealEliteCost = 80f;
        private float etherealMultiplier = 2.5f;
        private EtherealBehavior ethInstance;

        [Header("Empyrean-Related")]
        private float empyreanEliteCost = 280f;
        private float empyreanMultiplier = 25f;
        public bool empyreanActive = false;

        [Header("Ultra-Related")]
        private float ultraEliteCost = 320f;
        private float ultraMultiplier = 35f;
        

        public void Awake()
        {
            instance = this; //I guess there will only be one of you after all...

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
        }

        public void Start()
        {
            Stage.onServerStageBegin += Stage_onServerStageBegin;
        }

        // this sucks
        private void Stage_onServerStageBegin(Stage stage)
        {
            DirectorAPI.Stage stageEnum = DirectorAPI.GetStageEnumFromSceneDef(stage.sceneDef);
            if (stageEnum == DirectorAPI.Stage.WetlandAspect || stageEnum == DirectorAPI.Stage.SunderedGrove || stageEnum == DirectorAPI.Stage.SkyMeadow)
            {
                followerStage = true;
            }
        }

        public void OnDestroy()
        {
            Stage.onServerStageBegin -= Stage_onServerStageBegin;
        }

        public void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (timer > directorTickRate)
            {
                timer = 0f;
                eliteCredit += (rng.RangeFloat(minEliteCreditPerTick, maxEliteCreditPerTick) * Run.instance.compensatedDifficultyCoefficient * 0.4f);
                minionCredit += (rng.RangeFloat(minMinionCreditPerTick, maxMinionCreditPerTick) * Run.instance.compensatedDifficultyCoefficient * 0.4f);
            }

            if (!enableMinionDirector)
            {
                if (minionCooldownTimer < minionCooldown)
                    minionCooldownTimer += Time.fixedDeltaTime;

                else
                {
                    enableMinionDirector = true;
                    minionCooldownTimer = 0f;
                }
            }
        }


        

        public void ModifySpawn(SpawnCard.SpawnResult spawnResult)
        {
            CharacterMaster cm = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
            if (!cm || !cm.bodyInstanceObject)
                return;
            CharacterBody cb = cm.bodyInstanceObject.GetComponent<CharacterBody>();
            if (cb == null)
                return;
            //cardCosts.TryGetValue(cm.bodyPrefab, out float baseCost);
            float baseCost = spawnResult.spawnRequest.spawnCard.directorCreditCost;
            SS2Log.Debug(cm.name + " base cost: " + baseCost);
            float totalCost = baseCost;
            if (cb.eliteBuffCount > 0)
            {
                totalCost *= baseEliteCostMultiplier;
                SS2Log.Debug(cm.name + " elite cost: " + totalCost);
            }

            if (totalCost * 2.5f <= fastCombatDirector.monsterCredit)
            {
                if (followerStage && followerCost <= minionCredit)
                {
                    var followerSummon = new MasterSummon();
                    //to-do: get position of nearby air node
                    followerSummon.position = cb.corePosition + (Vector3.up * 3);
                    followerSummon.masterPrefab = Monsters.Lamp._masterPrefab;
                    followerSummon.summonerBodyObject = cb.gameObject;
                    var followerMaster = followerSummon.Perform();
                    SS2Log.Debug("Summoned Follower");
                    fastCombatDirector.monsterCredit -= followerCost;
                    minionCredit -= followerCost;
                    if (followerMaster)
                    {
                        var masterEquip = cb.inventory.GetEquipmentIndex();
                        if (masterEquip != EquipmentIndex.None)
                        {
                            followerMaster.inventory.SetEquipmentIndex(masterEquip);
                            fastCombatDirector.monsterCredit -= followerCost * 2f;
                            minionCredit -= followerCost * 2f;
                            //possibly a bug where would-be masters are getting 'follower' prefix and no minion..???
                            //saw it once on a scav super deep loop. this isn't even programmed behavior.
                            //why???

                            //im thinking about it even more and it wasnt even on a map where followers were enabled at the time!!!!!!!!!!!!
                            //WHAT THE FUCK!!!!!!!!!!!
                        }
                    }
                }
            }

            if (Run.instance.stageClearCount > 8)
            {
                if (empyreanEliteCost <= eliteCredit && (baseCost * empyreanMultiplier <= fastCombatDirector.monsterCredit) || (baseCost * empyreanMultiplier <= slowCombatDirector.monsterCredit))
                {
                    MakeEmpyrean(cb);
                    fastCombatDirector.monsterCredit -= baseCost * empyreanMultiplier * 1.5f;
                    SS2Log.Debug(cm.name + " empyrean monster cost : " + baseCost * empyreanMultiplier);
                    eliteCredit -= empyreanEliteCost * 1.5f;
                }
            }

            if (ethInstance.etherealsCompleted >= 1)
            {
                float baseEtherealCost = (totalCost * etherealMultiplier) / ethInstance.etherealsCompleted; //possibly too mean?? lol

                if (Run.instance.stageClearCount > 5)
                {
                    if ((ultraEliteCost - (20 * (ethInstance.etherealsCompleted - 1))) <= eliteCredit && baseEtherealCost * 3f <= fastCombatDirector.monsterCredit)
                    {
                        MakeUltra(cb);
                        fastCombatDirector.monsterCredit -= baseEtherealCost * 5f;
                        SS2Log.Debug(cm.name + " ultra monster cost: " + totalCost + "  yuou are now Ultra. :D");
                        eliteCredit -= ultraEliteCost * 1.5f;
                    }
                }

                if ((etherealEliteCost - (20 * (ethInstance.etherealsCompleted - 1))) <= eliteCredit && baseEtherealCost <= fastCombatDirector.monsterCredit)
                {
                    MakeEthereal(cb);
                    fastCombatDirector.monsterCredit -= baseEtherealCost * 1.25f; //fuck you go broke
                    SS2Log.Debug(cm.name + " ethereal monster cost: " + totalCost + "    (i am making this bitch ethereal)");
                    eliteCredit -= etherealEliteCost * 1.25f;
                }
            }
        }

        public void MakeEmpyrean(CharacterBody body)
        {
            var inventory = body.inventory;

            inventory.RemoveItem(RoR2Content.Items.BoostHp, inventory.GetItemCount(RoR2Content.Items.BoostHp));
            inventory.RemoveItem(RoR2Content.Items.BoostDamage, inventory.GetItemCount(RoR2Content.Items.BoostDamage));

            inventory.GiveItem(RoR2Content.Items.BoostHp, 500);
            inventory.GiveItem(SS2Content.Items.BoostMovespeed, 35);
            inventory.GiveItem(SS2Content.Items.BoostCooldowns, 50);
            inventory.GiveItem(RoR2Content.Items.BoostDamage, 80);
            inventory.GiveItem(RoR2Content.Items.TeleportWhenOob); //REALLY DON'T LIKE THIS ONE. knocking enemies off the stage is a RIGHT. going to make a specific elite to replace this functionality.
            inventory.GiveItem(RoR2Content.Items.AdaptiveArmor);
            inventory.SetEquipmentIndex(SS2Content.Equipments.AffixEmpyrean.equipmentIndex);

            DeathRewards rewards = body.GetComponent<DeathRewards>();
            if (rewards)
            {
                rewards.expReward *= 12;
                rewards.goldReward *= 12;
            }

            empyreanActive = true;
        }

        public void MakeUltra(CharacterBody body)
        {
            var inventory = body.inventory;

            inventory.GiveItem(RoR2Content.Items.BoostHp, (int)(500f + (250f * ethInstance.etherealsCompleted)));
            //inventory.GiveItem(SS2Content.Items.BoostMovespeed, 50);
            inventory.GiveItem(SS2Content.Items.BoostCooldowns, 70);
            inventory.GiveItem(RoR2Content.Items.BoostDamage, 100);
            inventory.GiveItem(RoR2Content.Items.TeleportWhenOob); //REALLY DON'T LIKE THIS ONE. knocking enemies off the stage is a RIGHT. going to make a specific elite to replace this functionality.
            inventory.GiveItem(RoR2Content.Items.AdaptiveArmor);
            inventory.GiveItem(SS2Content.Items.AffixUltra);

            DeathRewards rewards = body.GetComponent<DeathRewards>();
            if (rewards)
            {
                rewards.expReward *= (uint)(10 + (5 * ethInstance.etherealsCompleted));
                rewards.goldReward *= (uint)(10 + (5 * ethInstance.etherealsCompleted));
            }


        }

        public void MakeEthereal(CharacterBody body)
        {
            var inventory = body.inventory;

            inventory.GiveItem(RoR2Content.Items.BoostHp, (int)(30 + (30 * ethInstance.etherealsCompleted)));
            inventory.GiveItem(SS2Content.Items.BoostCooldowns, 15);
            inventory.GiveItem(RoR2Content.Items.BoostDamage, (int)(10 + (10 * ethInstance.etherealsCompleted)));
            inventory.GiveItem(SS2Content.Items.EtherealItemAffix);

            DeathRewards rewards = body.GetComponent<DeathRewards>();
            if (rewards)
            {
                rewards.expReward *= (uint)(2 + (2 * ethInstance.etherealsCompleted));
                rewards.goldReward *= (uint)(2 + (2 * ethInstance.etherealsCompleted));
            }
        }

        private static void SpawnEliteIL(ILContext il)
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
                    if (!CustomEliteDirector.instance) return ogMethod;
                    return new Action<SpawnCard.SpawnResult>((spawnResult) =>
                    {
                        ogMethod(spawnResult);
                        CustomEliteDirector.instance.ModifySpawn(spawnResult);
                    });
                });
            }
            else
                SS2Log.Fatal("Custom Elite IL Hook Failed ! ! !");
        }

    }
}
