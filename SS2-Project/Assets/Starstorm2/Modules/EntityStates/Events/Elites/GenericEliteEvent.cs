using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.Buffs;
using Moonstorm.Starstorm2.Components;
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
            //Debug.Log("current equipmentDef: " + equipmentDef);
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
            //SS2Log.Debug("Beginning Elite Event");
            base.StartEvent();
            if (eventStateEffect)
            {
                eventStateEffect.OnEffectStart();
            }
            CharacterBody.onBodyStartGlobal += MakeElite;
            Run.onRunDestroyGlobal += RunEndRemoveEliteEvent;
            Stage.onServerStageComplete += StageEndRemoveEliteEvent;
            //On.RoR2.DeathRewards.OnKilledServer += DropAspect;
        }

        private void StageEndRemoveEliteEvent(Stage obj)
        {
            //SS2Log.Debug("Removing active elite event because the stage ended.");
            CharacterBody.onBodyStartGlobal -= MakeElite;
            Run.onRunDestroyGlobal -= RunEndRemoveEliteEvent;
            Stage.onServerStageComplete -= StageEndRemoveEliteEvent;
            //On.RoR2.DeathRewards.OnKilledServer -= DropAspect;
        }

        private void RunEndRemoveEliteEvent(Run obj)
        {
            //SS2Log.Debug("Removing active elite event because the run ended.");
            CharacterBody.onBodyStartGlobal -= MakeElite;
            Run.onRunDestroyGlobal -= RunEndRemoveEliteEvent;
            Stage.onServerStageComplete -= StageEndRemoveEliteEvent;
            //On.RoR2.DeathRewards.OnKilledServer -= DropAspect;
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
                //SS2Log.Debug("Elite event ended - removed hook normally.");
                CharacterBody.onBodyStartGlobal -= MakeElite;
                Run.onRunDestroyGlobal -= RunEndRemoveEliteEvent;
                Stage.onServerStageComplete -= StageEndRemoveEliteEvent;
                
            }
        }

        private void MakeElite(CharacterBody body)
        {
            if (!NetworkServer.active)
                return;
            //Check if body ISN'T player controlled, masterless, marked as a champion, IS on monster team - and a little RNG. (plus not being married or an earth affix bomb)
            var team = body.teamComponent.teamIndex;

            bool specialCondition = true;
            if (body.master)
            {
                if (body.master.name.Contains("AffixEarthHealerMaster)") || body.master.name == "AffixEarthHealerMaster(Clone)")
                {
                    //Debug.Log("special condition met - affix earth");
                    specialCondition = false;
                }
                if (body.master.name.Contains("LemurianBruiserFireMaster") || body.master.name.Contains("LemurianBruiserIceMaster"))
                {
                    //Debug.Log("special condition met - married");
                    specialCondition = false;
                }
            }

            if (!(body.isPlayerControlled || body.bodyFlags == CharacterBody.BodyFlags.Masterless) && !(body.isChampion) && (team == TeamIndex.Monster || team == TeamIndex.Void) && (Util.CheckRoll(80)) && specialCondition)
            {
                body.inventory.SetEquipmentIndex(equipmentDef.equipmentIndex);
                body.maxHealth *= 4.7f;
                body.damage *= 2f;
                body.RecalculateStats();
                //Debug.Log("spawned with" + equipmentDef);
            }
        }

        /*private void DropAspect(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport damageReport)
        {
            if (damageReport.victimBody.inventory.currentEquipmentIndex == equipmentDef.equipmentIndex)
            {
                if (damageReport.victimBody.teamComponent.teamIndex == TeamIndex.Monster || damageReport.victimBody.teamComponent.teamIndex == TeamIndex.Lunar || damageReport.victimBody.teamComponent.teamIndex == TeamIndex.Void)
                {
                    if (Util.CheckRoll(50)) //0009765625f
                    {
                        //Debug.Log("rolled");
                        Vector3 vector = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 15f + Vector3.forward * (4.75f + (.25f * 1)));
                        PickupDropletController.CreatePickupDroplet(equipmentDef., damageReport.victimBody.transform.position, vector);
 
                    }
                }
            }
        }*/
    }

}
