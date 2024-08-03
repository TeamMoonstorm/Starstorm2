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
        public float heatMax;

        public float numberAround;
        [HideInInspector]
        public static float enemyCheckInterval = 0.16f;
        [HideInInspector]
        private static float enemyCheckStopwatch = 0f;
        private SphereSearch bodySearch;
        private List<HurtBox> hits;
        public float enemyRadius = 25f;

        [Header("Heat UI")]
        [SerializeField]
        public GameObject heatOverlayPrefab;

        [SerializeField]
        public string heatOverlayChildLocatorEntry;
        private ChildLocator heatOverlayInstanceChildlocator;
        private OverlayController heatOverlayController;
        private List<ImageFillController> fillUiList = new List<ImageFillController>();
        private Text uiHeatText;

        [SyncVar(hook = "OnHeatModified")]
        private float _heat;
        public float heat
        {
            get
            {
                return _heat;
            }
        }

        public bool isMaxHeat
        {
            get
            {
                return heat >= heatMax;
            }
        }

        public float Network_charge
        {
            get
            {
                return _heat;
            }
            [param: In]
            set
            {
                if (NetworkServer.localClientActive && syncVarHookGuard)
                {
                    syncVarHookGuard = true;
                    OnHeatModified(value);
                    syncVarHookGuard = false;
                }
                SetSyncVar<float>(value, ref _heat, 1U);
            }
        }

        public void OnEnable()
        {
            OverlayCreationParams heatOverlayCreationParams = new OverlayCreationParams
            {
                prefab = heatOverlayPrefab,
                childLocatorEntry = heatOverlayChildLocatorEntry
            };
            heatOverlayController = HudOverlayManager.AddOverlay(gameObject, heatOverlayCreationParams);
            heatOverlayController.onInstanceAdded += OnHeatOverlayInstanceAdded;
            heatOverlayController.onInstanceRemove += OnHeatOverlayInstanceRemoved;
            characterBody = GetComponent<CharacterBody>();

            hits = new List<HurtBox>();
            bodySearch = new SphereSearch();
            bodySearch.mask = LayerIndex.entityPrecise.mask;
            bodySearch.radius = enemyRadius;
        }

        private void OnDisable()
        {
            if (heatOverlayController != null)
            {
                heatOverlayController.onInstanceAdded -= OnHeatOverlayInstanceAdded;
                heatOverlayController.onInstanceRemove -= OnHeatOverlayInstanceRemoved;
                fillUiList.Clear();
                HudOverlayManager.RemoveOverlay(heatOverlayController);
            }
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                UpdateUI();

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

                                    if (body.HasBuff(RoR2Content.Buffs.OnFire) || body.HasBuff(DLC1Content.Buffs.StrongerBurn))
                                    {
                                        burnCount++; //add burn for burning body

                                        if (body.hullClassification == HullClassification.Golem || body.hullClassification == HullClassification.BeetleQueen)
                                        {
                                            burnCount++; //bigger enemies burn hotter
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (characterBody)
                        characterBody.SetBuffCount(SS2Content.Buffs.bdPyroManiac.buffIndex, (int)burnCount);

                    AddHeat(burnCount * 0.25f);
                }
            }
        }

        private void UpdateUI()
        {
            foreach (ImageFillController imageFillController in fillUiList)
            {
                imageFillController.SetTValue(heat / heatMax);
            }
            if (heatOverlayInstanceChildlocator)
            {
                //heatOverlayInstanceChildlocator.FindChild("HeatThreshold").rotation = Quaternion.Euler(0f, 0f, Mathf.InverseLerp(0f, heatMax, heat) * -360f);
            }
        }

        private void OnHeatOverlayInstanceAdded(OverlayController controller, GameObject instance)
        {
            fillUiList.Add(instance.GetComponent<ImageFillController>());
            uiHeatText = instance.GetComponent<Text>();

            heatOverlayInstanceChildlocator = instance.GetComponent<ChildLocator>();
        }

        private void OnHeatOverlayInstanceRemoved(OverlayController controller, GameObject instance)
        {
            fillUiList.Remove(instance.transform.GetComponent<ImageFillController>());
        }

        public void AddHeat(float amount)
        {
            if (!NetworkServer.active)
            {
                SS2Log.Error("pyro controller add heat on client");
                return;
            }

            if (isMaxHeat && amount > 0)
                amount = 0f;

            Network_charge = Mathf.Clamp(heat + amount, 0f, heatMax);
        }

        private void OnHeatModified(float newHeat)
        {
            Network_charge = newHeat;
        }

        //let him cook
        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.Write(_heat);
                return true;
            }
            bool flag = false;
            if ((syncVarDirtyBits & 1U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(syncVarDirtyBits);
                    flag = true;
                }
                writer.Write(_heat);
            }
            if (!flag)
            {
                writer.WritePackedUInt32(syncVarDirtyBits);
            }
            return flag;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                _heat = reader.ReadSingle();
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if ((num & 1) != 0)
            {
                OnHeatModified(reader.ReadSingle());
            }
        }
    }
}