using EntityStates;
using Moonstorm.Components;
using Moonstorm.Starstorm2.Items;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class KnightParry : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdParry", SS2Bundle.Indev);
        public sealed class Behavior : BaseBuffBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdParry;

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo.attacker != body)
                {
                    if (body.baseNameToken != "SS2_KNIGHT_BODY_NAME")
                        return;

                    damageInfo.rejected = true;

                    SetStateOnHurt ssoh = damageInfo.attacker.GetComponent<SetStateOnHurt>();
                    if (ssoh)
                    {
                        Type state = ssoh.targetStateMachine.state.GetType();
                        if (state != typeof(StunState) && state != typeof(ShockState) && state != typeof(FrozenState))
                        {
                            ssoh.SetStun(3f);
                        }
                    }

                    body.AddBuff(SS2Content.Buffs.bdFortified.buffIndex);
                    body.AddBuff(SS2Content.Buffs.bdFortified.buffIndex);
                    if (body.GetBuffCount(SS2Content.Buffs.bdFortified.buffIndex) > 3)
                        body.SetBuffCount(SS2Content.Buffs.bdFortified.buffIndex, 3);

                    Util.PlaySound("NemmandoDecisiveStrikeReady", gameObject);

                    NetworkStateMachine nsm = body.GetComponent<NetworkStateMachine>();
                    EntityStateMachine weaponEsm = nsm.stateMachines[1];
                    weaponEsm.SetNextState(new EntityStates.Knight.Parry());

                    Destroy(this);
                }
            }

            //to-do: implement behaviour lol
        }
    }
}
