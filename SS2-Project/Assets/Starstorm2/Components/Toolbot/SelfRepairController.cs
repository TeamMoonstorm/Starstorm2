using RoR2;
using RoR2.HudOverlay;
using RoR2.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class SelfRepairController : NetworkBehaviour
    {
        public float repairMax = 10f;
        public float repairGainPerUpdate = 0.10f;

        public GameObject repairOverlayPrefab;
        public string repairOverlayChildLocatorEntry = "CrosshairExtras";

        private CharacterBody characterBody;
        private OverlayController repairOverlayController;
        private List<ImageFillController> fillUiList = new List<ImageFillController>();

        [SyncVar(hook = nameof(OnRepairModified))]
        private float _repair;
        public float repair
        {
            get
            {
                return _repair;
            }
        }

        public bool isMaxRepair
        {
            get
            {
                return repair >= repairMax;
            }
        }

        public void OnEnable()
        {
            if (!TryGetComponent(out characterBody))
            {
                Debug.LogError("SelfRepairController: Missing CharacterBody on " + gameObject.name);
            }

            if (repairOverlayPrefab != null)
            {
                OverlayCreationParams repairOverlayCreationParams = new OverlayCreationParams
                {
                    prefab = repairOverlayPrefab,
                    childLocatorEntry = repairOverlayChildLocatorEntry
                };
                repairOverlayController = HudOverlayManager.AddOverlay(gameObject, repairOverlayCreationParams);
                repairOverlayController.onInstanceAdded += OnRepairOverlayInstanceAdded;
                repairOverlayController.onInstanceRemove += OnRepairOverlayInstanceRemoved;
            }
        }

        private void OnDisable()
        {
            if (repairOverlayController != null)
            {
                repairOverlayController.onInstanceAdded -= OnRepairOverlayInstanceAdded;
                repairOverlayController.onInstanceRemove -= OnRepairOverlayInstanceRemoved;
                fillUiList.Clear();
                HudOverlayManager.RemoveOverlay(repairOverlayController);
            }
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                if (!isMaxRepair)
                {
                    AddRepair(repairGainPerUpdate);
                }
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            foreach (ImageFillController imageFillController in fillUiList)
            {
                imageFillController.SetTValue(repair / repairMax);
            }
        }

        private void OnRepairOverlayInstanceAdded(OverlayController controller, GameObject instance)
        {
            if (instance.TryGetComponent(out ImageFillController ifc))
            {
                fillUiList.Add(ifc);
            }
            else
            {
                Debug.LogError("SelfRepairController: Overlay instance missing ImageFillController");
            }
        }

        private void OnRepairOverlayInstanceRemoved(OverlayController controller, GameObject instance)
        {
            if (instance.TryGetComponent(out ImageFillController ifc))
            {
                fillUiList.Remove(ifc);
            }
        }

        public void AddRepair(float amount)
        {
            if (!NetworkServer.active)
            {
                SS2Log.Error("SelfRepairController: AddRepair called on client");
                return;
            }

            if (isMaxRepair && amount > 0)
                amount = 0f;

            _repair = Mathf.Clamp(repair + amount, 0f, repairMax);
        }

        private void OnRepairModified(float newRepair)
        {
            _repair = newRepair;
        }
    }
}