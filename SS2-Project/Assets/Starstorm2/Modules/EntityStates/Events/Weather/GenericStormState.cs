/*using Moonstorm.Components;
using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.Buffs;
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
        public static float fadeDuration = 7f;


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
                    eventStateEffect.intensityMultiplier = difficultyScalingValue;
                }
            }
        }

        public override void StartEvent()
        {
            base.StartEvent();
            if (eventStateEffect)
            {
                eventStateEffect.OnEffectStart();
            }
            if (NetworkServer.active)
            {
                var enemies = TeamComponent.GetTeamMembers(TeamIndex.Monster).Concat(TeamComponent.GetTeamMembers(TeamIndex.Lunar));
                foreach (var teamMember in enemies)
                {
                    BuffEnemy(teamMember.body);
                }
            }
            CharacterBody.onBodyStartGlobal += BuffEnemy;
        }

        public override void OnExit()
        {
            base.OnExit();
            if (eventStateEffect)
            {
                eventStateEffect.OnEndingStart(fadeDuration);
            }
            if (hasWarned)
            {
                CharacterBody.onBodyStartGlobal -= BuffEnemy;
                if(NetworkServer.active)
                {
                    var bodies = CharacterBody.readOnlyInstancesList;
                    foreach (var body in bodies)
                    {
                        if(body.HasBuff(stormBuffDef ?? Starstorm2Content.Buffs.BuffStorm))
                            body.RemoveBuff(stormBuffDef ?? Starstorm2Content.Buffs.BuffStorm);
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
            if (!(body.isPlayerControlled || body.bodyFlags == CharacterBody.BodyFlags.Masterless) && (team == TeamIndex.Monster || team == TeamIndex.Lunar) && !body.HasBuff(stormBuffDef ?? Starstorm2Content.Buffs.BuffStorm))
            {
                body.AddBuff(stormBuffDef ?? Starstorm2Content.Buffs.BuffStorm);
            }
        }
    }

}
*/