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
    public class WardenChargeMeter : NetworkBehaviour
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

        [SyncVar(hook = nameof(OnChargeModified))]
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

        public void OnEnable()
        {
            OverlayCreationParams chargeOverlayCreationParams = new OverlayCreationParams
            {
                prefab = chargeOverlayPrefab,
                childLocatorEntry = chargeOverlayChildLocatorEntry
            };
            chargeOverlayController = HudOverlayManager.AddOverlay(gameObject, chargeOverlayCreationParams);
            chargeOverlayController.onInstanceAdded += OnChargeOverlayInstanceAdded;
            chargeOverlayController.onInstanceRemove += OnChargeOverlayInstanceRemoved;
            characterBody = GetComponent<CharacterBody>();
        }

        private void OnDisable()
        {
            if (chargeOverlayController != null)
            {
                chargeOverlayController.onInstanceAdded -= OnChargeOverlayInstanceAdded;
                chargeOverlayController.onInstanceRemove -= OnChargeOverlayInstanceRemoved;
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

        private void OnChargeOverlayInstanceAdded(OverlayController controller, GameObject instance)
        {
            fillUiList.Add(instance.GetComponent<ImageFillController>());
        }

        private void OnChargeOverlayInstanceRemoved(OverlayController controller, GameObject instance)
        {
            fillUiList.Remove(instance.transform.GetComponent<ImageFillController>());
        }

        public void AddCharge(float amount)
        {
            if (!NetworkServer.active)
            {
                SS2Log.Error("Warden Controller AddCharge network server error");
                return;
            }

            if (isMaxCharge && amount > 0)
                amount = 0f;

            _charge = Mathf.Clamp(charge + amount, 0f, chargeMax);
            Debug.Log("DEBUGGER What is Network_charge? " + _charge);
        }

        private void OnChargeModified(float newCharge)
        {
            _charge = newCharge;
        }

        //let him cook
        //          who let him cook he broke weaver
        //public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        //{
        //    if (forceAll)
        //    {
        //        writer.Write(_charge);
        //        return true;
        //    }
        //    bool flag = false;
        //    if ((syncVarDirtyBits & 1U) != 0U)
        //    {
        //        if (!flag)
        //        {
        //            writer.WritePackedUInt32(syncVarDirtyBits);
        //            flag = true;
        //        }
        //        writer.Write(_charge);
        //    }
        //    if (!flag)
        //    {
        //        writer.WritePackedUInt32(syncVarDirtyBits);
        //    }
        //    return flag;
        //}

        //public override void OnDeserialize(NetworkReader reader, bool initialState)
        //{
        //    if (initialState)
        //    {
        //        _charge = reader.ReadSingle();
        //        return;
        //    }
        //    int num = (int)reader.ReadPackedUInt32();
        //    if ((num & 1) != 0)
        //    {
        //        OnChargeModified(reader.ReadSingle());
        //    }
        //}
    }
}