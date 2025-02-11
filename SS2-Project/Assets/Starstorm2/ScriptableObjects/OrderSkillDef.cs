using RoR2.Skills;
using UnityEngine;
using JetBrains.Annotations;
using RoR2;
using SS2.Components;

namespace SS2
{
    [CreateAssetMenu(menuName = "Starstorm2/SkillDef/OrderSkillDef")]
    public class OrderSkillDef : SkillDef
    {
        public static void Init()
        {
            On.RoR2.GenericSkill.RecalculateMaxStock += (orig, self) =>
            {
                Debug.Log("test");
                if (self.skillDef != null && self.skillDef is OrderSkillDef orderSkillDef)
                {
                    self.maxStock = (int)orderSkillDef.stressValue;
                }
                else
                    orig(self);
            };
        }

        [Tooltip("The amount of stress added by this skill.")]
        public float stressValue = 0;
        [Tooltip("If the skill can be casted while overstressed.")]
        public bool canCastIfOverstressed = false;
        [Tooltip("If the skill can be casted when there is insufficient stress.")]
        public bool canCastIfWillOverstress = false;
        /// <summary>
        /// If the skill will automatically add stress (immediately on activation) or if it will be handled manually. Defaults to true.
        /// If False, YOU MUST CALL :
        /// <code>
        /// NemCaptainController.AddOrderStress(float value)
        /// </code>
        /// or rather :
        /// <code>
        /// 'ncc.AddOrderStress(amount)'
        /// </code>
        /// in the entity state!
        /// </summary>
        [Tooltip("If the skill will automatically add stress (immediately on activation) or if it will be handled manually. Defaults to true. If False, YOU MUST CALL NemCaptainController.AddOrderStress(value) in the entity state!")]
        public bool autoHandleAddStress = true;
        /// <summary>
        /// If the skill will automatically cycle to the next Order (immediately on activation) or if it will be handled manually. Defaults to true.
        /// If False, YOU MUST CALL :
        /// <code>
        /// NemCaptainController.CycleNextOrder(GenericSkill skill)
        /// </code>
        /// or rather :
        /// <code>
        /// 'ncc.CycleNextOrder(activatorSkillSlot)'
        /// </code>
        /// in the entity state!
        /// </summary>
        [Tooltip("If the skill will automatically cycle to the next Order (immediately on activation) or if it will be handled manually. Defaults to true. If False, YOU MUST CALL NemCaptainController.CycleNextOrder(activatorSkillSlot) in the entity state!")]
        public bool autoHandleOrderQueue = true;
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                ncc = skillSlot.GetComponent<NemCaptainController>()
            };
        }

        private static bool IsOverstressed([NotNull] GenericSkill skillSlot)
        {
            NemCaptainController ncc = ((InstanceData)skillSlot.skillInstanceData).ncc;
            return ncc.isOverstressed;
        }

        private static bool IsTotalReset([NotNull] GenericSkill skillSlot)
        {
            NemCaptainController ncc = ((InstanceData)skillSlot.skillInstanceData).ncc;
            return ncc.isTotalReset;
        }

        private bool HasEnoughStress([NotNull] GenericSkill skillSlot)
        {
            if (canCastIfWillOverstress)
                return true;
            NemCaptainController ncc = ((InstanceData)skillSlot.skillInstanceData).ncc;
            float _stressValue = stressValue;
            if (ncc.hasManaReductionBuff)
                _stressValue /= 2;
            return (ncc.totalMaxStress - ncc.stress) > _stressValue;
        }

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            NemCaptainController ncc = ((InstanceData)skillSlot.skillInstanceData).ncc;
            return base.CanExecute(skillSlot) && (!IsOverstressed(skillSlot) || canCastIfOverstressed) && (!IsTotalReset(skillSlot) || ncc.hasFreeOrders) && HasEnoughStress(skillSlot);
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            NemCaptainController ncc = ((InstanceData)skillSlot.skillInstanceData).ncc;
            return base.IsReady(skillSlot) && (!IsOverstressed(skillSlot) || canCastIfOverstressed) && (!IsTotalReset(skillSlot) || ncc.hasFreeOrders) && HasEnoughStress(skillSlot);
        }

        public override void OnExecute([NotNull] GenericSkill skillSlot)
        {
            base.OnExecute(skillSlot);
            NemCaptainController ncc = ((InstanceData)skillSlot.skillInstanceData).ncc;
            if (autoHandleAddStress)
                ncc.AddOrderStress(stressValue);
        }

        protected class InstanceData : SkillDef.BaseSkillInstanceData
        {
            public NemCaptainController ncc;
        }
    }
}
