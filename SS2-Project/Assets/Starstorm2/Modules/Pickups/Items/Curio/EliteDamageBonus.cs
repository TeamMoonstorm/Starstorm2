using RoR2;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
namespace SS2.Items
{
    public sealed class EliteDamageBonus : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("EliteDamageBonus", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => true;
        public override void Initialize()
        {
            IL.RoR2.HealthComponent.TakeDamageProcess += TakeDamageProcess; // should move this to one hook
        }

        // bonus damage to elites
        private void TakeDamageProcess(ILContext il)
        {
            //grabbed this from moffein. no clue how to get that dumbass displayclass thing and idc to find out
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(
                 x => x.MatchStloc(7)
                ))
            {
                c.Emit(OpCodes.Ldarg_0);    //self
                c.Emit(OpCodes.Ldarg_1);    //damageInfo
                c.EmitDelegate<Func<float, HealthComponent, DamageInfo, float>>((damage, victim, damageInfo) =>
                {
                    if (victim.body.isElite && damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody body) && body.inventory)
                    {
                        int elite = body.inventory.GetItemCount(SS2Content.Items.EliteDamageBonus);
                        if (elite > 0)
                        {
                            damage *= 1.3f + 0.1f * (elite - 1);
                            damageInfo.damageColorIndex = DamageColorIndex.Luminous;
                            //
                            // GET CUSTOM COLOR LATER ?
                            //
                        }
                    }
                    return damage;
                });
            }
            else
            {
                SS2Log.Fatal("RiskyMod: ModifyFinalDamage IL Hook failed. This will break a lot of things.");
            }
        }
    }
}
