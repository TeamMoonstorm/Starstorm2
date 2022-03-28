using Moonstorm.Starstorm2;

namespace EntityStates.Executioner
{
    public class Armor : BaseSkillState
    {
        public override void OnEnter()
        {
            if (skillLocator.secondary.stock <= 0)
            { skillLocator.secondary.AddOneStock(); }
            characterBody.AddTimedBuff(SS2Content.Buffs.BuffExecutionerArmor, 4);
        }
    }
}