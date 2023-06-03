using EntityStates;
using EntityStates.AI.Walker;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Moonstorm.Components;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class MULENet : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdMULENet", SS2Bundle.Indev);
        public override void Initialize()
        {
            Hook();
        }

        internal void Hook()
        {
            IL.EntityStates.AI.Walker.Combat.GenerateBodyInputs += (il) =>
            {
                ILCursor curs = new ILCursor(il);
                curs.GotoNext(x => x.MatchLdfld<Combat>("currentSkillMeetsActivationConditions"));
                curs.Index += 1;
                curs.Emit(OpCodes.Ldarg_0);
                curs.Emit(OpCodes.Ldfld, typeof(EntityState).GetFieldCached("outer"));
                curs.EmitDelegate<Func<bool, EntityStateMachine, bool>>((cond, ESM) =>
                {
                    if (ESM.GetComponent<CharacterMaster>().GetBody())
                        if (ESM.GetComponent<CharacterMaster>().GetBody().HasBuff(SS2Content.Buffs.bdMULENet))
                            return false;
                    return cond;
                });
            };
        }
        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {

            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdMULENet;
            private float mass = 0f;
            private float pushForce;
            
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                int buffCount = body.GetBuffCount(SS2Content.Buffs.bdMULENet);
                if (buffCount > 0)
                {
                    body.rigidbody.velocity = Vector3.zero;
                }

                if (body.characterMotor)
                {
                    mass = body.characterMotor.mass;
                }
                else if (body.rigidbody)
                {
                    mass = body.rigidbody.mass;
                }

                if (mass < 50f) mass = 50f;
                pushForce = 50f * mass;
            }

            private void FixedUpdate()
            {
                int buffCount = body.GetBuffCount(SS2Content.Buffs.bdMULENet);
                if (buffCount > 0)
                {
                    Knockdown();

                    body.isSprinting = false;

                    if (body.characterMotor)
                    {
                        body.characterMotor.moveDirection = Vector3.zero;
                    }

                    if (body.characterDirection)
                    {
                        body.characterDirection.moveVector = Vector3.zero;
                    }

                    if (body.rigidbody)
                    {
                        body.rigidbody.velocity = new Vector3(0f, body.rigidbody.velocity.y, 0f);
                    }
                }
            }

            public void Knockdown()
            {
                if (NetworkServer.active)
                {
                    DamageInfo info = new DamageInfo
                    {
                        attacker = null,
                        inflictor = null,
                        damage = 0,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.Generic,
                        crit = false,
                        dotIndex = DotController.DotIndex.None,
                        force = Vector3.down * pushForce * Time.fixedDeltaTime,
                        position = transform.position,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = 0
                    };

                    if (body.characterMotor)
                    {
                        body.healthComponent.TakeDamageForce(info);
                    }
                    else if (body.rigidbody)
                    {
                        body.healthComponent.TakeDamageForce(info);
                    }
                }
            }
        }
    }
}
