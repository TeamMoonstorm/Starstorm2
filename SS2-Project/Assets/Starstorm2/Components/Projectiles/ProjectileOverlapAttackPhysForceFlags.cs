using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace SS2
{
    [RequireComponent(typeof(ProjectileOverlapAttack))]
    public class ProjectileOverlapAttackPhysForceFlags : MonoBehaviour
    {
        //I miss writing stupid code

        public PhysForceFlags physForceFlags;

        private ProjectileOverlapAttack overlapAttack;
        private bool applied = false;

        private void Start()
        {
            overlapAttack = GetComponent<ProjectileOverlapAttack>();
        }

        private void FixedUpdate()
        {
            if (!applied && overlapAttack.attack != null)
            {
                applied = true;
                overlapAttack.attack.physForceFlags = physForceFlags;
            }
        }
    }
}
