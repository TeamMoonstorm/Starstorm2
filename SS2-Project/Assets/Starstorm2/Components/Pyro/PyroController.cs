using RoR2;
using RoR2.HudOverlay;
using RoR2.UI;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SS2.Components
{
    public class PyroController : NetworkBehaviour
    {
        public CharacterBody characterBody;
        public float heatMax = 100f;
        public float highHeat = 70f;
        public float heatPerIgnitedEnemyPerSecond = 5f;

        [HideInInspector]
        public static float enemyCheckInterval = 0.16f;
        [HideInInspector]
        private static float enemyCheckStopwatch = 0f;
        private SphereSearch bodySearch;
        private List<HurtBox> hits;
        public float enemyRadius = 35f;

        [SyncVar(hook = nameof(OnHeatModified))]
        private float _heat;
        public float heat
        {
            get
            {

                return _heat;
            }
        }

        public bool isHighHeat
        {
            get
            {
                return heat >= highHeat;
            }
        }

        public bool isMaxHeat
        {
            get
            {
                return heat >= heatMax;
            }
        }

        public void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
        }

        public void OnEnable()
        {
            hits = new List<HurtBox>();
            bodySearch = new SphereSearch();
            bodySearch.mask = LayerIndex.entityPrecise.mask;
            bodySearch.radius = enemyRadius;
        }
        private void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.J))
            {
                AddHeat(heatMax);
            }
#endif
        }
        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                enemyCheckStopwatch += Time.fixedDeltaTime;
                if (enemyCheckStopwatch >= enemyCheckInterval)
                {
                    enemyCheckStopwatch -= enemyCheckInterval;

                    float burnCount = 0f;

                    hits.Clear();

                    bodySearch.ClearCandidates();
                    bodySearch.origin = characterBody.corePosition;
                    bodySearch.RefreshCandidates();
                    bodySearch.FilterCandidatesByDistinctHurtBoxEntities();
                    bodySearch.GetHurtBoxes(hits);

                    if (hits.Count > 0)
                    {
                        foreach (HurtBox h in hits)
                        {
                            HealthComponent hp = h.healthComponent;
                            if (hp)
                            {
                                CharacterBody body = hp?.body;
                                if (body && !body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless))
                                {
                                    if (body.HasBuff(RoR2Content.Buffs.AffixRed))
                                    {
                                        burnCount++; //blazing elite are already hot
                                    }

                                    if (body.IsOnFirePyro())
                                    {
                                        burnCount++; //add burn for burning body
                                    }
                                }
                            }
                        }
                    }

                    float heatGain = burnCount * heatPerIgnitedEnemyPerSecond * enemyCheckInterval;
                    AddHeat(heatGain);
                }
            }
        }

        // This will have to be revisited
        public void AddHeat(float amount)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            _heat = Mathf.Clamp(heat + amount, 0f, heatMax);
        }

        private void OnHeatModified(float newHeat)
        {
            _heat = newHeat;
        }
    }
}