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
using R2API.Networking.Interfaces;
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
        
        private static bool shouldLog = false;
        [SystemInitializer]
        private static void Init()
        {
            IL.RoR2.CombatDirector.Spawn += SpawnEliteIL;
            
        }
        // 5 ish credits per second, up to 10 per second giga late game.
        public float directorTickInterval = 4f;
        [Header("Elite Director Values")]
        public float minEliteCreditPerSecond = 2f;
        public float maxEliteCreditPerSecond = 5f;
        public float initialCredits = 12f;
        private List<StackableAffix> allElites = new List<StackableAffix>();
        private Xoroshiro128Plus rng;
        private float timer = 0;
        public Xoroshiro128Plus treasureRng { get; private set; }
        public void Awake()
        {
            instance = this; //I guess there will only be one of you after all...        

            if (!NetworkServer.active) return;
            rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);
            treasureRng = new Xoroshiro128Plus(Run.instance.treasureRng);
            if(EtherealBehavior.instance)
            {
                allElites.Add(new Ethereal()); // lol what was the point
                allElites.Add(new Ultra());
            }          
            allElites.Add(new Empyrean());

            for (int i = allElites.Count - 1; i >= 0; i--)
            {
                if (allElites[i].IsAvailable())
                    allElites[i].eliteCredit += initialCredits * Run.instance.difficultyCoefficient; //////////
            }
#if DEBUG
            shouldLog = true;
