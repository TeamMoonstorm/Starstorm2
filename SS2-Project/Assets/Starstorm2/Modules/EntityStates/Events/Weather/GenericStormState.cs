using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.Buffs;
using Moonstorm.Starstorm2.Components;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Events
{
    public abstract class GenericStormState : EventState
    {
        [SerializeField]
        public GameObject effectPrefab;
        [SerializeField]
        public BuffDef stormBuffDef;
        [SerializeField]
        public GameObject encounterPrefab;
        [SerializeField]
        public float monsterCreditMultiplier = 20f;
        [SerializeField]
        public static float fadeDuration = 7f;

        public static GameObject SArain;


        private GameObject effectInstance;
        private EventStateEffect eventStateEffect;
        public override void OnEnter()
        {
            base.OnEnter();
            if (effectPrefab)
            {
                effectInstance = Object.Instantiate(effectPrefab);
                eventStateEffect = effectInstance.GetComponent<EventStateEffect>();
                if (eventStateEffect)
                {
                    eventStateEffect.intensityMultiplier = DiffScalingValue;
                }
            }
        }

        public override void StartEvent()
        {
            //SS2Log.Debug("Beginning Storm");
            base.StartEvent();
            if (eventStateEffect)
            {
                eventStateEffect.OnEffectStart();
            }


            if (NetworkServer.active)
            {
                //TO-DO: team specific events
                var enemies = TeamComponent.GetTeamMembers(TeamIndex.Monster).Concat(TeamComponent.GetTeamMembers(TeamIndex.Lunar)).Concat(TeamComponent.GetTeamMembers(TeamIndex.Void));
                foreach (var teamMember in enemies)
                {
                    BuffEnemy(teamMember.body);
                }
            }

            if (encounterPrefab)
            {
                NetworkServer.Spawn(encounterPrefab);
                CombatDirector combatDirector = encounterPrefab.GetComponent<CombatDirector>();
                if (combatDirector && Stage.instance)
                {
                    DifficultyIndex difficultyIndex = Run.instance.ruleBook.FindDifficulty();
                    DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(difficultyIndex);
                    float scalingValue = difficultyDef.scalingValue;
                    if (difficultyDef.nameToken == "SS2_DIFFICULTY_TYPHOON_NAME")
                        scalingValue++; //lol

                    float monsterCredit = monsterCreditMultiplier * Run.instance.difficultyCoefficient * scalingValue;
                    //Debug.Log("monstercredit: " + monsterCredit);
                    if (monsterCredit != null)
                        monsterCredit = 40f;
                    
                    WeightedSelection<DirectorCard> weightedSelection = Util.CreateReasonableDirectorCardSpawnList(monsterCredit, combatDirector.maximumNumberToSpawnBeforeSkipping, 6);
                    if(weightedSelection != null && combatDirector.rng != null) //adding a null check for the rng? seems to nullref 
                    {
                        DirectorCard directorCard = weightedSelection.Evaluate(combatDirector.rng.nextNormalizedFloat);
                        if (directorCard != null)
                        {
                            combatDirector.monsterCredit += monsterCredit;
                            combatDirector.OverrideCurrentMonsterCard(directorCard);
                            combatDirector.monsterSpawnTimer = 0f;
                        }
                        else
                            Debug.Log("missing director card");
                    }
                    else
                    {
                        Debug.Log("weightedselection null");
                    }

                }
            }

            CharacterBody.onBodyStartGlobal += BuffEnemy;
            RoR2.Run.onRunDestroyGlobal += RunEndRemoveStorms;
            RoR2.Stage.onServerStageComplete += StageEndRemoveStorms;
        }

        private void StageEndRemoveStorms(Stage obj)
        {
            //SS2Log.Debug("Removing active storm because the stage ended.");
            CharacterBody.onBodyStartGlobal -= BuffEnemy;
            RoR2.Run.onRunDestroyGlobal -= RunEndRemoveStorms;
            RoR2.Stage.onServerStageComplete -= StageEndRemoveStorms;
        }

        private void RunEndRemoveStorms(Run obj)
        {
            //SS2Log.Debug("Removing active storm because the run ended.");
            CharacterBody.onBodyStartGlobal -= BuffEnemy;
            RoR2.Run.onRunDestroyGlobal -= RunEndRemoveStorms;
            RoR2.Stage.onServerStageComplete -= StageEndRemoveStorms;
        }

        public override void OnExit()
        {
            base.OnExit();
            if (eventStateEffect)
            {
                eventStateEffect.OnEndingStart(fadeDuration);
            }
            if (HasWarned)
            {
                //SS2Log.Debug("Storm ended - removed hook normally.");
                CharacterBody.onBodyStartGlobal -= BuffEnemy;
                RoR2.Run.onRunDestroyGlobal -= RunEndRemoveStorms;
                RoR2.Stage.onServerStageComplete -= StageEndRemoveStorms;
                if (NetworkServer.active)
                {
                    var bodies = CharacterBody.readOnlyInstancesList;
                    foreach (var body in bodies)
                    {
                        if(body.HasBuff(stormBuffDef ?? SS2Content.Buffs.BuffStorm))
                            body.RemoveBuff(stormBuffDef ?? SS2Content.Buffs.BuffStorm);
                    }
                }
            }
        }

        private void BuffEnemy(CharacterBody body)
        {
            if (!NetworkServer.active)
                return;
            //If the body isnt on player team, isnt controlled by a player and isnt masterless (no ai). Second part is for potential mod compatibility
            var team = body.teamComponent.teamIndex;
            if (!(body.isPlayerControlled || body.bodyFlags == CharacterBody.BodyFlags.Masterless) && (team == TeamIndex.Monster || team == TeamIndex.Lunar || team == TeamIndex.Void) && !body.HasBuff(stormBuffDef ?? SS2Content.Buffs.BuffStorm))
            {
                body.AddBuff(stormBuffDef ?? SS2Content.Buffs.BuffStorm);
            }
        }
    }

}