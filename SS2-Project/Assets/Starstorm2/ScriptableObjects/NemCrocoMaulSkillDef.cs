using UnityEngine;
using RoR2.Skills;
using JetBrains.Annotations;
using RoR2;
using EntityStates;
using SS2.Components;
namespace SS2
{
    [CreateAssetMenu(menuName = "Starstorm2/SkillDef/NemCrocoMaulSkillDef")]
    public class NemCrocoMaulSkillDef : SteppedSkillDef
    {
        // Applies overrides if a target is available
        public Sprite targetOverrideIcon;
        public SerializableEntityStateType targetOverrideState;
        public InterruptPriority targetOverridePriority;
        public string targetOverrideStateMachine = "";
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new MaulInstanceData
            {
                tracker = skillSlot.GetComponent<NemCrocoTracker>()
            };
        }

        public override void OnExecute([NotNull] GenericSkill skillSlot)
        {
            EntityStateMachine defaultStateMachine = skillSlot.stateMachine;
            EntityStateMachine overrideStateMachine = EntityStateMachine.FindByCustomName(skillSlot.gameObject, targetOverrideStateMachine);
            if (overrideStateMachine && HasTarget(skillSlot))
            {
                skillSlot.stateMachine = overrideStateMachine;
            }

            base.OnExecute(skillSlot);

            skillSlot.stateMachine = defaultStateMachine;
        }

        public override EntityState InstantiateNextState([NotNull] GenericSkill skillSlot)
        {
            if (HasTarget(skillSlot))
            {
                SerializableEntityStateType state = HasTarget(skillSlot) ? targetOverrideState : activationState;
                EntityState entityState = EntityStateCatalog.InstantiateState(state.stateType);
                ISkillState skillState;
                if ((skillState = (entityState as ISkillState)) != null)
                {
                    skillState.activatorSkillSlot = skillSlot;
                }
                return entityState;
            }
            
            return base.InstantiateNextState(skillSlot);
        }
        public override Sprite GetCurrentIcon([NotNull] GenericSkill skillSlot)
        {
            return HasTarget(skillSlot) ? targetOverrideIcon : base.GetCurrentIcon(skillSlot);
        }
        public static bool HasTarget([NotNull] GenericSkill skillSlot)
        {
            NemCrocoTracker tracker = ((MaulInstanceData)skillSlot.skillInstanceData).tracker;
            return (tracker != null) ? tracker.GetTrackingTarget() : null;
        }

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            var interruptPriority = HasTarget(skillSlot) ? targetOverridePriority : base.interruptPriority;
            return IsReady(skillSlot) && skillSlot.stateMachine && !skillSlot.stateMachine.HasPendingState() && skillSlot.stateMachine.CanInterruptState(interruptPriority);
        }
        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return HasTarget(skillSlot) || base.IsReady(skillSlot);
        }

        public class MaulInstanceData : SteppedSkillDef.InstanceData
        {
            public NemCrocoTracker tracker;
        }

    }
}
