using RoR2;
using System;
using EntityStates;
using UnityEngine;
using UnityEngine.Networking;
using SS2.Survivors;

namespace SS2.Components
{
    public class KnightBlockTracker : NetworkBehaviour, IOnIncomingDamageServerReceiver
    {
        private GameObject lastAttacker;
        private float lastAttackTime;

        private float parryWindowEndTime;
        private bool hasStunned;

        public delegate void OnIncomingDamageAuthority(GameObject attacker);
        public OnIncomingDamageAuthority onIncomingDamageAuthority;

        void Update()
        {
            lastAttackTime -= Time.deltaTime;
            if(lastAttackTime < 0)
            {
                lastAttacker = null;
            }
        }

        public GameObject GetLastAttacker() => lastAttacker;

        [Command]
        public void CmdArmParry(float duration)
        {
            parryWindowEndTime = Time.fixedTime + duration;
            hasStunned = false;
        }

        [Command]
        public void CmdDisarmParry()
        {
            parryWindowEndTime = 0f;
        }

        public void OnIncomingDamageServer(DamageInfo damageInfo)
        {
            if (Time.fixedTime < parryWindowEndTime)
            {
                damageInfo.rejected = true;

                if (!hasStunned)
                {
                    hasStunned = true;

                    if (damageInfo.attacker
                        && damageInfo.attacker.TryGetComponent(out SetStateOnHurt setStateOnHurt)
                        && setStateOnHurt.canBeStunned)
                    {
                        Type state = setStateOnHurt.targetStateMachine.state.GetType();
                        if (state != typeof(StunState)
                            && state != typeof(ShockState)
                            && state != typeof(FrozenState))
                        {
                            setStateOnHurt.SetStun(3f);
                        }
                    }
                }
            }
            RpcSetLastAttacker(damageInfo.attacker);
        }

        [ClientRpc]
        public void RpcSetLastAttacker(GameObject attacker)
        {
            lastAttacker = attacker;
            lastAttackTime = Knight.lastAttackWindow;
            onIncomingDamageAuthority?.Invoke(attacker);
        }
    }
}
