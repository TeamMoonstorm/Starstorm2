using System.Collections.Generic;
using System.Runtime.InteropServices;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class BarrierAuraController : NetworkBehaviour
    {
        private struct OwnerInfo
        {
            public readonly GameObject gameObject;

            public readonly Transform transform;

            public readonly CharacterBody characterBody;

            public readonly CameraTargetParams cameraTargetParams;

            public OwnerInfo(GameObject gameObject)
            {
                this.gameObject = gameObject;
                if ((bool)gameObject)
                {
                    transform = gameObject.transform;
                    characterBody = gameObject.GetComponent<CharacterBody>();
                    cameraTargetParams = gameObject.GetComponent<CameraTargetParams>();
                }
                else
                {
                    transform = null;
                    characterBody = null;
                    cameraTargetParams = null;
                }
            }
        }

        public float baseBarrierAuraInterval = 1.0f;

        public float barrierAuraRadius = 2f;

        public float barrierCoefficientPerTick = 0.1f;

        public float barrierAuraDuration = 5f;

        public BuffWard buffWard;

        private CameraTargetParams.AimRequest aimRequest;

        private float attackStopwatch;

        private float intervalStopwatch;

        [SyncVar]
        public GameObject owner;

        private OwnerInfo cachedOwnerInfo;

        public ParticleSystem[] auraParticles;

        public ParticleSystem[] procParticles;

        private new Transform transform;

        private float actualRadius;

        private float scaleVelocity;

        private NetworkInstanceId ___ownerNetId;

        public GameObject Networkowner
        {
            get
            {
                return owner;
            }
            [param: In]
            set
            {
                SetSyncVarGameObject(value, ref owner, 2u, ref ___ownerNetId);
            }
        }

        private void Awake()
        {
            transform = base.transform;
            intervalStopwatch = baseBarrierAuraInterval;
            if ((bool)buffWard)
            {
                buffWard.interval = baseBarrierAuraInterval;
            }
        }

        private void FixedUpdate()
        {
            if (cachedOwnerInfo.gameObject != owner)
            {
                cachedOwnerInfo = new OwnerInfo(owner);
            }
            UpdateRadius();
            if (NetworkServer.active)
            {
                if (!owner || attackStopwatch >= barrierAuraDuration)
                {
                    Object.Destroy(base.gameObject);
                    return;
                }
                attackStopwatch += Time.fixedDeltaTime;
                intervalStopwatch += Time.fixedDeltaTime;
            }
            if (NetworkServer.active && (bool)cachedOwnerInfo.characterBody && intervalStopwatch >= baseBarrierAuraInterval)
            {
                intervalStopwatch -= baseBarrierAuraInterval;
                Collider[] array = Physics.OverlapSphere((cachedOwnerInfo.characterBody ? cachedOwnerInfo.characterBody.corePosition : cachedOwnerInfo.transform.position), barrierAuraRadius, LayerIndex.entityPrecise.mask);

                foreach (Collider i in array)
                {
                    CharacterBody allyBody = Util.HurtBoxColliderToBody(i);
                    if (!(bool)allyBody)
                        continue;
                    HealthComponent allyHealthComponent = allyBody.healthComponent;
                    if (!(bool)allyHealthComponent)
                        continue;
                    TeamComponent allyTeamComponent = allyBody.teamComponent;
                    if (!(bool)(allyTeamComponent) || allyTeamComponent.teamIndex != cachedOwnerInfo.characterBody.teamComponent.teamIndex)
                        continue;
                    allyHealthComponent.AddBarrierAuthority(allyHealthComponent.fullCombinedHealth * barrierCoefficientPerTick);
                }
            }
            if ((bool)buffWard)
            {
                buffWard.Networkradius = actualRadius;
            }
        }

        private void UpdateRadius()
        {
            if ((bool)owner)
            {
                actualRadius = (cachedOwnerInfo.characterBody ? (cachedOwnerInfo.characterBody.radius + barrierAuraRadius) : 0f);
            }
            else
            {
                actualRadius = 0f;
            }
        }

        private void UpdateVisuals()
        {
            if ((bool)cachedOwnerInfo.gameObject)
            {
                transform.position = (cachedOwnerInfo.characterBody ? cachedOwnerInfo.characterBody.corePosition : cachedOwnerInfo.transform.position);
            }
            float num = Mathf.SmoothDamp(transform.localScale.x, actualRadius, ref scaleVelocity, 0.5f);
            transform.localScale = new Vector3(num, num, num);
        }

        private void OnDisable()
        {
            Util.PlaySound("Stop_item_proc_icicle", base.gameObject);
            ParticleSystem[] array = auraParticles;
            for (int i = 0; i < array.Length; i++)
            {
                ParticleSystem.MainModule main = array[i].main;
                main.loop = false;
            }
            aimRequest?.Dispose();
        }

        private void OnEnable()
        {
            Util.PlaySound("Play_item_proc_icicle", base.gameObject);
            if ((bool)cachedOwnerInfo.cameraTargetParams)
            {
                aimRequest = cachedOwnerInfo.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }
            ParticleSystem[] array = auraParticles;
            foreach (ParticleSystem obj in array)
            {
                ParticleSystem.MainModule main = obj.main;
                main.loop = true;
                obj.Play();
            }
        }

        private void OnIcicleGained()
        {
            ParticleSystem[] array = procParticles;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].Play();
            }
        }

        private void LateUpdate()
        {
            UpdateVisuals();
        }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.Write(owner);
                return true;
            }
            bool flag = false;
            if ((base.syncVarDirtyBits & (true ? 1u : 0u)) != 0)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
            }
            if ((base.syncVarDirtyBits & 2u) != 0)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.Write(owner);
            }
            if (!flag)
            {
                writer.WritePackedUInt32(base.syncVarDirtyBits);
            }
            return flag;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                ___ownerNetId = reader.ReadNetworkId();
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if (((uint)num & 2u) != 0)
            {
                owner = reader.ReadGameObject();
            }
        }

        public override void PreStartClient()
        {
            if (!___ownerNetId.IsEmpty())
            {
                Networkowner = ClientScene.FindLocalObject(___ownerNetId);
            }
        }
    }
}

