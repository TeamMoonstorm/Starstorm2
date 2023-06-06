using R2API;
using RoR2;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class Gouge : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffGouge", SS2Bundle.Nemmando);
        public static DotController.DotIndex index;

        public override void Initialize()
        {
            index = DotAPI.RegisterDotDef(0.25f, 0.25f, DamageColorIndex.SuperBleed, BuffDef);
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            bool triggerGougeProc = false;
            if (NetworkServer.active)
            {
                if (damageInfo.dotIndex == index && damageInfo.procCoefficient == 0f && self.alive)
                {
                    if (damageInfo.attacker)
                    {
                        CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                        if (attackerBody)
                        {
                            damageInfo.crit = Util.CheckRoll(attackerBody.crit, attackerBody.master);
                        }
                    }
                    damageInfo.procCoefficient = 0.2f;
                    triggerGougeProc = true;
                }
            }

            orig(self, damageInfo);

            if (NetworkServer.active && !damageInfo.rejected && self.alive)
            {
                if (triggerGougeProc)
                {
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, self.gameObject);
                }
            }
        }
    }
}
