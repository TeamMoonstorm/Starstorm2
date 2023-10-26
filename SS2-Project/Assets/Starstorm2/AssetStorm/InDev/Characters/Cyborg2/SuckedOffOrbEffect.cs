using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using UnityEngine.UI;

namespace Moonstorm.Starstorm2.Components
{
	[RequireComponent(typeof(EffectComponent))]
	public class SuckedOffOrbEffect : MonoBehaviour
	{
		private void Start()
		{
			GameObject bodyObject = BodyCatalog.GetBodyPrefab((BodyIndex)(Util.UintToIntMinusOne(base.GetComponent<EffectComponent>().effectData.genericUInt)));
			CharacterBody body = bodyObject.GetComponent<CharacterBody>();
			
			Color color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Money);
			this.trailToColor.startColor = this.trailToColor.startColor * color;
			this.trailToColor.endColor = this.trailToColor.endColor * color;
			for (int i = 0; i < this.particlesToColor.Length; i++)
			{
				ParticleSystem particleSystem = this.particlesToColor[i];
				particleSystem.startColor = color;
				particleSystem.Play();
			}
			for (int j = 0; j < this.spritesToColor.Length; j++)
			{
				this.spritesToColor[j].color = color;
			}
			this.image.texture = body.portraitIcon;
		}

		public TrailRenderer trailToColor;

		public ParticleSystem[] particlesToColor;

		public SpriteRenderer[] spritesToColor;

		public RawImage image;

	}
}
