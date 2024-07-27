using RoR2;
using RoR2.UI;
using RoR2.HudOverlay;
using System.Collections;
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
        public static float enemyCheckInterval = 0.03333333f;
        [HideInInspector]
        private static float enemyCheckStopwatch = 0f;
        private SphereSearch enemySearch;
        private List<HurtBox> hits;
        public float enemyRadius = 18f;

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
        }

        private void UpdateUI()
        {
            foreach (ImageFillController imageFillController in fillUiList)
            {
                imageFillController.SetTValue(heat / heatMax);
            }
            if (heatOverlayInstanceChildlocator)
            {
                heatOverlayInstanceChildlocator.FindChild("HeatThreshold").rotation = Quaternion.Euler(0f, 0f, Mathf.InverseLerp(0f, heatMax, heat) * -360f);
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

            if (isMaxHeat)
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