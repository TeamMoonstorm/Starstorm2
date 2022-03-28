/*using Moonstorm.Components;
using Moonstorm.Starstorm2;
using Moonstorm.Components;
using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.Buffs;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Events
{
    public abstract class GenericEliteEvent : EventState
    {
        [SerializeField]
        public GameObject effectPrefab;
        [SerializeField]
        private EquipmentDef equipmentDef;
        [SerializeField]
        public string equipmentName;
        public static float fadeDuration = 7f;


        private GameObject effectInstance;
        private EventStateEffect eventStateEffect;
        public override void OnEnter()
        {
            base.OnEnter();
            equipmentDef = EquipmentCatalog.GetEquipmentDef(EquipmentCatalog.FindEquipmentIndex(equipmentName));
            Debug.Log("current equipmentDef: " + equipmentDef);
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
            CharacterBody.onBodyStartGlobal += MakeElite;
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
                CharacterBody.onBodyStartGlobal -= MakeElite;
            }
            }

        private void MakeElite(CharacterBody body)
        {
            if (!NetworkServer.active)
                return;
            //Check if body ISN'T player controlled, masterless, marked as a champion, IS on monster team - and a little RNG.
            var team = body.teamComponent.teamIndex;
            if (!(body.isPlayerControlled || body.bodyFlags == CharacterBody.BodyFlags.Masterless) && !(body.isChampion) && (team == TeamIndex.Monster) && (Util.CheckRoll(65)))
            {
                body.inventory.SetEquipmentIndex(equipmentDef.equipmentIndex);
                body.maxHealth *= 4.7f;
                body.damage *= 2f;
                body.RecalculateStats();
                Debug.Log("spawned with" + equipmentDef);
            }
        }
    }

}
*/