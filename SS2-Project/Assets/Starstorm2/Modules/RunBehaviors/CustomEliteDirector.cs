﻿using Mono.Cecil.Cil;
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

    //add stormborns to this

    public class CustomEliteDirector : MonoBehaviour
    {
        public static CustomEliteDirector instance;
        
        private static bool shouldLog;
        [SystemInitializer]
        private static void Init()
        {
            IL.RoR2.CombatDirector.Spawn += SpawnEliteIL;
            
        }
        
        public float directorTickRate = 4f;
        [Header("Elite Director Values")]
        public float minEliteCreditPerTick = 1.6f;
        public float maxEliteCreditPerTick = 5.2f;
        private List<StackableAffix> allElites = new List<StackableAffix>();
        private Xoroshiro128Plus rng;
        private float timer = 0;

        public void Awake()
        {
            instance = this; //I guess there will only be one of you after all...        

            if (!NetworkServer.active) return;
            rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);

            if(EtherealBehavior.instance)
            {
                allElites.Add(new Ethereal()); // lol what was the point
                allElites.Add(new Ultra());
            }          
            allElites.Add(new Empyrean());
#if DEBUG
            shouldLog = true;
#endif
        }

        public void FixedUpdate()
        {
            if(NetworkServer.active)
            {
                timer += Time.fixedDeltaTime;
                if (timer > directorTickRate)
                {
                    timer = 0f;
                    float eliteCredit = (rng.RangeFloat(minEliteCreditPerTick, maxEliteCreditPerTick) * Run.instance.compensatedDifficultyCoefficient * 0.4f);
                    for (int i = allElites.Count - 1; i >= 0; i--)
                    {
                        if (allElites[i].IsAvailable())
                            allElites[i].eliteCredit += eliteCredit;
                    }
                }
            }            
        }      

        public void ModifySpawn(CombatDirector director, SpawnCard.SpawnResult spawnResult)
        {
            if (spawnResult.spawnedInstance == null || spawnResult.spawnRequest == null || spawnResult.spawnRequest.spawnCard == null)
                return;
            CharacterMaster cm = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
            if (cm == null || cm.bodyInstanceObject == null)
                return;
            CharacterBody body = cm.bodyInstanceObject.GetComponent<CharacterBody>();
            if (body == null)
                return;

            if (spawnResult.spawnRequest.spawnCard.eliteRules != SpawnCard.EliteRules.Default)
                return;

            float baseCost = spawnResult.spawnRequest.spawnCard.directorCreditCost;
            float totalCost = baseCost;
            if (body.eliteBuffCount > 0)
            {
                totalCost *= director.currentActiveEliteTier.costMultiplier;
            }
            if(shouldLog)
            {
                SS2Log.Info($"CombatDirector {director.GetInstanceID()} has {director.monsterCredit} credits");
                string e = (director.currentActiveEliteDef ? director.currentActiveEliteDef.name : "None");
                SS2Log.Info($"CharacterBody {body}, Elite {e}, costs {totalCost} credits");
            }
                
            // :(
            StackableAffix highestCostAffix = null;
            bool isBoss = false;
            for(int i = allElites.Count - 1; i >= 0; i--)
            {
                StackableAffix affix = allElites[i];
                if(affix.IsAvailable())
                {
                    if(shouldLog)
                        SS2Log.Info($"{affix.GetType().Name} Available. eliteCredit: {affix.eliteCredit}, EliteDirector Cost: {affix.EliteCreditCost}, CombatDirector Cost: {affix.CostMultiplier * totalCost}");
                    if (affix.CanAfford(baseCost, totalCost, director))
                    {
                        if (shouldLog)
                            SS2Log.Info($"Could afford {affix.GetType().Name}. EliteDirector Cost: {affix.EliteCreditCost}, CombatDirector Cost: {affix.CostMultiplier * totalCost}");
                        if (highestCostAffix  == null || affix.CostMultiplier > highestCostAffix.CostMultiplier)
                        {
                            highestCostAffix = affix;
                        }
                        affix.MakeElite(body);
                        affix.SubtractCost(body, baseCost, director); // lol
                        isBoss |= affix.IsBoss;
                    }
                }
            }           
            if(highestCostAffix != null)
            {
                EntityStateMachine bodyMachine = EntityStateMachine.FindByCustomName(body.gameObject, "Body");
                if(bodyMachine)
                    highestCostAffix.SetSpawnState(bodyMachine);
            }
            if (isBoss)
            {
                CombatSquad squad = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("EliteBossGroup", SS2Bundle.Equipments))?.GetComponent<CombatSquad>();
                if (squad)
                {
                    squad.AddMember(body.master);
                }
                NetworkServer.Spawn(squad.gameObject);
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
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<Action<SpawnCard.SpawnResult>, CombatDirector, Action<SpawnCard.SpawnResult>>>((ogMethod, director) =>
                {
                    if (CustomEliteDirector.instance == null) return ogMethod;
                    return new Action<SpawnCard.SpawnResult>((spawnResult) =>
                    {
                        ogMethod(spawnResult);
                        CustomEliteDirector.instance.ModifySpawn(director, spawnResult);
                    });
                });
            }
            else
                SS2Log.Fatal("Custom Elite IL Hook Failed ! ! !");
        }
    }

    // was gonna make this work kinda like objectivetracker with subscriptions and automatic collecting but i stoppe caring half way thru
    // each elite builds up credits independently, which should make stacking more common
    // also gives us more control over how frequently each elite spawns. which is good because their spawns are almost all opt-in and conditional
    public abstract class StackableAffix
    {
        public float eliteCredit; // independent credit
        public abstract float EliteCreditCost { get; } // flat value for CustomEliteDirector elite credit cost. higher = less common
        public abstract float CostMultiplier { get; } // multiplier for CombatDirector monster credit cost. lower = bigger enemies get the affix
        public virtual bool IsBoss => false;
        public virtual bool CanAfford(float baseCost, float totalCost, CombatDirector combatDirector)
        {
            return EliteCreditCost <= eliteCredit && totalCost * CostMultiplier <= combatDirector.monsterCredit;
        }

        // this is just because Empyreans were subtracting extra for reasons i do not fully understand. but i already liked their spawn rate and didnt want to change it
        public virtual void SubtractCost(float baseCost, float totalCost, CombatDirector combatDirector) 
        {
            combatDirector.monsterCredit -= totalCost * CostMultiplier - totalCost;
            eliteCredit -= EliteCreditCost;
        }
        public abstract bool IsAvailable();
        public abstract void MakeElite(CharacterBody body); // add items n stuff
        public virtual void SetSpawnState(EntityStateMachine bodyMachine) { } // only called on the highest cost affix
    }

    public class Empyrean : StackableAffix
    {
        public override float EliteCreditCost => 1800f;
        public override float CostMultiplier => 22.5f;
        public override bool IsBoss => true;
        public override bool IsAvailable()
        {
            return Run.instance.stageClearCount > 7;
        }
        public override bool CanAfford(float baseCost, float totalCost, CombatDirector combatDirector)
        {
            return EliteCreditCost <= eliteCredit && baseCost * CostMultiplier <= combatDirector.monsterCredit; // use base cost instead of elite
        }
        public override void SubtractCost(float baseCost, float totalCost, CombatDirector combatDirector)
        {
            combatDirector.monsterCredit -= baseCost * CostMultiplier * 1.5f - baseCost;
            eliteCredit -= EliteCreditCost * 1.5f;
        }

        public override void MakeElite(CharacterBody body)
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
            inventory.GiveItem(SS2Content.Items.BoostCharacterSize, 15 + 15 * extraLoops); // teehee
            if (body.characterMotor) body.characterMotor.mass = 2000f; // NO KNOCKBACK !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            if (body.rigidbody) body.rigidbody.mass = 2000f;
            // remove level cap
            if (Run.instance.ambientLevel >= Run.ambientLevelCap)
            {
                int extraLevels = Mathf.FloorToInt(SS2Util.AmbientLevelUncapped()) - Run.instance.ambientLevelFloor;
                inventory.GiveItem(RoR2Content.Items.LevelBonus, extraLevels);
            }
            DeathRewards rewards = body.GetComponent<DeathRewards>();
            if (rewards)
            {
                rewards.expReward *= 30;
                rewards.goldReward *= 30;
            }         
        }
        public override void SetSpawnState(EntityStateMachine bodyMachine)
        {
            bodyMachine.initialStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.AffixEmpyrean.SpawnState));
            bodyMachine.SetNextState(new EntityStates.AffixEmpyrean.SpawnState());
        }
    }

    public class Ethereal : StackableAffix
    {
        public override float EliteCreditCost => 300f * Mathf.Pow(0.67f, EtherealBehavior.instance.etherealsCompleted-1);
        public override float CostMultiplier => 12f * Mathf.Pow(0.67f, EtherealBehavior.instance.etherealsCompleted-1);
        public override bool IsAvailable() => EtherealBehavior.instance && EtherealBehavior.instance.etherealsCompleted > 0;
        public override void SubtractCost(float baseCost, float totalCost, CombatDirector combatDirector)
        {
            combatDirector.monsterCredit -= totalCost * CostMultiplier * 1.25f - totalCost;
            eliteCredit -= EliteCreditCost * 1.25f;
        }
        public override void MakeElite(CharacterBody body)
        {
            var inventory = body.inventory;
            var ethInstance = EtherealBehavior.instance;
            inventory.GiveItem(RoR2Content.Items.BoostHp, (int)(100 + (100 * ethInstance.etherealsCompleted)));
            inventory.GiveItem(SS2Content.Items.BoostCooldowns, 15);
            inventory.GiveItem(RoR2Content.Items.BoostDamage, (int)(20 + (20 * ethInstance.etherealsCompleted)));
            inventory.GiveItem(SS2Content.Items.BoostCharacterSize, 20); // MORE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            inventory.GiveItem(SS2Content.Items.EtherealItemAffix);
            DeathRewards rewards = body.GetComponent<DeathRewards>();
            if (rewards)
            {
                rewards.expReward *= (uint)(8 + (8 * ethInstance.etherealsCompleted));
                rewards.goldReward *= (uint)(8 + (8 * ethInstance.etherealsCompleted));
            }
        }
    }
    public class Ultra : StackableAffix
    {
        public override float EliteCreditCost => 1200f * Mathf.Pow(0.8f, EtherealBehavior.instance.etherealsCompleted-1);
        public override float CostMultiplier => 20f * Mathf.Pow(0.8f, EtherealBehavior.instance.etherealsCompleted-1);
        public override bool IsAvailable() => EtherealBehavior.instance && EtherealBehavior.instance.etherealsCompleted > 0 && Run.instance.loopClearCount >= 1;
        public override bool IsBoss => true;
        public override void MakeElite(CharacterBody body)
        {
            var inventory = body.inventory;
            var ethInstance = EtherealBehavior.instance;
            inventory.GiveItem(RoR2Content.Items.BoostHp, (int)(500f + (250f * ethInstance.etherealsCompleted)));
            inventory.GiveItem(SS2Content.Items.BoostCooldowns, 70);
            inventory.GiveItem(RoR2Content.Items.BoostDamage, 100);
            inventory.GiveItem(SS2Content.Items.AffixUltra);

            DeathRewards rewards = body.GetComponent<DeathRewards>();
            if (rewards)
            {
                rewards.expReward *= (uint)(20 + (20 * ethInstance.etherealsCompleted));
                rewards.goldReward *= (uint)(20 + (20 * ethInstance.etherealsCompleted));
            }
        }
    }
}
