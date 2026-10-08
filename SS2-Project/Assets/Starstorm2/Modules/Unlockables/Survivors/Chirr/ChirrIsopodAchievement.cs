using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Achievements;
using UnityEngine.SceneManagement;
using UnityEngine;
using MSU;
namespace SS2.Unlocks.Chirr
{
    public sealed class ChirrIsopodAchievement : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            base.SetServerTracked(true);
        }
        
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("ChirrBody");
        }
        
        private class ChirrIsopodServerAchievement : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                IL.EntityStates.VoidInfestor.Infest.FixedUpdate += InfestOnFixedUpdate;
            }

            private void InfestOnFixedUpdate(ILContext il)
            {
                ILCursor cursor = new ILCursor(il);

                /* // CharacterBody body = obj.body;
                   IL_00b1: dup
                   IL_00b2: ldfld class RoR2.CharacterBody RoR2.HealthComponent::body
                   IL_00b7: stloc.3
                 */
                int bodyStack = -1;
                bool bodyIL = cursor.TryGotoNext(MoveType.After,
                    instruction => instruction.MatchDup(),
                    instruction => instruction.MatchLdfld<HealthComponent>(nameof(HealthComponent.body)),
                    instruction => instruction.MatchStloc(out bodyStack));

                if (!bodyIL)
                {
                    SS2Log.Error("failed body il isopod");
                    return;
                }
                
                /* // body.teamComponent.teamIndex = TeamIndex.Void;
                   IL_0105: ldloc.3
                   IL_0106: callvirt instance class RoR2.TeamComponent RoR2.CharacterBody::get_teamComponent()
                   IL_010b: ldc.i4.4
                   IL_010c: callvirt instance void RoR2.TeamComponent::set_teamIndex(valuetype RoR2.TeamIndex)
                 */
                bool addVoidEquipIL = cursor.TryGotoNext(MoveType.After,
                    instruction => instruction.MatchLdloc(bodyStack),
                    instruction => instruction.MatchCallvirt<CharacterBody>("get_teamComponent"),
                    instruction => instruction.MatchLdcI4(out _),
                    instruction => instruction.MatchCallvirt<TeamComponent>("set_teamIndex"));
                
                if (!addVoidEquipIL)
                {
                    SS2Log.Error("failed addVoidEquipIL il isopod");
                    return;
                }
                
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<EntityStates.VoidInfestor.Infest>>((state) =>
                {
                    CharacterBody ownerBody = state.characterBody.AsValidOrNull()?.master.AsValidOrNull()?.minionOwnership.AsValidOrNull()?.ownerMaster.AsValidOrNull()?.GetBody();
                    if (ownerBody?.bodyIndex == BodyCatalog.FindBodyIndex("ChirrBody"))
                    {
                        // this grants all chirrs connected in multiplayer the achievement at once but i dont think theres a way to grant to a single body/pcmc here ?,.,
                        Grant();
                    }
                });
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
                IL.EntityStates.VoidInfestor.Infest.FixedUpdate -= InfestOnFixedUpdate;
            }
        }
    }   
}