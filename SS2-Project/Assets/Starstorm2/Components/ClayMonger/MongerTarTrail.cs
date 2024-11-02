using RoR2;
using System;
using System.Collections.Generic;
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
        public NativeArray<MongerTarTrailManager.TarPoint> points;
        private Queue<int> _freeIndices;

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            teamComponent = characterBody.teamComponent;
            gameObject = base.gameObject;
            transform = base.transform;
            points = new NativeArray<MongerTarTrailManager.TarPoint>(MongerTarTrailManager.instance.totalPointsPerMonger, Allocator.Persistent);
            _freeIndices = new Queue<int>();
            for(int i = 0; i < points.Length; i++)
            {
                points[i] = MongerTarTrailManager.TarPoint.invalid;
                _freeIndices.Enqueue(i);
            }
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
            if(!_freeIndices.TryDequeue(out var index))
            {
                return;
            }
            float yRot = UnityEngine.Random.Range(0, 360);
            MongerTarTrailManager.TarPoint tarPoint = MongerTarTrailManager.instance.RequestTarPoint(this, hit.point, hit.normal, yRot, index, out var poolEntry);

            var pointTransform = poolEntry.tiedGameObject.transform;
            pointTransform.up = hit.normal;
            pointTransform.rotation *= Quaternion.Euler(0, yRot, 0);
            pointTransform.position = hit.point;
            points[index] = tarPoint;
        }

        public void RemovePoint(MongerTarTrailManager.TarPoint tarPoint)
        {
            var index = tarPoint.ownerPointIndex;
            _freeIndices.Enqueue(index);
            points[index] = MongerTarTrailManager.TarPoint.invalid;
        }
    }
}