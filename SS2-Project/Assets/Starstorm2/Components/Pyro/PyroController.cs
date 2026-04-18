using RoR2;
using RoR2.HudOverlay;
using RoR2.UI;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
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
        public float enemyRadius = 35f;

        [HideInInspector]
        public static float enemyCheckInterval = 0.16f;
        [HideInInspector]
        private static float enemyCheckStopwatch = 0f;
        private SphereSearch bodySearch;
        private List<HurtBox> hits;
        

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
                return _isHighHeat;
            }
        }
        private bool _isHighHeat;
        public void SetHighHeat(bool newHighHeat)
        {
            _isHighHeat = newHighHeat;
        }

        public bool isMaxHeatAuthority
        {
            get
            {
                return heat >= heatMax;
            }
        }

        
        // just for telling the crosshair what to do
        public bool inNapalm { get; set; }
        private bool hasEffectiveAuthority;

        public void Awake()
        {
            // This seriously should never happen but I guess this modding community causes
            // insane incompats and users disable even cool shit like Pyro
            if (!TryGetComponent(out characterBody))
            {
                SS2Log.Error("PyroController.Awake: CharacterBody missing");
            }
        }
        private void Start()
        {
            UpdateAuthority();
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
            if (hasEffectiveAuthority)
            {
                AddHeat(Survivors.Pyro.passiveHeatPerSecond * Time.fixedDeltaTime);

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


        public override void OnStartAuthority()
        {
            UpdateAuthority();
        }
        public override void OnStopAuthority()
        {
            UpdateAuthority();
        }

        private void UpdateAuthority()
        {
            hasEffectiveAuthority = Util.HasEffectiveAuthority(gameObject);
        }

        // Pyro heat is now client authoritative. better for everyone except people spectating pyro.
        public void AddHeat(float amount)
        {
            if (hasEffectiveAuthority)
            {
                AddHeatInternal(amount);
                return;
            }

            if (NetworkServer.active)
            {
                RpcAddHeat(amount);
            }
            else
            {
                SS2Log.Error("PyroController.AddHeat called by non-authoritative client.");
            }
        }

        [ClientRpc]
        public void RpcAddHeat(float amount)
        {
            if (hasEffectiveAuthority)
            {
                AddHeatInternal(amount);
            }
        }
        public void AddHeatInternal(float amount)
        {
            _heat = Mathf.Clamp(heat + amount, 0f, heatMax);
        }
    }
}