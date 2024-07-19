using System;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.Audio;
namespace SS2
{
	// no way im networking this fuck youuuuuuuuuuuuuu 
	// pay me
    public class BeamChain : MonoBehaviour
    {
		public LineRenderer lineRenderer;
		public GameObject pointEffectPrefab;
		public string pointSpawnSoundString = "Play_engi_seekerMissile_lockOn";
		public float newPointLerp = 0.1f;

		public Transform startTransform;
		public Transform endTransform;

		private List<BeamPoint> beamPoints;
		public struct BeamPoint
        {
			public float age;
			public Vector3 startPos;
			public Vector3 position;
			public Transform target;
			public GameObject effectInstance;			
        }

        private void Awake()
        {
			beamPoints = new List<BeamPoint>();
			beamPoints.Add(new BeamPoint { target = startTransform, age = Mathf.Infinity });
			beamPoints.Add(new BeamPoint { target = endTransform, age = Mathf.Infinity });

			UpdateBeamPositions(0);
		}
        private void Update()
        {
			CleanList();
			UpdateBeamPositions(Time.deltaTime);
        }
		public void CleanList()
        {
			for (int i = this.beamPoints.Count - 1; i >= 0; i--)
			{
				BeamPoint point = this.beamPoints[i];
				if (!point.target)
				{
					this.RemoveAt(i);
				}
			}
		}
		public void RemoveAt(int index)
        {
			BeamPoint point = this.beamPoints[index];
			if (point.effectInstance) Destroy(point.effectInstance);
			this.beamPoints.RemoveAt(index);
        }
        public void AddTarget(Transform transform)
        {
			// find closest point on the chain to transform position, using vector projection
			Vector3 position = transform.position;
			float maxDistanceSqr = Mathf.Infinity;
			BeamPoint closestPoint = beamPoints[0];
			BeamPoint secondClosestPoint = beamPoints[beamPoints.Count-1];

			// if there are only start and end points, just use those
			int closestIndex = 0;
			if(beamPoints.Count > 2)
            {
				for (int i = 0; i < beamPoints.Count; i++)
				{
					Vector3 point = beamPoints[i].position;
					float between = (point - position).sqrMagnitude;
					if (between < maxDistanceSqr)
					{
						maxDistanceSqr = between;
						secondClosestPoint = closestPoint;
						closestPoint = beamPoints[i];
						closestIndex = i;
					}
				}
			}
			// probably want to compare distance and only use closestpoint if position is further away than we want
			//Vector3 projection = Vector3.Project(position - secondClosestPoint.position, closestPoint.position - secondClosestPoint.position);
			// NVM LOLLLLLLLL
			Vector3 projection = ((closestPoint.position - secondClosestPoint.position) / 2f) + secondClosestPoint.position;
			BeamPoint beamPoint = new BeamPoint
			{
				startPos = projection,
				target = transform,			
			};
			if (pointEffectPrefab)
				beamPoint.effectInstance = GameObject.Instantiate(pointEffectPrefab, projection, Quaternion.identity);

			// insert new point after closest point
			// ensure its between the endpoints
			int j = Mathf.Clamp(closestIndex, 1, beamPoints.Count - 1);
			beamPoints.Insert(j, beamPoint);

			PointSoundManager.EmitSoundLocal((AkEventIdArg)this.pointSpawnSoundString, base.transform.position); // lazy fuck
		}

		private void UpdateBeamPositions(float deltaTime)
		{
			int count = beamPoints.Count;
			Vector3[] newPoints = new Vector3[count];
			this.lineRenderer.positionCount = count;
			for(int i = 0; i < count; i++)
            {
				BeamPoint point = beamPoints[i];
				point.age += deltaTime;

				Vector3 lerpedPos = Vector3.Lerp(point.startPos, point.target.transform.position, point.age / newPointLerp);
				point.position = lerpedPos;
				if (point.effectInstance) point.effectInstance.transform.position = lerpedPos;
				newPoints[i] = lerpedPos;

				beamPoints[i] = point;
            }
			this.lineRenderer.SetPositions(newPoints);
		}

        private void OnDestroy()
        {
			for (int i = this.beamPoints.Count - 1; i >= 0; i--)
			{
				this.RemoveAt(i);				
			}
		}
    }
}
