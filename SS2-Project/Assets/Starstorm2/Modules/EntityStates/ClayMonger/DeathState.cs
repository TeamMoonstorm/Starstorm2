using MSU;
using RoR2;
using SS2;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EntityStates.ClayMonger
{
    public class DeathState : GenericCharacterDeath
    {
        public static GameObject initialEffect;
        public static GameObject deathEffect;
        private static GameObject _omniExplosion;
        public static float duration;
        [Header("Explosion Data")]
        public static float explosionRadius;
        public static float explosionDamageCoefficient;
        public static float procCoefficient;
        public static float baseForce;
        public static Vector3 bonusForce;


        private Transform _modelTransform;
        private Transform _baseTransform;
        private bool _attemptedDeathBehaviour;
        private EffectData _effectData;
        private EffectManagerHelper _emh_initialEffect;

        [AsyncAssetLoad]
        private static IEnumerator Load()
        {
            var request = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFX.prefab");
            while (!request.IsDone)
                yield return null;

            _omniExplosion = request.Result;
        }

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
                    var t = Object.Instantiate(initialEffect).transform;
                    t.localPosition = Vector3.zero;
                    t.SetParent(_baseTransform);
                }
                else
                {
                    _emh_initialEffect = EffectManager.GetAndActivatePooledEffect(initialEffect, _baseTransform, true);
                }
            }

            _modelTransform = modelLocator.modelTransform;
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

            if(NetworkServer.active)
            {
                if (_effectData == null)
                    _effectData = new EffectData();

                _effectData.origin = _baseTransform.position;
                EffectManager.SpawnEffect(deathEffect, _effectData, true);
                Util.PlaySound("Play_clayboss_M1_explo", _baseTransform.gameObject);

                EffectManager.SpawnEffect(_omniExplosion, new EffectData
                {
                    origin = _baseTransform.position,
                    scale = explosionRadius,
                }, true);

                BlastAttack attack = new BlastAttack
                {
                    attacker = gameObject,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    baseDamage = damageStat * explosionDamageCoefficient,
                    baseForce = baseForce,
                    bonusForce = bonusForce,
                    canRejectForce = true,
                    crit = RollCrit(),
                    damageColorIndex = DamageColorIndex.Default,
                    falloffModel = BlastAttack.FalloffModel.Linear,
                    inflictor = gameObject,
                    losType = BlastAttack.LoSType.NearestHit,
                    position = _baseTransform.position,
                    procCoefficient = procCoefficient,
                    radius = explosionRadius,
                    teamIndex = teamComponent.teamIndex
                };

                var result = attack.Fire();
                for (int i = 0; i < result.hitCount; i++)
                {
                    var hit = result.hitPoints[i];
                    if (!hit.hurtBox)
                        continue;

                    if (!hit.hurtBox.healthComponent)
                        continue;

                    var body = hit.hurtBox.healthComponent.body;
                    if (body.bodyFlags.HasFlag(CharacterBody.BodyFlags.ImmuneToGoo))
                        continue;

                    if(!body.HasBuff(SS2Content.Buffs.bdMongerTar))
                    {
                        body.AddTimedBuff(SS2Content.Buffs.bdMongerTar, 5);
                    }
                }
            }

            if (_modelTransform)
            {
                Destroy(_modelTransform.gameObject);
                _modelTransform = null;
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
            if(_emh_initialEffect != null && _emh_initialEffect.OwningPool != null)
            {
                _emh_initialEffect.OwningPool.ReturnObject(_emh_initialEffect);
                _emh_initialEffect = null;
            }
        }
    }
}