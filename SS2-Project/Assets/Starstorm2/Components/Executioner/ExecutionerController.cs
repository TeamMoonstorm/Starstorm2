using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(SkillLocator))]
    //[RequireComponent(typeof(CharacterModel))]
    public class ExecutionerController : NetworkBehaviour, IOnDamageDealtServerReceiver
    {
        private GenericSkill secondary;
        private ModelLocator modelLocator;
        private Animator modelAnimator;
        private ChildLocator childLocator;
        private float clipCycleEnd;

        private float chargeLevel = 0;
        private float targetChargeLevel;

        private Transform transformExeAxe;
        public GameObject meshExeAxe;
        private bool axeVisible = false;

        public bool hasOOB = false;
        public bool isExec = false;

        private void Awake()
        {
            secondary = GetComponent<SkillLocator>().secondary;
            modelLocator = GetComponent<ModelLocator>();
        }

        private void Start()
        {
            secondary.RemoveAllStocks();

            modelAnimator = modelLocator.modelTransform.GetComponent<Animator>();
            childLocator = modelLocator.modelTransform.GetComponent<ChildLocator>();
            if (childLocator)
                transformExeAxe = childLocator.FindChild("AxeSpawn");
            if (transformExeAxe)
                meshExeAxe = transformExeAxe.gameObject;
            meshExeAxe.SetActive(axeVisible);

            int layerIndex = modelAnimator.GetLayerIndex("Pack");
            modelAnimator.Play("IonCharge", layerIndex);
            modelAnimator.Update(0f);
        }

        private void FixedUpdate()
        {
            if (!secondary)
                return;

            targetChargeLevel = (secondary.stock * 10f / secondary.maxStock * 10f) * 0.01f; //this is stupid but it wouldn't work normally for some reason.

            if (chargeLevel != targetChargeLevel)
            {       //this is also stupid
                if (targetChargeLevel > chargeLevel)
                    chargeLevel += 0.01f;
                if (targetChargeLevel < chargeLevel)
                    chargeLevel = targetChargeLevel;
                if (chargeLevel < 0)
                    chargeLevel = 0;
                if (chargeLevel > 0.99f)
                    chargeLevel = 0.99f;
            }

            //Debug.Log("chargeLevel: " + chargeLevel);
            //Debug.Log("targetChargeLevel: " + targetChargeLevel);

            if (modelAnimator)
                modelAnimator.SetFloat("chargeLevel", chargeLevel);
        }

        public void OnDamageDealtServer(DamageReport report)
        {
            //This will break is anyone renames that skilldef's identifier
            if (report.victim.gameObject != report.attacker && !report.victimBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless) && secondary.skillDef.skillName == "ExecutionerChargeIons")
            {
                //Debug.Log("adding killcpt");
                var killComponents = report.victimBody.GetComponents<ExecutionerKillComponent>();
                foreach (var killCpt in killComponents)
                {
                    //If a kill component for this executioner already exists, reset the timer.
                    if (killCpt.attacker == gameObject)
                    {
                        killCpt.timeAlive = 0f;
                        return;
                    }
                }
                //Else add a kill component
                var killComponent = report.victim.gameObject.AddComponent<ExecutionerKillComponent>();
                killComponent.attacker = gameObject;
            }
        }

        [ClientRpc]
        public void RpcAddIonCharge()
        {
            SkillLocator skillLoc = gameObject.GetComponent<SkillLocator>();
            GenericSkill ionGunSkill = skillLoc != null ? skillLoc.secondary : null;
            if (ionGunSkill && ionGunSkill.stock < ionGunSkill.maxStock)
                ionGunSkill.AddOneStock();
        }

        /*public void OnKilledOtherServer(DamageReport damageReport)
        {
            if (damageReport.victimBody.HasBuff(SS2Content.Buffs.BuffFear))
            {
                SkillLocator skillLocator = damageReport.attackerBody.skillLocator;
                skillLocator.DeductCooldownFromAllSkillsServer(1f);
            }
        }*/
    }
}