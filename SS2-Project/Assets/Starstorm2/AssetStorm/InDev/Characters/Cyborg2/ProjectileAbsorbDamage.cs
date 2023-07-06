using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using RoR2.Projectile;

namespace Moonstorm.Starstorm2.Components
{
    public class ProjectileAbsorbDamage : MonoBehaviour, IOnTakeDamageServerReceiver
    {
        private ProjectileDamage damage;
        public float damageAbsorptionMultiplier = 1.5f;

        private void Awake()
        {
            this.damage = base.GetComponent<ProjectileDamage>();
        }
        public void OnTakeDamageServer(DamageReport damageReport)
        {
            if (this.damage)
                this.damage.damage += damageReport.damageDealt * damageAbsorptionMultiplier;

            //VFX
            //SOUND
            //GROW SIZE?

            //attackspeedsound. increase pitch with charge
            Util.PlaySound("Play_treeBot_shift_shoot", base.gameObject);
        }
    }
}
