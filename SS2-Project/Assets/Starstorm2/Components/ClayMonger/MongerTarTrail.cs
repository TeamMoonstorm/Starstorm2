using RoR2;
using System;
using Unity.Collections;
using UnityEngine;

namespace SS2.Components
{
    // Represents a component for a Clay Monger, this mostly just contains data thats used if the monger is killed.
    [RequireComponent(typeof(CharacterBody))]
    public class MongerTarTrail : MonoBehaviour
    {
        [NonSerialized]
        public new Transform transform;
        public CharacterBody characterBody { get; private set; }
        public TeamComponent teamComponent { get; private set; }
        public new GameObject gameObject { get; private set; }

        //A list of this monger's points.
        public NativeList<MongerTarTrailManager.TarPoint> points;

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            teamComponent = characterBody.teamComponent;
            gameObject = base.gameObject;
            transform = base.transform;
            points = new NativeList<MongerTarTrailManager.TarPoint>(MongerTarTrailManager.instance.totalPointsPerMonger, Allocator.Persistent);
        }

        private void OnEnable()
        {
            MongerTarTrailManager.instance.AddMonger(this);
        }

        private void OnDisable()
        {
            MongerTarTrailManager.instance.RemoveMonger(this);   
        }

        private void OnDestroy()
        {
            //We must free the memory consumed by the list manually, otherwise we cause a memory leak.
            if (points.IsCreated)
                points.Dispose();
        }

        public void AddPoint(RaycastHit hit)
        {
            float yRot = UnityEngine.Random.Range(0, 360);
            MongerTarTrailManager.TarPoint tarPoint = MongerTarTrailManager.instance.RequestTarPoint(this, hit.point, hit.normal, yRot, out var poolEntry);

            var pointTransform = poolEntry.tiedGameObject.transform;
            pointTransform.up = hit.normal;
            pointTransform.rotation *= Quaternion.Euler(0, yRot, 0);
            pointTransform.position = hit.point;
            points.Add(tarPoint);
        }
    }
}