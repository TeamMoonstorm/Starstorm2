using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ClayMonger
{
    public class DeathState : GenericCharacterDeath
    {
        public static GameObject initialEffect;
        public static GameObject deathEffect;
        public static float duration;

        private Transform _modelBaseTransform;
        private Transform _baseTransform;
        private bool _attemptedDeathBehaviour;
        private EffectData _effectData;
        private EffectManagerHelper _emh_initialEffect;

        public override void OnEnter()
        {
            base.OnEnter();
            if (!modelLocator)
                return;

            ChildLocator loc = GetModelChildLocator();
            if (!loc)
            {
                return;
            }

            _baseTransform = loc.FindChild("Base");
            if(initialEffect)
            {
                if(!EffectManager.ShouldUsePooledEffect(initialEffect))
                {
                    var gameObject = Object.Instantiate(initialEffect, _baseTransform.position, _baseTransform.rotation, _baseTransform);
                }
                else
                {
                    _emh_initialEffect = EffectManager.GetAndActivatePooledEffect(initialEffect, _baseTransform.transform.position, _baseTransform.rotation, _baseTransform);
                    _emh_initialEffect.transform.SetParent(_baseTransform);
                }
            }
            _modelBaseTransform = modelLocator.modelBaseTransform;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge >= duration)
            {
                AttemptDeathBehaviour();
            }
        }

        private void AttemptDeathBehaviour()
        {
            if (_attemptedDeathBehaviour)
                return;

            _attemptedDeathBehaviour = true;
            CleanupInitialEffect();
            if(deathEffect && NetworkServer.active)
            {
                if (_effectData == null)
                    _effectData = new EffectData();

                _effectData.origin = _baseTransform.position;
                EffectManager.SpawnEffect(deathEffect, _effectData, true);
            }

            if(_modelBaseTransform)
            {
                Destroy(_modelBaseTransform.gameObject);
                _modelBaseTransform = null;
            }
            if (NetworkServer.active)
            {
                DestroyBodyAsapServer();
            }
        }

        public override void OnExit()
        {
            if (!outer.destroying)
            {
                AttemptDeathBehaviour();
            }
            else
            {
                CleanupInitialEffect();
            }
            base.OnExit();
        }

        public void CleanupInitialEffect()
        {
            if (_emh_initialEffect != null && _emh_initialEffect.OwningPool != null)
            {
                _emh_initialEffect.OwningPool.ReturnObject(_emh_initialEffect);
                _emh_initialEffect = null;
            }
        }
    }
}