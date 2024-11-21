using RoR2;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class DUTController : NetworkBehaviour
    {
        public CharacterBody characterBody;
        public InputBankTest inputBank;

        public GameObject spherePrefab;
        public Light sphereLight;
        public ParticleSystem fireParticle;
        public ParticleSystem beamParticle;
        public ParticleSystem lightningParticle;
        public enum ChargeType
        {
            Damage,
            Healing
        }

        public ChargeType currentChargeType;

        [Header("Charge Values")]
        public float threshold;
        public float threshold2;
        public float thresholdMultiplier;
        public float threshold2Multiplier;
        public float chargeMax;


        [SyncVar(hook = "OnChargeModified")]
        private float _charge;
        public float charge
        {
            get
            {
                return _charge;
            }
        }

        public bool pastThreshold
        {
            get
            {
                return charge >= threshold;
            }
        }

        public bool pastThreshold2
        {
            get
            {
                return charge >= threshold2;
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
                    OnChargeModified(value);
                    syncVarHookGuard = false;
                }
                SetSyncVar<float>(value, ref _charge, 1U);
            }
        }

        public void OnEnable()
        {

        }

        public void AddCharge(float amount)
        {
            if (!NetworkServer.active)
            {
                SS2Log.Error("[Server] function DUTController.AddStress called on client");
                return;
            }

            if (!inputBank.skill1.down) //du-t can't store charge without charging - maybe revise later?
                return;

            if (pastThreshold && amount > 1) //check for amounts higher than 1 so energy flare bypasses first multiplier
                amount *= thresholdMultiplier;

            if (pastThreshold2)
                amount *= threshold2Multiplier;

            if (isMaxCharge)
                amount = 0f;

            Network_charge = Mathf.Clamp(charge + amount, 0f, chargeMax);
        }

        public void SwapChargeType()
        {
            if (currentChargeType == ChargeType.Damage)
            {
                currentChargeType = ChargeType.Healing;
                return;
            }

            if (currentChargeType == ChargeType.Healing)
            {
                currentChargeType = ChargeType.Damage;
                return;
            }
        }

        public void ModifySphere()
        {
            if (fireParticle == null || beamParticle == null || lightningParticle == null)
            {
                SS2Log.Error("DUTController failed to find sphere part for modifciation! Aborting...");
                return;
            }

            if (currentChargeType == ChargeType.Damage)
                fireParticle.startColor = Color.red;

            if (currentChargeType == ChargeType.Healing)
                fireParticle.startColor = Color.green;

            if (charge == 0)
            {
                fireParticle.gameObject.SetActive(false);

                beamParticle.gameObject.SetActive(false);

                lightningParticle.gameObject.SetActive(false);

                return;
            }
            else
                fireParticle.gameObject.SetActive(true);

            if (charge > threshold)
                lightningParticle.gameObject.SetActive(true);

            if (charge >= threshold2)
                beamParticle.gameObject.SetActive(true);

            float scaleCoefficient = Util.Remap(charge, 0f, chargeMax, 1f, 10f);
            spherePrefab.transform.localScale = new Vector3(scaleCoefficient, scaleCoefficient, scaleCoefficient);
        }

        public void AddChargeBypassThresholds(float amount)
        {
            if (!NetworkServer.active)
            {
                SS2Log.Error("[Server] function DUTController.AddStress called on client");
                return;
            }

            if (isMaxCharge)
                amount = 0f;

            Network_charge = Mathf.Clamp(charge + amount, 0f, chargeMax);
        }

        private void OnChargeModified(float newCharge)
        {
            Network_charge = newCharge;
            Debug.Log("can i just ... do shit here?");
            ModifySphere();
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
                OnChargeModified(reader.ReadSingle());
            }
        }
    }
}
