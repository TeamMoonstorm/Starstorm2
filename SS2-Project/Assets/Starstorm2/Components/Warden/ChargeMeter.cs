using RoR2;
using RoR2.HudOverlay;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class ChargeMeter : NetworkBehaviour
    {
        public CharacterBody characterBody;

        [Header("Charge Meter UI")]
        [SerializeField]
        public GameObject chargeOverlayPrefab;

        [SerializeField]
        public string chargeOverlayChildLocatorEntry;


        public float chargeMax = 100f;

        private OverlayController chargeOverlayController;
        private List<ImageFillController> fillUiList = new List<ImageFillController>();

        [SyncVar(hook = "OnChargeModified")]
        private float _charge;
        public float charge
        {
            get
            {
                return _charge;
            }
        }

        public bool isMaxCharge
        {
            get
            {
                return charge >= chargeMax;
            }
        }

        public float Network_charge
        {
            get
            {
                return _charge;
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
                SetSyncVar<float>(value, ref _charge, 1U);
            }
        }

        public void OnEnable()
        {
            OverlayCreationParams repairOverlayCreationParams = new OverlayCreationParams
            {
                prefab = chargeOverlayPrefab,
                childLocatorEntry = chargeOverlayChildLocatorEntry
            };
            chargeOverlayController = HudOverlayManager.AddOverlay(gameObject, repairOverlayCreationParams);
            chargeOverlayController.onInstanceAdded += OnRepairOverlayInstanceAdded;
            chargeOverlayController.onInstanceRemove += OnRepairOverlayInstanceRemoved;
            characterBody = GetComponent<CharacterBody>();
        }

        private void OnDisable()
        {
            if (chargeOverlayController != null)
            {
                chargeOverlayController.onInstanceAdded -= OnRepairOverlayInstanceAdded;
                chargeOverlayController.onInstanceRemove -= OnRepairOverlayInstanceRemoved;
                fillUiList.Clear();
                HudOverlayManager.RemoveOverlay(chargeOverlayController);
            }
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            foreach (ImageFillController imageFillController in fillUiList)
            {
                imageFillController.SetTValue(charge / chargeMax);
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

            if (isMaxCharge && amount > 0)
                amount = 0f;

            Network_charge = Mathf.Clamp(charge + amount, 0f, chargeMax);
        }

        private void OnRepairModified(float newCharge)
        {
            Network_charge = newCharge;
        }

        //let him cook
        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.Write(_charge);
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
                writer.Write(_charge);
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
                _charge = reader.ReadSingle();
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