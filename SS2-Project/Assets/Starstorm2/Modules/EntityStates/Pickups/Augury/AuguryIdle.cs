using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Pickups.Augury
{
    public class AuguryIdle : AuguryBaseState
    {
        public static float damageThresholdMultiplier = 6;

        private float damageThreshold;
        private float tankedDamage;
        private TeamIndex lastestAttackerTeamIndex;
        private Vector3 effectScale;

        public void OnTakeDamage(DamageReport damageReport)
        {
            if (damageReport.isFallDamage) return;

            tankedDamage += damageReport.damageDealt;
            lastestAttackerTeamIndex = damageReport.attackerTeamIndex;
        }

        public override void Update()
        {
            base.Update();
            effectScale = ((Vector3.one * tankedDamage / attachedBody.maxHealth) / 2) * attachedBody.bestFitRadius;
            auguryEffect.UpdateSize(effectScale);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            damageThreshold = attachedBody.maxHealth * damageThresholdMultiplier;
            if (tankedDamage > damageThreshold && NetworkServer.active)
            {
                tankedDamage = 0;
                var state = new AuguryPrepDetonation();
                state.scale = effectScale;
                state.detonatorTeamIndex = lastestAttackerTeamIndex;
                outer.SetNextState(state);
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(tankedDamage);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            tankedDamage = reader.ReadSingle();
        }
    }
}
