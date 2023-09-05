using System;
using System.Collections.Generic;
using HG;
using UnityEngine;
using RoR2;
using JetBrains.Annotations;
using RoR2.Projectile;
using System.Text;
using R2API;
namespace Moonstorm.Starstorm2.Components
{
	[RequireComponent(typeof(EffectComponent))]
	public class SetMaterialColorFromEffectData : MonoBehaviour
	{
		public ParticleSystem[] particleSystems;

		public EffectComponent effectComponent;
		private void Start()
		{
			Color color = this.effectComponent.effectData.color;
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				this.particleSystems[i].GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", color);
				this.particleSystems[i].Clear();
				this.particleSystems[i].Play();

			}
		}

	}		
}