using UnityEngine;
using RoR2;

namespace SS2.Components
{
    public class AcidBugForceCollector : MonoBehaviour, IOnTakeDamageServerReceiver
    {
        public Vector3 force { get; set; }
        public float decayTime = 0.067f; // we want to collect all recent hits instead of only the very last
        private Vector3 forceDecayVelocity;
        private void FixedUpdate()
        {
            if (force != Vector3.zero)
            {
                force = Vector3.SmoothDamp(force, Vector3.zero, ref forceDecayVelocity, decayTime);
            }
        }
        public void OnTakeDamageServer(DamageReport damageReport)
        {
            force += damageReport.damageInfo.force;
        }
    }
}
