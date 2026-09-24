using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using RoR2;

namespace SS2.Components
{
    // Copy of DamageTrail, but we also spawn points based on distance traveled
    public class NemCrocoDamageTrail : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            RoR2Application.onUpdate += UpdateOptimizedDamageUpdateInterval;
        }

        [Tooltip("How often to drop a new point onto the trail.")]
        public float pointUpdateInterval = 0.2f;
        [Tooltip("How often to drop a new point onto the trail, based on distance moved.")]
        public float pointUpdateDistance = 2f;
        [Tooltip("How often the damage trail should deal damage.")]
        public float damageUpdateInterval = 0.2f;
        [Tooltip("How large the radius, or width, of the damage detection should be.")]
        public float radius = 4f;
        [Tooltip("How large the height of the damage detection should be.")]
        public float height = 0.5f;
        [Tooltip("How long a point on the trail should last.")]
        public float pointLifetime = 3f;
        [Tooltip("Which DamageType should this apply.")]
        public DamageTypeCombo damageType = DamageTypeCombo.Generic;
        [Tooltip("The line renderer to use for display.")]
        public LineRenderer lineRenderer;
        public bool active = true;
        [Tooltip("Prefab to use per segment.")]
        public GameObject segmentPrefab;
        public GameObject impactEffectPrefab;
        public bool destroyTrailSegments;
        public static bool doStretchingThing = false;
        public float damagePerSecond;
        public float procCoefficientPerSecond;
        public bool crit;
        public GameObject owner;

        private HashSet<GameObject> ignoredObjects = new HashSet<GameObject>();
        private TeamIndex teamIndex = TeamIndex.Neutral;
        private new Transform transform;
        private List<TrailPoint> pointsList;
        private float localTime;
        private float nextTrailPointUpdate;
        private float nextTrailDamageUpdate;
        private Vector3 lastPlacedPosition;
        private static float optimizedDamageUpdateinterval = 0.2f;
        private struct TrailPoint
        {
            public Vector3 position;
            public float localStartTime;
            public float localEndTime;
            public Transform segmentTransform;
        }

        private void Awake()
        {
            pointsList = new List<TrailPoint>();
            transform = base.transform;
        }

        private void Start()
        {
            localTime = 0f;
            AddPoint();
            AddPoint();
        }

        private static void UpdateOptimizedDamageUpdateInterval()
        {
            float MaxDamageUpdateInterval = 0.4f;
            float MinDamageUpdateInterval = 0.2f;
            float standardFPS = 60f;
            float fps = 1f / Time.deltaTime;
            if (fps > standardFPS)
            {
                optimizedDamageUpdateinterval = MinDamageUpdateInterval;
            }
            else
            {
                float slowRatio = (standardFPS - fps) / 30f;
                slowRatio = Mathf.Min(slowRatio, 1f);
                optimizedDamageUpdateinterval = Mathf.Lerp(MinDamageUpdateInterval, MaxDamageUpdateInterval, slowRatio);
            }
        }

        private void OnDisable()
        {
            if (EffectManager.UsePools)
            {
                for (int pointIndex = pointsList.Count - 1; pointIndex >= 0; pointIndex--)
                {
                    if (pointsList[pointIndex].segmentTransform)
                    {
                        GameObject segmentGO = pointsList[pointIndex].segmentTransform.gameObject;
                        EffectManagerHelper efh = segmentGO.GetComponent<EffectManagerHelper>();
                        if (efh != null && efh.OwningPool != null)
                        {
                            efh.OwningPool.ReturnObject(efh);
                        }
                    }
                    pointsList.RemoveAt(pointIndex);
                }
            }
        }

        private void FixedUpdate()
        {
            localTime += Time.fixedDeltaTime;
            Vector3 between = transform.position - lastPlacedPosition; ///////////////////////////////
            if (localTime >= nextTrailPointUpdate || between.sqrMagnitude >= pointUpdateDistance * pointUpdateDistance)
            {
                nextTrailPointUpdate += pointUpdateInterval;
                UpdateTrail(active);
            }

            if (localTime >= nextTrailDamageUpdate)
            {
                nextTrailDamageUpdate += optimizedDamageUpdateinterval;
                DoDamage(optimizedDamageUpdateinterval);
            }

            if (pointsList.Count > 0)
            {
                TrailPoint currentPoint = pointsList[pointsList.Count - 1];
                currentPoint.position = transform.position;
                currentPoint.localEndTime = localTime + pointLifetime;
                pointsList[pointsList.Count - 1] = currentPoint;

                if (currentPoint.segmentTransform)
                {
                    currentPoint.segmentTransform.position = transform.position;
                }

                if (lineRenderer)
                {
                    lineRenderer.SetPosition(pointsList.Count - 1, currentPoint.position);
                }
            }

            if (segmentPrefab)
            {
                Vector3 previousPosition = transform.position;
                for (int i = pointsList.Count - 1; i >= 0; i--)
                {
                    Transform segmentTransform = pointsList[i].segmentTransform;
                    if (segmentTransform)
                    {
                        //segmentTransform.LookAt(previousPosition, Vector3.up);
                        Vector3 diff = pointsList[i].position - previousPosition;
                        segmentTransform.position = previousPosition + diff * 0.5f;
                        //float t = Mathf.Clamp01(Mathf.InverseLerp(pointsList[i].localStartTime, pointsList[i].localEndTime, localTime));
                        //Vector3 segmentScale = new Vector3(radius * (1f - t), radius * (1f - t), diff.magnitude);
                        //segmentTransform.localScale = segmentScale;
                        previousPosition = pointsList[i].position;
                    }
                }
            }
            
            
        }

        private void UpdateTrail(bool addPoint)
        {
            while (pointsList.Count > 0 && pointsList[0].localEndTime <= localTime)
            {
                RemovePoint(0);
            }
            if (addPoint)
            {
                AddPoint();
            }

            if (lineRenderer)
            {
                UpdateLineRenderer(lineRenderer);
            }
        }

        private void DoDamage(float damageInterval)
        {
            if (NetworkServer.active && pointsList.Count > 0)
            {
                Vector3 previousPosition = pointsList[pointsList.Count - 1].position;
                ignoredObjects.Clear();
                TeamIndex teamIndex = TeamIndex.Neutral;
                float damage = damagePerSecond * damageInterval;

                if (owner)
                {
                    ignoredObjects.Add(owner);
                    teamIndex = TeamComponent.GetObjectTeam(owner);
                }

                DamageInfo damageInfo = new DamageInfo();
                damageInfo.attacker = owner;
                damageInfo.inflictor = base.gameObject;
                damageInfo.crit = crit;
                damageInfo.damage = damage;
                damageInfo.damageColorIndex = DamageColorIndex.Default;
                damageInfo.damageType = damageType;
                damageInfo.force = Vector3.zero;
                damageInfo.procCoefficient = procCoefficientPerSecond * damageInterval;
                for (int pointIndex = pointsList.Count - 2; pointIndex >= 0; pointIndex--)
                {
                    Vector3 currentPosition = pointsList[pointIndex].position;
                    Vector3 diff = currentPosition - previousPosition;
                    Vector3 boxHalfExtents = new Vector3(radius, height, diff.magnitude);
                    Vector3 boxCenter = Vector3.Lerp(currentPosition, previousPosition, 0.5f);
                    Quaternion boxOrientation = Util.QuaternionSafeLookRotation(diff);
                    Collider[] hits;
                    int hitsCount = HGPhysics.OverlapBox(out hits, boxCenter, boxHalfExtents, boxOrientation, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal);
                    for (int hitIndex = 0; hitIndex < hitsCount; hitIndex++)
                    {
                        Collider hitCollider = hits[hitIndex];
                        HurtBox hurtBox = hitCollider.GetComponent<HurtBox>();
                        if (hurtBox)
                        {
                            HealthComponent healthComponent = hurtBox.healthComponent;
                            if (healthComponent)
                            {
                                GameObject hitGameObject = healthComponent.gameObject;
                                if (ignoredObjects.Contains(hitGameObject))
                                {
                                    if (!FriendlyFireManager.ShouldSplashHitProceed(healthComponent, teamIndex))
                                    {
                                        ignoredObjects.Add(hitGameObject);
                                        damageInfo.position = hits[hitIndex].transform.position;
                                        damageInfo.inflictedHurtbox = hurtBox;
                                        healthComponent.TakeDamage(damageInfo);

                                        if (impactEffectPrefab)
                                        {
                                            EffectManager.SimpleEffect(impactEffectPrefab, damageInfo.position, Quaternion.identity, true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    HGPhysics.ReturnResults(hits);
                    previousPosition = currentPosition;
                }
            }
        }

        private void UpdateLineRenderer(LineRenderer lineRenderer)
        {
            lineRenderer.positionCount = pointsList.Count;
            for (int i = 0; i < pointsList.Count; i++)
            {
                lineRenderer.SetPosition(i, pointsList[i].position);
            }
        }

        private void AddPoint()
        {
            TrailPoint newPoint = new TrailPoint
            {
                position = transform.position,
                localStartTime = localTime,
                localEndTime = localTime + pointLifetime
            };

            if (segmentPrefab)
            {
                if (!EffectManager.ShouldUsePooledEffect(segmentPrefab))
                {
                    newPoint.segmentTransform = GameObject.Instantiate<GameObject>(segmentPrefab, transform).transform;
                }
                else
                {
                    EffectManagerHelper efh = EffectManager.GetAndActivatePooledEffect(segmentPrefab, transform, true);
                    newPoint.segmentTransform = efh.gameObject.transform;
                }
            }
            lastPlacedPosition = transform.position;
            pointsList.Add(newPoint);
        }

        private void RemovePoint(int pointIndex)
        {
            if (destroyTrailSegments)
            {
                if (pointsList[pointIndex].segmentTransform)
                {
                    if (!EffectManager.UsePools)
                    {
                        GameObject.Destroy(pointsList[pointIndex].segmentTransform.gameObject);
                    }
                    else
                    {
                        GameObject segmentGO = pointsList[pointIndex].segmentTransform.gameObject;
                        EffectManagerHelper efh = segmentGO.GetComponent<EffectManagerHelper>();
                        if (efh != null && efh.OwningPool != null)
                        {
                            efh.OwningPool.ReturnObject(efh);
                        }
                        else
                        {
                            GameObject.Destroy(segmentGO);
                        }
                    }
                }
            }
            else
            {
                if (EffectManager.UsePools)
                {
                    if (pointsList[pointIndex].segmentTransform)
                    {
                        GameObject segmentGO2 = pointsList[pointIndex].segmentTransform.gameObject;
                        segmentGO2.transform.SetParent(null);
                    }
                }
            }
            pointsList.RemoveAt(pointIndex);
        }
    }
}
