using System;
using UnityEngine;

namespace RoR2
{
	public class ApplyRandomForceOnStart : MonoBehaviour
	{
		private void Start()
		{
			Rigidbody component = base.GetComponent<Rigidbody>();
			if (component)
			{
				float i = UnityEngine.Random.Range(0f, 1f);
				Vector3 force = Vector3.Lerp(minLocalForce, maxLocalForce, i);
				component.AddRelativeForce(force);
			}
		}

		public Vector3 minLocalForce;
		public Vector3 maxLocalForce;
	}
}
