using System;
using UnityEngine;
using RoR2;
using RoR2.Orbs;
namespace Moonstorm.Starstorm2.Components
{
	public class FlowerOrb : GenericDamageOrb
	{
		public override void Begin()
		{
			this.speed = 120f;
			base.Begin();
		}

		public override GameObject GetOrbEffect()
		{
			return SS2Assets.LoadAsset<GameObject>("FlowerTurretOrbEffect", SS2Bundle.Items);
		}

		public override void OnArrival()
		{
			if (this.target)
			{
				HealthComponent healthComponent = this.target.healthComponent;
				if (healthComponent)
				{
					Vector3 vector = this.target.transform.position - this.origin;
					if (vector.sqrMagnitude > 0f)
					{
						vector.Normalize();
						vector *= this.forceScalar;
					}
					DamageInfo damageInfo = new DamageInfo();
					damageInfo.damage = this.damageValue;
					damageInfo.attacker = this.attacker;
					damageInfo.inflictor = null;
					damageInfo.force = Vector3.zero;
					damageInfo.crit = this.isCrit;
					damageInfo.procChainMask = this.procChainMask;
					damageInfo.procCoefficient = this.procCoefficient;
					damageInfo.position = this.target.transform.position;
					damageInfo.damageColorIndex = this.damageColorIndex;
					damageInfo.damageType = this.damageType;
					damageInfo.force = vector;
					healthComponent.TakeDamage(damageInfo);
					GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
					GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
				}
			}
		}

		public float forceScalar;
	}
}
