using R2API;
using RoR2;
namespace SS2.Buffs
{
    //[DisabledContent]
    public sealed class ChirrGrabFriend : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffChirrGrabFriend", SS2Bundle.Chirr);

        public static float attackSpeedBuff = 1f;

       
        public override void Initialize()
        {
            base.Initialize();
            RecalculateStatsAPI.GetStatCoefficients += ModifyStats;
        }

        private void ModifyStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int count = sender.GetBuffCount(BuffDef);
            args.baseAttackSpeedAdd += attackSpeedBuff * count;
        }
        
    }
}
