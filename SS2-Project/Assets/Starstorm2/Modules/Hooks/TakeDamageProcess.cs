using System;
using System.Collections.Generic;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2;

namespace SS2.Hooks
{
    public class TakeDamageProcess
    {
        [SystemInitializer]
        public static void Initialize()
        {
            IL.RoR2.HealthComponent.TakeDamageProcess += DeathIL;
        }

        private static void DeathIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            bool ILFound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(12),
                x => x.MatchCallOrCallvirt<GlobalEventManager>(nameof(GlobalEventManager.ServerDamageDealt)),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<HealthComponent>("get_alive")
            );
            c.Index += 3;
            if (ILFound)
            {
                c.Emit(OpCodes.Ldarg_0); // hc
                c.Emit(OpCodes.Ldarg_1); // damageinfo
                c.Emit(OpCodes.Ldloc, 4); // total damage
                c.EmitDelegate<Action<HealthComponent, DamageInfo, float>>(OnLethalDamageTaken);
            }
            else
            {
                SS2Log.Fatal("AffixEthereal.EtherealDeathIL(): Failed to find IL match.");
            }

        }

        public static void OnLethalDamageTaken(HealthComponent healthComponent, DamageInfo damageInfo, float totalDamage) // poorly named
        {
            if (healthComponent.health > 0) return; ///
            CharacterBody body = healthComponent.body;
            //bleedout
            if (body.HasBuff(SS2Content.Buffs.BuffBleedoutReady))
            {
                body.RemoveBuff(SS2Content.Buffs.BuffBleedoutReady);
                healthComponent.Networkhealth = 1f;
                healthComponent.HealFraction(1, default(ProcChainMask));
                InflictDotInfo dot = new InflictDotInfo
                {
                    attackerObject = null,
                    dotIndex = Items.Blessings.bleedout,
                    duration = 2f,
                    victimObject = healthComponent.gameObject,
                    totalDamage = healthComponent.fullHealth,
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
                healthComponent.Networkhealth += totalDamage;
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
