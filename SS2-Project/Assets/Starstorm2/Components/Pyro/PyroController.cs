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
        public float enemyRadius = 35f;

        [Header("Heat UI")]
        [SerializeField]
        public GameObject heatOverlayPrefab;

        [SerializeField]
        public string heatOverlayChildLocatorEntry;
        private ChildLocator heatOverlayInstanceChildlocator;
        private OverlayController heatOverlayController;
        private List<ImageFillController> fillUiList = new List<ImageFillController>();
        private Text uiHeatText;

        //jump air control stuff
        private CharacterMotor characterMotor;
        //able to be set by other parts of code, and this script will handle ramping it back to normal
            //that's professional speak for I couldn't be fucked to make an entitystate and threw it in this component
        public float? cachedAirControl;
        private float originalAirControl;

        [SerializeField, Tooltip("time multiplier for how long it should take to ramp air control back to normal when cachedAirControl is modified")]
        private float aircontroldecay;

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

        public void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            characterMotor = GetComponent<CharacterMotor>();
            originalAirControl = characterMotor.airControl;
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
            UpdateUI();

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

            if (cachedAirControl.HasValue)
            {
                cachedAirControl += Mathf.Clamp(Time.fixedDeltaTime / aircontroldecay, 0, originalAirControl);
                characterMotor.airControl = cachedAirControl.Value;
                if(cachedAirControl.Value >= originalAirControl)
                {
                    cachedAirControl = null;
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