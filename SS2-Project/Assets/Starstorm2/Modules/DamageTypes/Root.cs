using EntityStates;
using EntityStates.Chirr;
using R2API;
using RoR2;
using static R2API.DamageAPI;

namespace Moonstorm.Starstorm2.DamageTypes
{
    [DisabledContent]
    public sealed class Root : DamageTypeBase
    {
        public override ModdedDamageType ModdedDamageType { get; protected set; }
        public static DotController.DotIndex DotIndex;

        public static ModdedDamageType rootDamageType;
        public static float rootDuration = 4f;
        private float tempDuration;

        public override void Initialize()
        {
            ModdedDamageType = rootDamageType;
            Delegates();
        }

        public void SetNextRootDuration(float duration)
        {
            tempDuration = duration;
        }

        public override void Delegates()
        {
            GlobalEventManager.onServerDamageDealt += ApplyRoot;
        }

        private void ApplyRoot(DamageReport report)
        {
            var victimBody = report.victimBody;
            var damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, ModdedDamageType))
            {
                if (tempDuration == 0f) tempDuration = rootDuration;
                SetRoot(victimBody.GetComponent<SetStateOnHurt>().targetStateMachine, tempDuration);
                tempDuration = 0f;
            }
        }

        private void SetRoot(EntityStateMachine entity, float duration)
        {
            RootState rootState = new RootState();
            rootState.rootDuration = duration;
            entity.SetInterruptState(rootState, InterruptPriority.Pain);
        }
    }
}
