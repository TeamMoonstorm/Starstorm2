using RoR2;
using UnityEngine;


namespace Moonstorm.Starstorm2.Survivors
{
    public sealed class Chirr : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("ChirrBody");
        //TODO: Make this
        public override GameObject MasterPrefab { get; } = null; //Assets.Instance.MainAssetBundle.LoadAsset<GameObject>("ChirrMonsterMaster");
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("SurvivorChirr");

        public override void ModifyPrefab()
        {
            base.ModifyPrefab();

            var cb = BodyPrefab.GetComponent<CharacterBody>();
            //cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }

        /*internal override void Hook()
        {
            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageInfo) =>
            {
                if (self.body.GetComponent<ChirrInfoComponent>())
                {
                    if (damageInfo.damage != 0 && self.body.GetComponent<ChirrInfoComponent>().friend && self.body.GetComponent<ChirrInfoComponent>().sharing)
                    {
                        damageInfo.damage *= .75f;
                        self.body.GetComponent<ChirrInfoComponent>().friend.healthComponent.TakeDamage(damageInfo);
                        damageInfo.damage /= 3f;
                        orig(self, damageInfo);
                    }
                    else
                        orig(self, damageInfo);
                }
                else
                    orig(self, damageInfo);
            };
        }*/

        /*private void SetUpPassive(SkillLocator skillLocator)
        {
            var passive = skillLocator.passiveSkill;
            passive.enabled = true;
            passive.skillNameToken = "CHIRR_PASSIVE_NAME";
            passive.skillDescriptionToken = "CHIRR_PASSIVE_DESCRIPTION";
            passive.icon = Assets.Instance.MainAssetBundle.LoadAsset<Sprite>("ChirrPassive");
        }*/

        /*private void SetUpSpecials(SkillLocator skillLocator)
        {
            specialDef1 = ScriptableObject.CreateInstance<SkillDef>();
            specialDef1.activationState = new EntityStates.SerializableEntityStateType(typeof(ChirrBefriend));
            specialDef1.activationStateMachineName = "Weapon";
            specialDef1.skillName = "CHIRR_BEFRIEND_NAME";
            specialDef1.skillNameToken = "CHIRR_BEFRIEND_NAME";
            specialDef1.skillDescriptionToken = "CHIRR_BEFRIEND_DESCRIPTION";
            specialDef1.icon = Assets.Instance.MainAssetBundle.LoadAsset<Sprite>("ChirrSpecial1");
            specialDef1.baseMaxStock = 1;
            specialDef1.baseRechargeInterval = 3f;
            specialDef1.beginSkillCooldownOnSkillEnd = false;
            specialDef1.canceledFromSprinting = false;
            specialDef1.fullRestockOnAssign = true;
            specialDef1.interruptPriority = EntityStates.InterruptPriority.Skill;
            specialDef1.isCombatSkill = false;
            specialDef1.mustKeyPress = false;
            specialDef1.cancelSprintingOnActivation = false;
            specialDef1.rechargeStock = 1;
            specialDef1.requiredStock = 1;
            specialDef1.stockToConsume = 1;

            Utils.RegisterSkillDef(specialDef1, typeof(ChirrBefriend));
            SkillFamily.Variant specialVariant1 = Utils.RegisterSkillVariant(specialDef1);

            specialDef2 = ScriptableObject.CreateInstance<SkillDef>();
            specialDef2.activationState = new EntityStates.SerializableEntityStateType(typeof(ChirrLeash));
            specialDef2.activationStateMachineName = "Weapon";
            specialDef2.skillName = "CHIRR_LEASH_NAME";
            specialDef2.skillNameToken = "CHIRR_LEASH_NAME";
            specialDef2.skillDescriptionToken = "CHIRR_LEASH_DESCRIPTION";
            specialDef2.icon = Assets.Instance.MainAssetBundle.LoadAsset<Sprite>("ChirrSpecial2");
            specialDef2.baseMaxStock = 1;
            specialDef2.baseRechargeInterval = 3f;
            specialDef2.beginSkillCooldownOnSkillEnd = false;
            specialDef2.canceledFromSprinting = false;
            specialDef2.fullRestockOnAssign = true;
            specialDef2.interruptPriority = EntityStates.InterruptPriority.Skill;
            specialDef2.isCombatSkill = false;
            specialDef2.mustKeyPress = false;
            specialDef2.cancelSprintingOnActivation = false;
            specialDef2.rechargeStock = 1;
            specialDef2.requiredStock = 1;
            specialDef2.stockToConsume = 1;

            Utils.RegisterSkillDef(specialDef2, typeof(ChirrLeash));
            SkillFamily.Variant specialVariant2 = Utils.RegisterSkillVariant(specialDef2);

            skillLocator.special = Utils.RegisterSkillsToFamily(bodyPrefab, specialVariant1);
        }*/
    }
}
