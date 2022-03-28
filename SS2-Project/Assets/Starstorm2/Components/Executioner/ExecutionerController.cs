using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(SkillLocator))]
    public class ExecutionerController : NetworkBehaviour, IOnDamageDealtServerReceiver, IOnKilledOtherServerReceiver
    {
        private GenericSkill secondary;

        private void Awake()
        {
            secondary = GetComponent<SkillLocator>().secondary;
            secondary.RemoveAllStocks();
        }

        public void OnDamageDealtServer(DamageReport report)
        {
            //This will break is anyone renames that skilldef's identifier
            if (report.victim.gameObject != report.attacker && !report.victimBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless) && (secondary.skillDef.skillName == "ExecutionerFireIonGun" || secondary.skillDef.skillName == "ExecutionerFireIonSummon"))
            {
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

        public void OnKilledOtherServer(DamageReport damageReport)
        {
            if (damageReport.victimBody.HasBuff(SS2Content.Buffs.BuffFear))
            {
                SkillLocator skillLocator = damageReport.attackerBody.skillLocator;
                skillLocator.DeductCooldownFromAllSkillsServer(1f);
            }
        }
    }
}