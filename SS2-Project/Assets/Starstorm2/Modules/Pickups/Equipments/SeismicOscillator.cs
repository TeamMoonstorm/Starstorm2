using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Equipments
{
    public class SeismicOscillator : SS2Equipment
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acSeismicOscillator", SS2Bundle.Indev);

        public static float duration = 6;
        public static int totalTicks = 24;
        public static float damageCoef = 1f;


        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override bool Execute(EquipmentSlot slot)
        {
            var behaviour = slot.GetComponent<EarthquakeBehaviour>();

            if (behaviour)
            {
                behaviour.ResetDeathStopwatch();
            }
            else
            {
                slot.characterBody.AddItemBehavior<EarthquakeBehaviour>(1);
            }
            return true;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public class EarthquakeBehaviour : CharacterBody.ItemBehavior
        {
            private SphereSearch _sphereSearch;
            private ShakeEmitter _emitter;
            private float _deathStopwatch;
            private float _timeBetweenTicks;
            private float _tickStopwatch;

            public void ResetDeathStopwatch()
            {
                _deathStopwatch = 0;
                if (_emitter)
                    _emitter.StartShake();
            }

            private void Awake()
            {
                _timeBetweenTicks = duration / totalTicks;
                _emitter = ShakeEmitter.CreateSimpleShakeEmitter(transform.position, new Wave
                {
                    amplitude = 0.25f,
                    cycleOffset = 1,
                    frequency = totalTicks,
                }, duration, 2048, false);
                Destroy(_emitter.GetComponent<DestroyOnTimer>());
                _sphereSearch = new SphereSearch();
                _sphereSearch.radius = 2048;
                _sphereSearch.mask = LayerIndex.entityPrecise.mask;
            }
            private void Start()
            {
                _emitter.StartShake();
            }

            private void FixedUpdate()
            {
                float deltaTime = Time.fixedDeltaTime;
                _deathStopwatch += deltaTime;
                if (_deathStopwatch > duration)
                {
                    Destroy(this);
                    return;
                }

                _tickStopwatch += deltaTime;
                if (_tickStopwatch > _timeBetweenTicks)
                {
                    _tickStopwatch -= _timeBetweenTicks;
                    if (NetworkServer.active)
                        TickDamageServer(deltaTime);
                }
            }

            private void TickDamageServer(float deltaTime)
            {
                _sphereSearch.origin = transform.position;
                _sphereSearch.RefreshCandidates()
                    .FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(body.teamComponent.teamIndex))
                    .FilterCandidatesByDistinctHurtBoxEntities();
                var hurtBoxes = _sphereSearch.GetHurtBoxes();
                foreach (var hurtBox in hurtBoxes)
                {
                    var hc = hurtBox.healthComponent;
                    if (!hc.TryGetComponent<CharacterMotor>(out var motor))
                        continue;

                    if (!motor.isGrounded)
                        continue;

                    hc.TakeDamage(new DamageInfo
                    {
                        attacker = gameObject,
                        canRejectForce = false,
                        crit = body.RollCrit(),
                        damage = body.damage * damageCoef,
                        damageColorIndex = DamageColorIndex.Item,
                        inflictor = gameObject,
                        procCoefficient = 1 / totalTicks,
                        position = hc.transform.position,
                    });
                }
            }

            private void OnDestroy()
            {
                if (_emitter)
                    Destroy(_emitter.gameObject);
            }
        }
    }
}