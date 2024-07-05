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
				this.targetTextMesh.text = displayValue.ToString();
			}
		}

		public float displayValue;
		public TextMeshPro targetTextMesh;
	}
}
