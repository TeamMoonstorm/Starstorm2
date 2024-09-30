using System;
using TMPro;
using UnityEngine;

namespace SS2
{
	public class ValueHologramContent : MonoBehaviour
	{
		private void FixedUpdate()
		{
			if (this.targetTextMesh)
			{
				targetTextMesh.enabled = true; // tmpro throws an nre in the editor and disables itself :(
				this.targetTextMesh.text = displayValue.ToString();
			}
		}

		public float displayValue;
		public TextMeshPro targetTextMesh;
	}
}
