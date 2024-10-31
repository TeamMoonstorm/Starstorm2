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
using UnityEngine.Networking;
namespace SS2.Components
{
    //This is a basic director for a custom elite to attempt to spawn.
    //Currently, it's planned to have unique directors for certain elites, due to intended scaling and overlap.


    //TO-DO:

    //figure out ethereal / ultra spawning balance. possible conflicts?

    //Condemned
    //Gilded
    //Twisted?- all of these will be like ethereals BUT they share a check. ethereal gets its own for overlap.

    //add support for appropriate hidden realms


    //maybe should've been an abstract with inheritence for different director types but scared to do that with il hook

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
        private float empyreanEliteCost = 1800f;
        private float empyreanMultiplier = 22.5f;
        public bool empyreanActive = false;

        [Header("Ultra-Related")]
        private float ultraEliteCost = 550f;
        private float ultraMultiplier = 30f;
        

        public void Awake()
        {
            instance = this; //I guess there will only be one of you after all...        
            ethInstance = EtherealBehavior.instance;

            if (!NetworkServer.active) return;
            rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);

            GameObject director = GameObject.Find("Director");
            if (director != null)
            {
                if (director.GetComponents<CombatDirector>().Length > 0 && director.GetComponents<CombatDirector>()[0] != null)
                    fastCombatDirector = director.GetComponents<CombatDirector>()[0];
                //the first director spams his shit
                if (director.GetComponents<CombatDirector>().Length > 1 && director.GetComponents<CombatDirector>()[1] != null)
                    slowCombatDirector = director.GetComponents<CombatDirector>()[1];
                //the second director loves to build big guys 
                if (fastCombatDirector == null || slowCombatDirector == null)
                {
                    SS2Log.Info("No combat director found. Killing custom director.");
                    Destroy(instance);
                }
            }
        }

        public void Start()
        {
            //Stage.onServerStageBegin += Stage_onServerStageBegin;
        }

        // this sucks
        private void Stage_onServerStageBegin(Stage stage)
        {
            DirectorAPI.Stage stageEnum = DirectorAPI.GetStageEnumFromSceneDef(stage.sceneDef);
            // and does nothing atm.
        }

        public void OnDestroy()
        {
            Stage.onServerStageBegin -= Stage_onServerStageBegin;
        }

        public void FixedUpdate()
        {
            if(NetworkServer.active)
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
            
        }


        

        public void ModifySpawn(SpawnCard.SpawnResult spawnResult)
        {
            if (spawnResult.spawnedInstance == null || spawnResult.spawnRequest == null || spawnResult.spawnRequest.spawnCard == null)
                return;
            CharacterMaster cm = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
            if (cm == null || cm.bodyInstanceObject == null)
                return;
            CharacterBody cb = cm.bodyInstanceObject.GetComponent<CharacterBody>();
            if (cb == null)
                return;
            //cardCosts.TryGetValue(cm.bodyPrefab, out float baseCost);

            if (spawnResult.spawnRequest.spawnCard.eliteRules != SpawnCard.EliteRules.Default)
                return;

            float baseCost = spawnResult.spawnRequest.spawnCard.directorCreditCost;
            float totalCost = baseCost;
            if (cb.eliteBuffCount > 0)
            {
                totalCost *= baseEliteCostMultiplier;
            }

            //if (totalCost * 2.5f <= fastCombatDirector.monsterCredit)
            //{
            //    if (followerStage && followerCost <= minionCredit)
            //    {
            //        var followerSummon = new MasterSummon();
            //        //to-do: get position of nearby air node
            //        followerSummon.position = cb.corePosition + (Vector3.up * 3);
            //        followerSummon.masterPrefab = Monsters.Lamp._masterPrefab;
            //        followerSummon.summonerBodyObject = cb.gameObject;
            //        var followerMaster = followerSummon.Perform();
            //        fastCombatDirector.monsterCredit -= followerCost;
            //        minionCredit -= followerCost;
            //        if (followerMaster)
            //        {
            //            var masterEquip = cb.inventory.GetEquipmentIndex();
            //            if (masterEquip != EquipmentIndex.None)
            //            {
            //                followerMaster.inventory.SetEquipmentIndex(masterEquip);
            //                fastCombatDirector.monsterCredit -= followerCost * 2f;
            //                minionCredit -= followerCost * 2f;
            //                //possibly a bug where would-be masters are getting 'follower' prefix and no minion..???
            //                //saw it once on a scav super deep loop. this isn't even programmed behavior.
            //                //why???

            //                //im thinking about it even more and it wasnt even on a map where followers were enabled at the time!!!!!!!!!!!!
            //                //WHAT THE FUCK!!!!!!!!!!!
            //            }
            //        }
            //    }
            //}

            if (Run.instance.stageClearCount > 7)
            {
                if (empyreanEliteCost <= eliteCredit && ((baseCost * empyreanMultiplier <= fastCombatDirector.monsterCredit) || (baseCost * empyreanMultiplier <= slowCombatDirector.monsterCredit)))
                {
                    MakeEmpyrean(cb);
                    fastCombatDirector.monsterCredit -= baseCost * empyreanMultiplier * 1.5f;
                    eliteCredit -= empyreanEliteCost * 1.5f;
                }
            }

            //if (ethInstance.etherealsCompleted >= 1)
            //{
            //    float baseEtherealCost = (totalCost * etherealMultiplier) / ethInstance.etherealsCompleted; //possibly too mean?? lol

            //    if (Run.instance.stageClearCount > 5)
            //    {
            //        if ((ultraEliteCost - (20 * (ethInstance.etherealsCompleted - 1))) <= eliteCredit && baseEtherealCost * 3f <= fastCombatDirector.monsterCredit)
            //        {
            //            MakeUltra(cb);
            //            fastCombatDirector.monsterCredit -= baseEtherealCost * 5f;
            //            eliteCredit -= ultraEliteCost * 1.5f;
            //        }
            //    }

            //    if ((etherealEliteCost - (20 * (ethInstance.etherealsCompleted - 1))) <= eliteCredit && baseEtherealCost <= fastCombatDirector.monsterCredit)
            //    {
            //        MakeEthereal(cb);
            //        fastCombatDirector.monsterCredit -= baseEtherealCost * 1.25f; //fuck you go broke
            //        eliteCredit -= etherealEliteCost * 1.25f;
            //    }
            //}
        }

        public void MakeEmpyrean(CharacterBody body)
        {
            var inventory = body.inventory;

            inventory.RemoveItem(RoR2Content.Items.BoostHp, inventory.GetItemCount(RoR2Content.Items.BoostHp));
            inventory.RemoveItem(RoR2Content.Items.BoostDamage, inventory.GetItemCount(RoR2Content.Items.BoostDamage));

            inventory.GiveItem(RoR2Content.Items.BoostHp, 750);
            inventory.GiveItem(SS2Content.Items.BoostMovespeed, 35);
            inventory.GiveItem(SS2Content.Items.BoostCooldowns, 50);
            inventory.GiveItem(RoR2Content.Items.BoostDamage, 60);
            //inventory.GiveItem(RoR2Content.Items.TeleportWhenOob); //REALLY DON'T LIKE THIS ONE. knocking enemies off the stage is a RIGHT. going to make a specific elite to replace this functionality.
            //inventory.GiveItem(RoR2Content.Items.AdaptiveArmor);
            inventory.SetEquipmentIndex(SS2Content.Equipments.AffixEmpyrean.equipmentIndex);

            int extraStages = Mathf.Max(Run.instance.stageClearCount - 7, 0);
            int extraLoops = Mathf.FloorToInt(extraStages / Run.stagesPerLoop);
            //inventory.GiveItem(SS2Content.Items.DoubleAllStats, extraLoops); // it is not yet your time
            inventory.GiveItem(SS2Content.Items.BoostCharacterSize, 15 * extraLoops); // teehee
            if (body.characterMotor) body.characterMotor.mass = 2000f; // NO KNOCKBACK !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            if (body.rigidbody) body.rigidbody.mass = 2000f;
            // remove level cap
            if(Run.instance.ambientLevel >= Run.ambientLevelCap)
            {
                int extraLevels = Mathf.FloorToInt(SS2Util.AmbientLevelUncapped()) - Run.instance.ambientLevelFloor;
                inventory.GiveItem(RoR2Content.Items.LevelBonus, extraLevels);
            }

            EntityStateMachine bodyMachine = EntityStateMachine.FindByCustomName(body.gameObject, "Body");
            if(bodyMachine)
            {
                bodyMachine.initialStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.AffixEmpyrean.SpawnState));
                bodyMachine.SetNextState(new EntityStates.AffixEmpyrean.SpawnState()); // why does this work for stormborn but not here ?>>???
            }
                
            
            DeathRewards rewards = body.GetComponent<DeathRewards>();
            if (rewards)
            {
                rewards.expReward *= 15;
                rewards.goldReward *= 15;
            }

            empyreanActive = true;

            CombatSquad squad = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("EliteBossGroup", SS2Bundle.Equipments))?.GetComponent<CombatSquad>();
            if(squad)
            {
                squad.AddMember(body.master);
            }
            NetworkServer.Spawn(squad.gameObject);
        }

        public void MakeUltra(CharacterBody body)
        {
            var inventory = body.inventory;

            inventory.GiveItem(RoR2Content.Items.BoostHp, (int)(500f + (250f * ethInstance.etherealsCompleted)));
            //inventory.GiveItem(SS2Content.Items.BoostMovespeed, 50);
            inventory.GiveItem(SS2Content.Items.BoostCooldowns, 70);
            inventory.GiveItem(RoR2Content.Items.BoostDamage, 100);
            inventory.GiveItem(RoR2Content.Items.TeleportWhenOob); 
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
                    if (CustomEliteDirector.instance == null) return ogMethod;
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
