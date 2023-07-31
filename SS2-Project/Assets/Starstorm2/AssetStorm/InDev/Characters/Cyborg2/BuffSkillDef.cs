using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Skills;
using JetBrains.Annotations;
using RoR2;
using EntityStates;

namespace Moonstorm.Starstorm2.ScriptableObjects
{
	[CreateAssetMenu(menuName = "Starstorm2/SkillDef/BuffSkillDef")]

    // do buff state when body has provided buffdef
	public class BuffSkillDef : SkillDef
	{
        public SerializableEntityStateType buffState;
        public BuffDef targetBuff;
        public override EntityState InstantiateNextState([NotNull] GenericSkill skillSlot)
        {
            bool hasBuff = skillSlot.characterBody.HasBuff(targetBuff);
            SerializableEntityStateType state = hasBuff ? buffState : activationState;
            if(hasBuff)
            {
                skillSlot.characterBody.ClearTimedBuffs(targetBuff);
            }
            EntityState entityState = EntityStateCatalog.InstantiateState(state);
            ISkillState skillState;
            if ((skillState = (entityState as ISkillState)) != null)
            {
                skillState.activatorSkillSlot = skillSlot;
            }
            return entityState;
        }
    }
}
