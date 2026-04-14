using RoR2;
using RoR2.Skills;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class LancerController : NetworkBehaviour
    {
        public enum SpearState : byte
        {
            Held = 0,
            Thrown = 1,
            Returning = 2
        }

        [SerializeField]
        public SkillDef unarmedPrimarySkillDef;

        [SerializeField]
        public SkillDef recallSpearSkillDef;

        [SyncVar(hook = nameof(OnSpearStateChanged))]
        private SpearState _spearState;

        private GameObject _spearProjectileInstance;
        private GenericSkill primarySkill;
        private GenericSkill secondarySkill;

        public SpearState spearState => _spearState;

        public SpearState Network_spearState
        {
            get
            {
                return _spearState;
            }
            [param: In]
            set
            {
                if (NetworkServer.localClientActive && !syncVarHookGuard)
                {
                    syncVarHookGuard = true;
                    OnSpearStateChanged(value);
                    syncVarHookGuard = false;
                }
                SetSyncVar<SpearState>(value, ref _spearState, 1U);
            }
        }

        private void Awake()
        {
            if (TryGetComponent(out SkillLocator skillLocator))
            {
                primarySkill = skillLocator.primary;
                secondarySkill = skillLocator.secondary;
            }
            else
            {
                Debug.LogError("LancerController: Failed to get SkillLocator component.");
            }
        }

        public void SetSpearState(SpearState newState)
        {
            if (!NetworkServer.active)
                return;

            Network_spearState = newState;
        }

        private void OnSpearStateChanged(SpearState newState)
        {
            _spearState = newState;

            switch (newState)
            {
                case SpearState.Thrown:
                    ApplyUnarmedOverrides();
                    break;
                case SpearState.Held:
                    RemoveUnarmedOverrides();
                    break;
            }
        }

        public void SetSpearProjectile(GameObject go)
        {
            _spearProjectileInstance = go;
        }

        public GameObject GetSpearProjectile()
        {
            return _spearProjectileInstance;
        }

        public Vector3 GetSpearPosition()
        {
            if (_spearProjectileInstance)
                return _spearProjectileInstance.transform.position;

            return gameObject.transform.position;
        }

        private bool _overridesApplied;

        public void ApplyUnarmedOverrides()
        {
            if (_overridesApplied) return;
            _overridesApplied = true;
            primarySkill?.SetSkillOverride(this, unarmedPrimarySkillDef, GenericSkill.SkillOverridePriority.Contextual);
            secondarySkill?.SetSkillOverride(this, recallSpearSkillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        public void RemoveUnarmedOverrides()
        {
            if (!_overridesApplied) return;
            _overridesApplied = false;
            primarySkill?.UnsetSkillOverride(this, unarmedPrimarySkillDef, GenericSkill.SkillOverridePriority.Contextual);
            secondarySkill?.UnsetSkillOverride(this, recallSpearSkillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        private void OnDestroy()
        {
            RemoveUnarmedOverrides();
        }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.Write((byte)_spearState);
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
                writer.Write((byte)_spearState);
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
                OnSpearStateChanged((SpearState)reader.ReadByte());
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if ((num & 1) != 0)
            {
                OnSpearStateChanged((SpearState)reader.ReadByte());
            }
        }
    }
}
