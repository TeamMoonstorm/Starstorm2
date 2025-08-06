using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class KnightBlockTracker : NetworkBehaviour, IOnIncomingDamageServerReceiver
    {
        private GameObject lastAttacker;
        private float lastAttackTime;

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

        public void OnIncomingDamageServer(DamageInfo damageInfo)
        {
            RpcSetLastAttacker(damageInfo.attacker);
        }

        [ClientRpc]
        public void RpcSetLastAttacker(GameObject attacker)
        {
            lastAttacker = attacker;
            lastAttackTime = 0.2f;
            onIncomingDamageAuthority?.Invoke(attacker);
        }
    }
}
