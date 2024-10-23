using RoR2;
using RoR2.HudOverlay;
using RoR2.UI;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class SelfRepairController : NetworkBehaviour
    {
        public CharacterBody characterBody;
        public float repairMax = 10f;
        public float repairGainPerUpdate = 0.01f;

        public GameObject repairOverlayPrefab = SS2.Survivors.Toolbot.RepairOverlayPrefab;

        public string repairOverlayChildLocatorEntry = "CrosshairExtras";
        private OverlayController repairOverlayController;
        private List<ImageFillController> fillUiList = new List<ImageFillController>();

        [SyncVar(hook = "OnRepairModified")]
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

        public float Network_charge
        {
            get
            {
                return _repair;
            }
            [param: In]
            set
            {
                if (NetworkServer.localClientActive && syncVarHookGuard)
                {
                    syncVarHookGuard = true;
                    OnRepairModified(value);
                    syncVarHookGuard = false;
                }
                SetSyncVar<float>(value, ref _repair, 1U);
            }
        }

        public void OnEnable()
        {
            OverlayCreationParams repairOverlayCreationParams = new OverlayCreationParams
            {
                prefab = repairOverlayPrefab,
                childLocatorEntry = repairOverlayChildLocatorEntry
            };
            repairOverlayController = HudOverlayManager.AddOverlay(gameObject, repairOverlayCreationParams);
            repairOverlayController.onInstanceAdded += OnRepairOverlayInstanceAdded;
            repairOverlayController.onInstanceRemove += OnRepairOverlayInstanceRemoved;
            characterBody = GetComponent<CharacterBody>();
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
                UpdateUI();

                if (!isMaxRepair)
                {
                    AddRepair(repairGainPerUpdate);
                }
            }
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
            fillUiList.Add(instance.GetComponent<ImageFillController>());
        }

        private void OnRepairOverlayInstanceRemoved(OverlayController controller, GameObject instance)
        {
            fillUiList.Remove(instance.transform.GetComponent<ImageFillController>());
        }

        public void AddRepair(float amount)
        {
            if (!NetworkServer.active)
            {
                SS2Log.Error("self repair controller add repair on client");
                return;
            }

            if (isMaxRepair && amount > 0)
                amount = 0f;

            Network_charge = Mathf.Clamp(repair + amount, 0f, repairMax);
        }

        private void OnRepairModified(float newRepair)
        {
            Network_charge = newRepair;
        }

        //let him cook
        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.Write(_repair);
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
                writer.Write(_repair);
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
                _repair = reader.ReadSingle();
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if ((num & 1) != 0)
            {
                OnRepairModified(reader.ReadSingle());
            }
        }
    }
}