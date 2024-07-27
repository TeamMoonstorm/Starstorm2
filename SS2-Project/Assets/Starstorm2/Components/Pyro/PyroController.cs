using RoR2;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class PyroController : NetworkBehaviour
    {
        public CharacterBody characterBody;
        public InputBankTest inputBank;
        public float heatMax;

        [SyncVar(hook = "OnHeatModified")]
        private float _heat;
        public float heat
        {
            get
            {
                return _heat;
            }
        }

        public bool isMaxCharge
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
                    OnChargeModified(value);
                    syncVarHookGuard = false;
                }
                SetSyncVar<float>(value, ref _heat, 1U);
            }
        }

        public void OnEnable()
        {

        }

        public void AddHeat(float amount)
        {
            if (!NetworkServer.active)
            {
                SS2Log.Error("pyro controller add heat on client");
                return;
            }

            if (isMaxCharge)
                amount = 0f;

            Network_charge = Mathf.Clamp(heat + amount, 0f, heatMax);
        }

        private void OnChargeModified(float newCharge)
        {
            Network_charge = newCharge;
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
                OnChargeModified(reader.ReadSingle());
            }
        }
    }
}