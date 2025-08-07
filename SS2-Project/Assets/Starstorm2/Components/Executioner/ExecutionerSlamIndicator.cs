using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace SS2.Components
{
	public class ExecutionerSlamIndicator : MonoBehaviour
	{
		public void OnTriggerEnter(Collider other)
		{
			this.AddIndictator(other);
		}

		public void OnTriggerExit(Collider other)
		{
			this.DelIndictator(other);
		}

		public void AddIndictator(Collider target)
		{
			if (this.pairs.ContainsKey(target))
			{
				return;
			}
			ModelLocator component = target.GetComponent<ModelLocator>();
			CharacterModel characterModel;
			if (component == null)
			{
				characterModel = null;
			}
			else
			{
				Transform modelTransform = component.modelTransform;
				characterModel = ((modelTransform != null) ? modelTransform.GetComponent<CharacterModel>() : null);
			}
			CharacterModel characterModel2 = characterModel;
			if (characterModel2 == null)
			{
				return;
			}
			if (!FriendlyFireManager.ShouldSplashHitProceed(characterModel2.body.healthComponent, teamFilter.teamIndex))
			{
				return;
			}
			foreach (CharacterModel.RendererInfo rendererInfo in characterModel2.baseRendererInfos)
			{
				if (!rendererInfo.ignoreOverlays)
				{
					Highlight highlight = base.gameObject.AddComponent<Highlight>();
					highlight.CustomColor = this.customColor;
					highlight.highlightColor = Highlight.HighlightColor.custom;
					highlight.targetRenderer = rendererInfo.renderer;
					highlight.strength = 1f;
					highlight.isOn = true;
					this.pairs.Add(target, highlight);
					return;
				}
			}
		}
		public void DelIndictator(Collider target)
		{
			if (this.pairs.ContainsKey(target))
			{
				UnityEngine.Object.Destroy(this.pairs[target]);
				this.pairs.Remove(target);
			}
		}

		public Color customColor = Color.magenta;
		public TeamFilter teamFilter;

		[NonSerialized]
		public Dictionary<Collider, Highlight> pairs = new Dictionary<Collider, Highlight>();
	}
}
