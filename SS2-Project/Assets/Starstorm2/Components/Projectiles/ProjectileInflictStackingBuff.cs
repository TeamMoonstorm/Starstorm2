using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(ProjectileController))]
    public class ProjectileInflictStackingBuff : MonoBehaviour, IOnDamageInflictedServerReceiver
    {
        public BuffDef buffDef;
        public float buffDuration;
        public int buffMaxStacks;

        public void OnDamageInflictedServer(DamageReport damageReport)
        {
            CharacterBody victim = damageReport.victimBody;
            if (victim)
            {
                victim.AddTimedBuff(buffDef, buffDuration, buffMaxStacks);
            }
        }

        private void OnValidate()
        {
            if (!this.buffDef)
            {
                Debug.LogWarningFormat(this, "ProjectileInflictStackingBuff {0} has no buff specified.", new object[]
                {
                    this
                });
            }
        }
    }
}
