using UnityEngine;
using RoR2;
using UnityEngine.UI;
namespace SS2.Orbs
{
    [RequireComponent(typeof(EffectComponent))]
	public class CharacterBodyOrbEffect : MonoBehaviour
	{
		private void Start()
		{
			GameObject bodyObject = BodyCatalog.GetBodyPrefab((BodyIndex)(Util.UintToIntMinusOne(base.GetComponent<EffectComponent>().effectData.genericUInt)));
			CharacterBody body = bodyObject.GetComponent<CharacterBody>();

			//SS2Log.Info("bodyobject from orb: " + bodyObject + " | " + body.portraitIcon.name + " | ");

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
