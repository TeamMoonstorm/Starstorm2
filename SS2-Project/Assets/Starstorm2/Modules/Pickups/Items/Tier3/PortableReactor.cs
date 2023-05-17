using RoR2;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    public sealed class PortableReactor : ItemBase
    {

        private const string token = "SS2_ITEM_PORTABLEREACTOR_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("PortableReactor", SS2Bundle.Items);

        [ConfigurableField(SS2Config.IDItem, ConfigDesc = "Duration of invulnerability from Portable Reactor. (1 = 1 second)")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float invulnTime = 80f;
        [ConfigurableField(SS2Config.IDItem, ConfigDesc = "Stacking duration of invulnerability. (1 = 1 second)")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float stackingInvuln = 40f;

        //★ ty nebby
        //To-Do: This thing spits out a few errors every time a stage is started. Doesn't really NEED fixed, but probably should be.

        public override void Initialize()
        {
            CharacterBody.onBodyStartGlobal += ImFuckingInvincible;
        }

        private void ImFuckingInvincible(CharacterBody obj)
        {
            if (!obj.inventory)
                return;
            var inv = obj.inventory;
            int count = inv.GetItemCount(ItemDef);
            if (count > 0)
            {
                if (obj.isPlayerControlled)
                {
                    obj.AddTimedBuff(SS2Content.Buffs.BuffReactor, invulnTime + ((count - 1) * stackingInvuln));
                }
                else
                {
                    obj.AddTimedBuff(SS2Content.Buffs.BuffReactor, invulnTime / 4 + ((count - 1) * stackingInvuln));
                }
            }
        }
    }
}