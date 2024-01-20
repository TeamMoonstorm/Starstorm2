using R2API;
using RoR2;
using RoR2.Items;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using System;
using Moonstorm.Starstorm2.Components;
namespace Moonstorm.Starstorm2.Items
{
    public sealed class Fork : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Fork", SS2Bundle.Items);

        //[RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Bonus base damage per fork. (1 = 1 base damage. Base damage for most characters is 12.)")]
        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Bonus percent damage per fork. (1 = 1%)")]
        [TokenModifier("SS2_ITEM_FORK_DESC", StatTypes.Default, 0)]
        public static float percentDamageBonus = 8f;
        public override void Initialize()
        {
            base.Initialize();
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
            bool b = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(0),
                x => x.MatchLdfld<DamageDealtMessage>("attacker"),
                x => x.MatchCallvirt<GameObject>(nameof(GameObject.GetComponent)),
                x => x.MatchStloc(2));
            if(b)
            {
                c.Emit(OpCodes.Ldloc_2); //teamComponent
                c.Emit(OpCodes.Ldloc_0); //damageDealtMessage
                c.EmitDelegate<Action<TeamComponent, DamageDealtMessage>>((t, d) =>
                {
                    float damageFromForks = 0;
                    // dots add a lot of clutter
                    if (!d.damageType.HasFlag(DamageType.DoT) && t && t.body && t.body.inventory)
                    {
                        float bodyDamage = t.body.damage;
                        int stack = t.body.inventory.GetItemCount(SS2Content.Items.Fork);

                        
                        if (stack > 0)
                        {
                            float damageWithoutForks = d.damage / (1 + percentDamageBonus * stack * .01f);
                            damageFromForks = d.damage - damageWithoutForks;
                            DamageNumberManager.instance.SpawnDamageNumber(damageFromForks, d.position + Vector3.up * 0.6f, d.crit, t.teamIndex, DamageColorIndex.Item);
                        }
                    }
                    d.damage -= damageFromForks;
                });
            }
            else
            {
                SS2Log.Warning("Fork.AddForkDamageNumber: ILHook failed.");
            }
        }
    }
}