#endif
        }

        public void FixedUpdate()
        {
            if(NetworkServer.active && Run.instance)
            {
                timer += Time.fixedDeltaTime;
                if (timer > directorTickInterval)
                {
                    timer = 0f;
                    float multiplier = Util.Remap(Run.instance.difficultyCoefficient, 0, 500, 1, 6); // arbitrary values. just guessing
                    float eliteCredit = (rng.RangeFloat(minEliteCreditPerSecond * directorTickInterval, maxEliteCreditPerSecond * directorTickInterval) * multiplier);
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
            if (director == null)
                return;
            if (spawnResult.spawnedInstance == null || spawnResult.spawnRequest == null || spawnResult.spawnRequest.spawnCard == null)
                return;
            CharacterMaster cm = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
            if (cm == null || cm.bodyInstanceObject == null)
                return;
            CharacterBody body = cm.bodyInstanceObject.GetComponent<CharacterBody>();
            if (body == null || body.inventory == null)
                return;
            if (spawnResult.spawnRequest.spawnCard.eliteRules != SpawnCard.EliteRules.Default)
                return;

            float baseCost = spawnResult.spawnRequest.spawnCard.directorCreditCost;
            float totalCost = baseCost;
            // yeah im hard coding it whho caresNOT ME thats who
            if(body.HasBuff(SS2Content.Buffs.BuffAffixSuperFire) || body.HasBuff(SS2Content.Buffs.BuffAffixSuperIce) || body.HasBuff(SS2Content.Buffs.BuffAffixSuperLightning) || body.HasBuff(SS2Content.Buffs.BuffAffixSuperEarth))
            {
                totalCost *= 18f;
                SS2Log.Info($"CharacterBody {body} has a Super Elite buff. Multiplying totalCost by 18 ({baseCost} => {totalCost})");             
            }
            else if (body.eliteBuffCount > 0 && director.currentActiveEliteTier != null)
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
                        affix.SubtractCost(baseCost, ref totalCost, director);
                        isBoss |= affix.IsBoss;
                    }
                }
            }           
            if(highestCostAffix != null && body.gameObject != null)
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
        [ConCommand(commandName = "elitedirector_enable_log", flags = ConVarFlags.None, helpText = "Enables/Disables CustomEliteDirector logging.")]
        public static void CCSetEliteLog(ConCommandArgs args)
        {
            shouldLog = !shouldLog;
        }
    }

    // was gonna make this work kinda like objectivetracker with subscriptions and automatic collecting but i stoppe caring half way thru
    // each elite builds up credits independently, which should make stacking more common
    // also gives us more control over how frequently each elite spawns. which is good because their spawns are almost all opt-in and conditional
    public abstract class StackableAffix
    {
        public float eliteCredit; // independent credit
        public abstract float EliteCreditCost { get; } // flat value for CustomEliteDirector elite credit cost. higher = less common/more time between spawns
        public abstract float CostMultiplier { get; } // multiplier for CombatDirector monster credit cost. this amount is subtracted from the combatdirector's credits. higher = longer "break" the player gets when this spawns
        public virtual float CostRequirement { get => CostMultiplier; } // ugh. sort of a bandaid to make stacking elites spawn more consistently.
                                                                        // vanilla combatdirectors use credits basically as soon as it gets them. so they usually dont have enough to spare for our elites
                                                                        // basically, having this be lower than the multiplier means it will spawn more often, but still use up the full credit cost.
                                                                        // i suck at this T_T   
                                                                        // in hindsight, inserting the stackable elite selection before/"during" vanilla elite selection would have been smarter than after.
        public virtual bool IsBoss => false;
        public virtual bool CanAfford(float baseCost, float totalCost, CombatDirector combatDirector)
        {
            return EliteCreditCost <= eliteCredit && totalCost * CostRequirement <= combatDirector.monsterCredit;
        }

        // this is virtual because Empyreans were subtracting extra for reasons i do not fully understand. but i already liked their spawn rate and didnt want to change it
        public virtual void SubtractCost(float baseCost, ref float totalCost, CombatDirector combatDirector) 
        {
            float originalCost = totalCost;
            totalCost *= CostMultiplier;
            combatDirector.monsterCredit -= totalCost - originalCost;
            eliteCredit -= EliteCreditCost;
        }
        public abstract bool IsAvailable();
        public abstract void MakeElite(CharacterBody body); // add items n stuff
        public virtual void SetSpawnState(EntityStateMachine bodyMachine) { } // only called on the highest cost affix
    }

    public class Empyrean : StackableAffix
    {
        public override float EliteCreditCost => 1200f;
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
        public override void SubtractCost(float baseCost, ref float totalCost, CombatDirector combatDirector)
        {
            totalCost = baseCost * CostMultiplier;
            combatDirector.monsterCredit -= baseCost * CostMultiplier * 1.5f - baseCost;
            eliteCredit -= EliteCreditCost * 1.5f;
        }

        public override void MakeElite(CharacterBody body)
        {
            if (SS2Config.enableBeta == false)
            {
                MakeEmpyreanOLDstupidFUCK(body);
                return;
            }
            var inventory = body.inventory;

            inventory.RemoveItem(RoR2Content.Items.BoostHp, inventory.GetItemCount(RoR2Content.Items.BoostHp));
            inventory.RemoveItem(RoR2Content.Items.BoostDamage, inventory.GetItemCount(RoR2Content.Items.BoostDamage));

            body.baseMaxHealth = Mathf.Max(body.baseMaxHealth, 500); /////////////////////////////////////////////////////////
            new FriendManager.SyncBaseStats(body).Send(R2API.Networking.NetworkDestination.Clients);
            inventory.GiveItem(RoR2Content.Items.BoostHp, 4000);
            inventory.GiveItem(SS2Content.Items.BoostMovespeed, 35);
            inventory.GiveItem(SS2Content.Items.BoostCooldowns, 50);
            inventory.GiveItem(RoR2Content.Items.BoostDamage, 100);
            //inventory.GiveItem(RoR2Content.Items.TeleportWhenOob); //REALLY DON'T LIKE THIS ONE. knocking enemies off the stage is a RIGHT. going to make a specific elite to replace this functionality.
            //inventory.GiveItem(RoR2Content.Items.AdaptiveArmor);
            inventory.SetEquipmentIndex(SS2Content.Equipments.AffixEmpyrean.equipmentIndex);

            int extraStages = Mathf.Max(Run.instance.stageClearCount - 7, 0);
            int extraLoops = Mathf.CeilToInt(extraStages / 5); // Run.stagesPerLoop); was removed in AC update but its 5 anyways, TODO: Make this a static int somewhere instead of magic number
            inventory.GiveItem(SS2Content.Items.MaxHealthPerMinute, 3 + extraStages * extraLoops * extraLoops * extraLoops * 2); ///
            inventory.GiveItem(SS2Content.Items.DoubleAllStats, extraLoops);
            inventory.GiveItem(SS2Content.Items.BoostCharacterSize, 15 + 20 * extraLoops); // teehee
            inventory.GiveItem(SS2Content.Items.NoSelfDamage); // >:)
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

            // create separate class for drop component. create catalog of super elites somewhere
            body.gameObject.AddComponent<OnBossKilledServer>().drop = CustomEliteDirector.instance.treasureRng.NextElementUniform(new ItemIndex[] { SS2Content.Items.ShardEarth.itemIndex, SS2Content.Items.ShardFire.itemIndex, SS2Content.Items.ShardIce.itemIndex, SS2Content.Items.ShardLightning.itemIndex }); // hi
            
            if (body.TryGetComponent(out RigidbodyMotor motor))
            {
                motor.canTakeImpactDamage = false; // i hate this shit so much. wisps and jellies take up all these spawns in the first loop and they die instantly
            }
        }
        public override void SetSpawnState(EntityStateMachine bodyMachine)
        {
            bodyMachine.initialStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.AffixEmpyrean.SpawnState));
            bodyMachine.SetNextState(new EntityStates.AffixEmpyrean.SpawnState());
        }

        public void MakeEmpyreanOLDstupidFUCK(CharacterBody body)
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
            int extraLoops = Mathf.FloorToInt(extraStages / 5); // Run.stagesPerLoop); was removed in AC update but its 5 anyways, TODO: Make this a static int somewhere instead of magic number
            //inventory.GiveItem(SS2Content.Items.DoubleAllStats, extraLoops); // it is not yet your time
            inventory.GiveItem(SS2Content.Items.BoostCharacterSize, 15 * extraLoops); // teehee
            if (body.characterMotor) body.characterMotor.mass = 2000f; // NO KNOCKBACK !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            if (body.rigidbody) body.rigidbody.mass = 2000f;
            // remove level cap
            if (Run.instance.ambientLevel >= Run.ambientLevelCap)
            {
                int extraLevels = Mathf.FloorToInt(SS2Util.AmbientLevelUncapped()) - Run.instance.ambientLevelFloor;
                inventory.GiveItem(RoR2Content.Items.LevelBonus, extraLevels);
            }

            EntityStateMachine bodyMachine = EntityStateMachine.FindByCustomName(body.gameObject, "Body");
            if (bodyMachine)
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
        }
    }

    public class Ethereal : StackableAffix
    {
        private static float baseCostMultiplier = 9; // so i can change it easier in game
        private static float baseCostRequirement = 4;
        private static float baseCreditCost = 360;
        public override float EliteCreditCost => baseCreditCost;// * Mathf.Pow(0.875f, EtherealBehavior.instance.etherealsCompleted-1);
        public override float CostMultiplier => baseCostMultiplier * Mathf.Pow(0.875f, EtherealBehavior.instance.etherealsCompleted-1);
        public override float CostRequirement => baseCostRequirement * Mathf.Pow(0.875f, EtherealBehavior.instance.etherealsCompleted-1);
        public override bool IsAvailable() => EtherealBehavior.instance && EtherealBehavior.instance.etherealsCompleted > 0;

        public override bool CanAfford(float baseCost, float totalCost, CombatDirector combatDirector)
        {
            return base.CanAfford(baseCost, totalCost, combatDirector) && combatDirector != TeleporterInteraction.instance?.bossDirector; // felt weird for tp bosses to be ethereal without shrine
        }
        public override void MakeElite(CharacterBody body)
        {
            var inventory = body.inventory;
            var ethInstance = EtherealBehavior.instance;
            int loopCount = Mathf.Max(Run.instance.loopClearCount, 1);
            int stageCount = EtherealBehavior.instance.etherealStagesCompleted;
            body.baseMaxHealth = Mathf.Max(body.baseMaxHealth, 300); /////////////////////////////////////////////////////////
            new FriendManager.SyncBaseStats(body).Send(R2API.Networking.NetworkDestination.Clients);
            inventory.GiveItem(RoR2Content.Items.BoostHp, (int)(150 + (150 * ethInstance.etherealsCompleted)));
            inventory.GiveItem(SS2Content.Items.MaxHealthPerMinute, 3 + stageCount * stageCount * 2 + Mathf.RoundToInt(loopCount * loopCount * loopCount * 0.5f));
            inventory.GiveItem(SS2Content.Items.BoostCooldowns, 15);
            inventory.GiveItem(RoR2Content.Items.BoostDamage, (int)(20 + (20 * ethInstance.etherealsCompleted * ethInstance.etherealsCompleted)));
            inventory.GiveItem(SS2Content.Items.BoostCharacterSize, 20); // MORE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            inventory.GiveItem(SS2Content.Items.EtherealItemAffix);
            if (body.characterMotor && body.characterMotor.mass < 400) body.characterMotor.mass = 400f;
            if (body.rigidbody && body.rigidbody.mass < 400) body.rigidbody.mass = 400f;
            DeathRewards rewards = body.GetComponent<DeathRewards>();
            if (rewards)
            {
                rewards.expReward *= (uint)(8 + (8 * ethInstance.etherealsCompleted));
                rewards.goldReward *= (uint)(8 + (8 * ethInstance.etherealsCompleted));
            }
            if(body.TryGetComponent(out RigidbodyMotor motor))
            {
                motor.canTakeImpactDamage = false;
            }
        }
    }
    public class Ultra : StackableAffix
    {
        private static float baseCostMultiplier = 12;
        private static float baseCostRequirement = 8;
        private static float baseCreditCost = 1000;
        public override float EliteCreditCost => baseCreditCost;// * Mathf.Pow(0.8f, EtherealBehavior.instance.etherealsCompleted - 1);
        public override float CostMultiplier => baseCostMultiplier * Mathf.Pow(0.8f, EtherealBehavior.instance.etherealsCompleted - 1);
        public override float CostRequirement => baseCostRequirement * Mathf.Pow(0.8f, EtherealBehavior.instance.etherealsCompleted - 1);
        public override bool IsAvailable() => EtherealBehavior.instance && EtherealBehavior.instance.etherealsCompleted > 0 && Run.instance.loopClearCount >= 1;
        public override bool IsBoss => true;
        public override void MakeElite(CharacterBody body)
        {
            var inventory = body.inventory;
            var ethInstance = EtherealBehavior.instance;
            int loopCount = Mathf.Max(Run.instance.loopClearCount, 1);
            int stageCount = EtherealBehavior.instance.etherealStagesCompleted;
            body.baseMaxHealth = Mathf.Max(body.baseMaxHealth, 500); /////////////////////////////////////////////////////////
            new FriendManager.SyncBaseStats(body).Send(R2API.Networking.NetworkDestination.Clients); ////////////////// make a fucking thing for this idiot
            inventory.GiveItem(RoR2Content.Items.BoostHp, (int)(1600f + (800f * ethInstance.etherealsCompleted)));
            inventory.GiveItem(SS2Content.Items.MaxHealthPerMinute, 3 + stageCount * stageCount * (1 + loopCount * loopCount) * ethInstance.etherealsCompleted * ethInstance.etherealsCompleted);
            inventory.GiveItem(SS2Content.Items.BoostCooldowns, 70);
            inventory.GiveItem(RoR2Content.Items.BoostDamage, 100);
            inventory.GiveItem(SS2Content.Items.NoSelfDamage); // >:)
            inventory.GiveItem(SS2Content.Items.AffixUltra);
            if (body.characterMotor && body.characterMotor.mass < 2000) body.characterMotor.mass = 2000f; 
            if (body.rigidbody && body.rigidbody.mass < 2000) body.rigidbody.mass = 2000f;
            DeathRewards rewards = body.GetComponent<DeathRewards>();
            if (rewards)
            {
                rewards.expReward *= (uint)(20 + (20 * ethInstance.etherealsCompleted));
                rewards.goldReward *= (uint)(20 + (20 * ethInstance.etherealsCompleted));
            }
            if (body.TryGetComponent(out RigidbodyMotor motor))
            {
                motor.canTakeImpactDamage = false;
            }
        }
        
    }
    public class OnBossKilledServer : MonoBehaviour, IOnKilledServerReceiver
    {
        public ItemIndex drop = ItemIndex.None;
        public void OnKilledServer(DamageReport damageReport)
        {
            PickupIndex drop = PickupCatalog.FindPickupIndex(this.drop);
            if (drop == PickupIndex.none) return;

            int playerCount = Run.instance.participatingPlayerCount;
            if (playerCount > 1)
            {
                float angle = 360f / (float)playerCount;
                Vector3 vector = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                int i = 0;
                while (i < playerCount)
                {
                    PickupDropletController.CreatePickupDroplet(drop, damageReport.victimBody.corePosition, vector);
                    i++;
                    vector = rotation * vector;
                }
            }
            else
            {
                PickupDropletController.CreatePickupDroplet(drop, damageReport.victimBody.corePosition, Vector3.up * 20f);
            }


        }
    }

}
