using RoR2;
using RoR2.Achievements;

namespace Moonstorm.Starstorm2.Unlocks.Executioner
{
    public sealed class ExecutionerWastelanderUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.executioner.wastelander", SS2Bundle.Executioner);

        public override void Initialize()
        {
            AddRequiredType<Survivors.Executioner>();
        }

        public sealed class ExecutionerWastelanderAchievement : BaseAchievement
        {
            public override BodyIndex LookUpRequiredBodyIndex()
            {
                return BodyCatalog.FindBodyIndex("ExecutionerBody");
            }

            public override void OnInstall()
            {
                On.EntityStates.Interactables.StoneGate.Opening.OnEnter += Check;
            }

            private void Check(On.EntityStates.Interactables.StoneGate.Opening.orig_OnEnter orig, EntityStates.Interactables.StoneGate.Opening self)
            {
                orig(self);

                if (meetsBodyRequirement)
                {
                    Grant();
                }
            }

            public override void OnUninstall()
            {
                On.EntityStates.Interactables.StoneGate.Opening.OnEnter -= Check;
            }
        }
    }
}