using R2API;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using System;
using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace SS2.Items
{
    public sealed class Fork : SS2Item
    {
        private const string token = "SS2_ITEM_FORK_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acFork", SS2Bundle.Items);
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Bonus percent damage per fork. (1 = 1%)")]
        [FormatToken(token, 0)]
        public static float percentDamageBonus = 8f;
        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += ForkDamage;
            // i tried to make it so damage number velocities were "synced" with 2 particlesystems using the same seed
            // it didnt immediately work so i gave up
            // would be nice to have if someone wants to look into it
            IL.RoR2.HealthComponent.HandleDamageDealt += AddForkDamageNumber;
        }

        private void ForkDamage(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int stack = sender.inventory ? sender.inventory.GetItemCount(ItemDef) : 0;
            args.damageMultAdd += (percentDamageBonus / 100f) * stack;
        }

        private void AddForkDamageNumber(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            // teamComponent = damageDealtMessage.attacker.GetComponent<TeamComponent>();
            int damageDealtMessageVarIndex = -1;
            int teamComponentVarIndex = -1;
            bool b = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(out damageDealtMessageVarIndex),
                x => x.MatchLdfld<DamageDealtMessage>(nameof(DamageDealtMessage.attacker)),
                x => x.MatchCallvirt<GameObject>(nameof(GameObject.GetComponent)),
                x => x.MatchStloc(out teamComponentVarIndex));
            if (b)
            {
                c.Emit(OpCodes.Ldloc, teamComponentVarIndex);
                c.Emit(OpCodes.Ldloc, damageDealtMessageVarIndex);
                c.EmitDelegate<Action<TeamComponent, DamageDealtMessage>>((t, d) =>
                {
                    float damageFromForks = 0;
                    float damageFromChocolate = 0;
                    float damageFromSigil = 0;
                    // dots add a lot of clutter
                    if (!d.damageType.damageType.HasFlag(DamageType.DoT) && t && t.body && t.body.inventory)
                    {
                        float bodyDamage = t.body.damage;
                        int stack = t.body.inventory.GetItemCount(SS2Content.Items.Fork);
                        bool hasChocolate = t.body.HasBuff(SS2Content.Buffs.BuffChocolate);
                        int sigil = t.body.GetBuffCount(SS2Content.Buffs.BuffSigilStack);

                        if (stack > 0)
                        {
                            float damageWithoutForks = d.damage / (1 + percentDamageBonus * stack * .01f);
                            damageFromForks = d.damage - damageWithoutForks;
                            DamageNumberManager.instance.SpawnDamageNumber(damageFromForks, d.position + Vector3.up * 0.6f, d.crit, t.teamIndex, DamageColorIndex.Item);
                        }
                        if (hasChocolate) // hello fork
                        {
                            float damageWithoutChocolate = d.damage / (1 + GreenChocolate.buffDamage);
                            damageFromChocolate = d.damage - damageWithoutChocolate;
                            DamageNumberManager.instance.SpawnDamageNumber(damageFromChocolate, d.position + Vector3.up * 0.85f, d.crit, t.teamIndex, GreenChocolate.damageColor);
                        }
                        if (sigil > 0) // hello fork and chocolate
                        {
                            float damageWithoutSigil = d.damage / (1 + (HuntersSigil.baseDamage + HuntersSigil.stackDamage * (sigil - 1)));
                            damageFromSigil = d.damage - damageWithoutSigil;
                            DamageNumberManager.instance.SpawnDamageNumber(damageFromSigil, d.position + Vector3.up * 0.75f, d.crit, t.teamIndex, DamageColorIndex.Bleed);
                        }
                    }
                    d.damage -= damageFromForks + damageFromChocolate + damageFromSigil;
                });
            }
            else
            {
                SS2Log.Fatal("Fork.AddForkDamageNumber: ILHook failed.");
            }
        }
    }
}