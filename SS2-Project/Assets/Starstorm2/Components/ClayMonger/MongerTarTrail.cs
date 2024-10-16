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
    [RequireComponent(typeof(CharacterBody))]
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
        public CharacterBody charBody { get; private set; }
        private Transform _transform;
        private float _trailUpdateStopwatch;
        private float _trailCheckStopwatch;

        private List<TarPoint> _points = new List<TarPoint>();
        private List<GameObject> _ignoredObjects = new List<GameObject>();
        private static List<MongerTarTrail> _instances = new List<MongerTarTrail>();
        private void Awake()
        {
            _transform = transform;
            teamComponent = GetComponent<TeamComponent>();
            charBody = GetComponent<CharacterBody>();
        }

        private void OnEnable()
        {
            _instances.Add(this);
        }
        private void OnDisable()
        {
            _instances.Remove(this);

            if (!EffectManager.UsePools)
            {
                for(int i = _points.Count - 1; i >= 0; i--)
                {
                    if (_points[i].pointTransform)
                    {
                        Destroy(_points[i].pointTransform.gameObject);
                    }
                    _points.RemoveAt(i);
                }
                return;
            }

            for(int i = _points.Count - 1; i >= 0; i--)
            {
                var point = _points[i];
                if(point.effectManagerHelper && point.effectManagerHelper.OwningPool != null)
                {
                    point.effectManagerHelper.OwningPool.ReturnObject(point.effectManagerHelper);
                }
                else
                {
                    Destroy(point.pointTransform.gameObject);
                }
            }
        }

        [SystemInitializer]
        private static void SystemInit() => SS2Main.onFixedUpdate += StaticFixedUpdate;

        private static void StaticFixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;
            for(int i = 0; i < _instances.Count; i++)
            {
                _instances[i].MyFixedUpdate(deltaTime);
            }
        }

        //I'm Gearbooxing
        private void MyFixedUpdate(float deltaTime)
        {
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
            if (!NetworkServer.active || _points.Count == 0)
                return;

            _ignoredObjects.Clear();
            TeamIndex attackerTeam = teamComponent.teamIndex;
            float damage = charBody.damage * damageCoefficient;
            _ignoredObjects.Add(gameObject);

            DamageInfo damageInfo = new DamageInfo();
            damageInfo.attacker = gameObject;
            damageInfo.inflictor = gameObject;
            damageInfo.crit = false;
            damageInfo.damage = damage;
            damageInfo.damageColorIndex = DamageColorIndex.Item;
            damageInfo.damageType = DamageType.Generic;
            damageInfo.force = Vector3.zero;
            damageInfo.procCoefficient = 0;
            for (int i = _points.Count - 1; i >= 0; i--)
            {
                var point = _points[i];
                var pointPos = point.worldPosition;
                var xy = Vector2.Lerp(Vector2.zero, point.pointWidthDepth, Util.Remap(point.pointLifetime, 0, point.totalLifetime, 0, 1));

                Collider[] colliders;
                int totalOverlaps = HGPhysics.OverlapBox(out colliders, pointPos, new Vector3(xy.x / 2, 0.5f, xy.y / 2), Quaternion.identity, LayerIndex.entityPrecise.mask);
                for(int j = 0; j < totalOverlaps; j++)
                {
                    var collider = colliders[j];
                    if(!collider.TryGetComponent<HurtBox>(out var hb))
                    {
                        continue;
                    }

                    HealthComponent hc = hb.healthComponent;
                    if (!hc)
                        continue;

                    if(!_ignoredObjects.Contains(hc.gameObject) && FriendlyFireManager.ShouldSplashHitProceed(hc, teamComponent.teamIndex))
                    {
                        _ignoredObjects.Add(hc.gameObject);
                        hc.body.AddTimedBuff(SS2Content.Buffs.bdMongerTar, timeBetweenChecks * 2);
                        if(point.isBubbling)
                        {
                            damageInfo.position = collider.transform.position;
                            hc.TakeDamage(damageInfo);
                        }
                    }
                }
                HGPhysics.ReturnResults(colliders);
            }

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
                tarPoint.effectManagerHelper = helper;
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
                    var helper = point.effectManagerHelper;
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
            public EffectManagerHelper effectManagerHelper;
            public bool stoppedParticleSystem;
            public ParticleSystem particleSystem;

            public float halfLifetime => totalLifetime / 2;
            public bool isBubbling => pointLifetime > halfLifetime;
        }
    }
}