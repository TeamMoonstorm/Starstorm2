using RoR2;

namespace SS2.Hooks
{
    public class TakeDamageProcess
    {
        [SystemInitializer]
        public static void Initialize()
        {
            GlobalEventManager.onServerDamageDealt += OnAffixEtherealLethalDamageTaken;
        }

        private static void OnAffixEtherealLethalDamageTaken(DamageReport damageReport)
        {
            if (!damageReport.victimBody || !damageReport.victimBody.healthComponent)
                return;

            HealthComponent healthComponent = damageReport.victimBody.healthComponent;
            if (healthComponent.alive)
                return;

            CharacterBody body = damageReport.victimBody;
            DamageInfo damageInfo = damageReport.damageInfo;
            float combinedHealthBeforeDamage = damageReport.combinedHealthBeforeDamage;

            //bleedout
            if (body.HasBuff(SS2Content.Buffs.BuffBleedoutReady))
            {
                body.RemoveBuff(SS2Content.Buffs.BuffBleedoutReady);
                healthComponent.Networkhealth = 1f;
                healthComponent.HealFraction(1, default(ProcChainMask));
                float undoBodyDamage = damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) ? 1 / attackerBody.damage : 1f;
                //undodamage will make it do 1 damage per tick
                //4 ticks per second
                //2 seconds duration
                //total damage is hc.fullhealth
                //fml and fuck dots
                float desiredDamagePerTick = healthComponent.fullHealth / (2 * 4);
                float damagePerTick = undoBodyDamage * desiredDamagePerTick;
                InflictDotInfo dot = new InflictDotInfo
                {
                    attackerObject = damageInfo.attacker ? damageInfo.attacker : healthComponent.gameObject,
                    dotIndex = Items.Bleedout.bleedout,
                    duration = 2f,
                    victimObject = healthComponent.gameObject,
                    damageMultiplier = damagePerTick,
                };
                DotController.InflictDot(ref dot);
                //
                // VFX HERE
                //
                return;
            }
            //shell piece
            if (body.inventory && body.inventory.GetItemCount(SS2Content.Items.ShellPiece) > 0)
            {
                healthComponent.Networkhealth = combinedHealthBeforeDamage;
                if (healthComponent.Networkhealth < 1) healthComponent.Networkhealth = 1;
                body.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 3f);
                body.inventory.RemoveItem(SS2Content.Items.ShellPiece);
                body.inventory.GiveItem(SS2Content.Items.ShellPieceConsumed);
                CharacterMasterNotificationQueue.SendTransformNotification(body.master, SS2Content.Items.ShellPiece.itemIndex, SS2Content.Items.ShellPieceConsumed.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                //
                // VFX HERE
                //
                return;
            }
            // ethereal death 
            if (body.HasBuff(SS2Content.Buffs.bdEthereal))
            {
                healthComponent.health = 1;
                if (!body.HasBuff(SS2Content.Buffs.bdHakai))
                {
                    body.AddBuff(SS2Content.Buffs.bdHakai);
                    body.AddBuff(RoR2Content.Buffs.Intangible);
                }
            }
        }
    }
}
