using RoR2;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    public sealed class PortableReactor : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("PortableReactor");

        [ConfigurableField(ConfigDesc = "Duration of invulnerability from Portable Reactor.")]
        public static float invulnTime = 80f;

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
                if (obj.teamComponent.teamIndex != TeamIndex.Monster) obj.AddTimedBuff(SS2Content.Buffs.BuffReactor, invulnTime + ((count - 1) * invulnTime / 2));
                else obj.AddTimedBuff(SS2Content.Buffs.BuffReactor, invulnTime / 4 + ((count - 1) * invulnTime / 2));
            }
        }
    }
}
