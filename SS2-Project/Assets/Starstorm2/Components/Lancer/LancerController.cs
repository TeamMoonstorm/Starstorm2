using RoR2;
using RoR2.Skills;
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

        [Server]
        public void SetSpearState(SpearState newState)
        {
            OnSpearStateChanged(newState);
            _spearState = newState;
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
    }
}
