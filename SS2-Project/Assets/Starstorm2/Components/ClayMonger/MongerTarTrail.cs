using HG;
using RoR2;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Networking;
using UnityEngine.ParticleSystemJobs;
using UnityEngine.Serialization;
using SS2.Jobs;

namespace SS2.Components
{
    [RequireComponent(typeof(TeamComponent))]
    public class MongerTarTrail : MonoBehaviour
    {
        public GameObject pointPrefab;
        public float pointLifetime;
        public float timeBetweenTrailUpdates;
        public float raycastLength;

        [Header("Check Data")]
        public float timeBetweenChecks;
        public float damageCoefficient;
        public BuffDef tarBuff;

        public TeamComponent teamComponent { get; private set; }
        private Transform _transform;
        private float _trailUpdateStopwatch;
        private float _trailCheckStopwatch;

        private List<TarPoint> _points = new List<TarPoint>();
        private void Awake()
        {
            _transform = transform;
            teamComponent = GetComponent<TeamComponent>();
        }

        private void OnEnable()
        {
            InstanceTracker.Add(this);
        }

        private void OnDisable()
        {
            InstanceTracker.Remove(this);
        }

        private static void SystemInit() => RoR2Application.onFixedUpdate += StaticFixedUpdate;

        private static void StaticFixedUpdate()
        {
            /*float deltaTime = Time.fixedDeltaTime;
            var instances = InstanceTracker.GetInstancesList<MongerTarTrail>();
            NativeArray<MongerFixedUpdateJob.MongerData> mongerDatas = new NativeArray<MongerFixedUpdateJob.MongerData>(instances.Count, Allocator.TempJob);

            for(int i = 0; i < instances.Count; i++)
            {
                var instance = instances[i];
                mongerDatas[i] = new MongerFixedUpdateJob.MongerData
                {
                    trailCheckStopwatch = instance._trailCheckStopwatch,
                    trailUpdateStopwatch = instance._trailUpdateStopwatch
                };
            }

            var job = new MongerFixedUpdateJob
            {
                datas = mongerDatas,
                deltaTime = deltaTime
            };
            var handle = job.Schedule(instances.Count, 10);
            handle.Complete();

            for(int i = 0; i < instances.Count; i++)
            {
                var instance = instances[i];
                instance._trailCheckStopwatch = mongerDatas[i].trailCheckStopwatch;
                instance._trailUpdateStopwatch = mongerDatas[i].trailUpdateStopwatch;
            }*/
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;
            _trailUpdateStopwatch += deltaTime;
            if (_trailUpdateStopwatch > timeBetweenTrailUpdates)
            {
                _trailUpdateStopwatch -= timeBetweenTrailUpdates;
                UpdateTrail();
            }
            _trailCheckStopwatch += deltaTime;
            if (_trailCheckStopwatch > timeBetweenChecks)
            {
                _trailCheckStopwatch -= timeBetweenChecks;
                CheckForBodiesOnTrail();
            }

            for (int i = _points.Count - 1; i >= 0; i--)
            {
                var point = _points[i];
                point.pointLifetime -= deltaTime;
                if (!point.pointTransform)
                {
                    _points[i] = point;
                    continue;
                }

                //Point should gradually shrink
                point.pointTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Util.Remap(point.pointLifetime, 0, point.totalLifetime, 0, 1));

                if(!point.isBubbling && !point.stoppedParticleSystem)
                {
                    point.stoppedParticleSystem = true;
                    if(point.particleSystem)
                    {
                        var emission = point.particleSystem.emission;
                        emission.enabled = false;
                    }
                }
                _points[i] = point;
            }
        }

        private void CheckForBodiesOnTrail()
        {

        }

        private void UpdateTrail()
        {
            //Clears dead points
            while (_points.Count > 0 && _points[0].pointLifetime <= 0)
            {
                RemovePoint(0);
            }
            AddPoint();
        }
        private void AddPoint()
        {
            //We should only place points if it looks realistic enough, since tar splotches in the air would look weird.
            if(!Physics.Raycast(_transform.position, Vector3.down, out var hit, raycastLength, LayerIndex.world.mask))
            {
                return;
            }

            TarPoint tarPoint = new TarPoint
            {
                worldPosition = hit.point,
                pointLifetime = pointLifetime,
                totalLifetime = pointLifetime,
                pointWidthDepth = new Vector2(5, 5)
            };

            if(!pointPrefab)
            {
                _points.Add(tarPoint);
                return;
            }

            if(!EffectManager.ShouldUsePooledEffect(pointPrefab))
            {
                var pointInstance = Instantiate(pointPrefab);
                var pointTransform = pointInstance.transform;

                pointTransform.up = hit.normal;
                pointTransform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
                tarPoint.pointTransform = pointTransform;
            }
            else
            {
                EffectManagerHelper helper = EffectManager.GetAndActivatePooledEffect(pointPrefab, hit.point, Quaternion.identity);
                var pointTransform = helper.transform;
                
                pointTransform.up = hit.normal;
                pointTransform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
                tarPoint.pointTransform = pointTransform;
                tarPoint.tiedHelper = helper;
                tarPoint.particleSystem = helper.GetComponentInChildren<ParticleSystem>();
                var emission = tarPoint.particleSystem.emission;
                emission.enabled = true;
            }
            _points.Add(tarPoint);
        }

        private void RemovePoint(int index)
        {
            _points.RemoveAt(index);
            var point = _points[index];
            if(point.pointTransform)
            {
                if(!EffectManager.UsePools)
                {
                    Destroy(point.pointTransform.gameObject);
                }
                else
                {
                    var helper = point.tiedHelper;
                    if(helper && helper.OwningPool != null)
                    {
                        helper.OwningPool.ReturnObject(helper);
                    }
                    else
                    {
                        Destroy(point.pointTransform.gameObject);
                    }
                }
            }
        }


        private struct TarPoint
        {
            public Vector3 worldPosition;
            public Vector2 pointWidthDepth;
            public float pointLifetime;
            public float totalLifetime;
            public Transform pointTransform;
            public EffectManagerHelper tiedHelper;
            public bool stoppedParticleSystem;
            public ParticleSystem particleSystem;

            public float halfLifetime => totalLifetime / 2;
            public bool isBubbling => pointLifetime > halfLifetime;
        }
    }
}