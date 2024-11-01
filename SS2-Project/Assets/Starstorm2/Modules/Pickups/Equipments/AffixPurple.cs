using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static MSU.BaseBuffBehaviour;

namespace SS2.Equipments
{
    public class AffixPurple : SS2EliteEquipment
    {
        
        public override SS2AssetRequest<EliteAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<EliteAssetCollection>("acAffixPurple", SS2Bundle.Equipments);
        public static DotController.DotIndex index;
        private static GameObject poisonEffect;
        private static GameObject projectilePrefab;
        public override void Initialize()
        {
            poisonEffect = SS2Assets.LoadAsset<GameObject>("PoisonPurpleEffect", SS2Bundle.Equipments);
            projectilePrefab = SS2Assets.LoadAsset<GameObject>("PoisonBombProjectile", SS2Bundle.Equipments);
            index = DotAPI.RegisterDotDef(0.25f, 0.2f, DamageColorIndex.DeathMark, AssetCollection.FindAsset<BuffDef>("bdPurplePoison"));
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override bool Execute(EquipmentSlot slot)
        {
            return false;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public class PurplePoisonDebuff : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdPurplePoison;

            private GameObject effectInstace;
            private ParticleSystem particleSystem;
            private void OnEnable()
            {
                if(!effectInstace)
                    effectInstace = GameObject.Instantiate(poisonEffect);
                particleSystem = effectInstace.GetComponent<ParticleSystem>();
                if (particleSystem) particleSystem.Play(true);
            }

            private void OnDisable()
            {
                if (particleSystem) particleSystem.Stop(true);
            }

            private void OnDestroy()
            {
                if (effectInstace) Destroy(effectInstace);
            }
        }

        public sealed class AffixPurpleBehavior : BaseBuffBehaviour, IOnDamageDealtServerReceiver
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdElitePurple;
            private static float projectileLaunchInterval = 18f;
            private static float maxTrackingDistance = 90f;
            private static float maxLaunchDistance = 40f;
            private static float projectileSpeed = 35f;
            private static float timeToTarget = 2f;
            private static float timeVariance = 0.3f;
            private static float minSpread = 5f;
            private static float maxSpread = 25f;
            private static float projectileDamageCoefficient = 1f;
            private static float gravityCoefficient = 0.7f;
            private float projectileTimer;
            private EntityStates.TitanMonster.FireFist.Predictor predictor; // might as well use this lol
            private SphereSearch sphereSearch;
            private int numProjectiles;
            private void OnEnable()
            {
                predictor = new EntityStates.TitanMonster.FireFist.Predictor(base.transform);
                sphereSearch = new SphereSearch();
                switch(base.characterBody.hullClassification)
                {
                    case HullClassification.Human: 
                        numProjectiles = 1;
                        break;
                    case HullClassification.Golem:
                        numProjectiles = 2;
                        break;
                    case HullClassification.BeetleQueen:
                        numProjectiles = 3;
                        break;
                    default:
                        numProjectiles = 2;
                        break;
                }
            }
            public void OnDamageDealtServer(DamageReport damageReport)
            {

            }

            private void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    projectileTimer += Time.fixedDeltaTime;

                    // Periodically launch projectiles that create poison AOE fields
                    if(!predictor.hasTargetTransform && projectileTimer >= projectileLaunchInterval - 0.5f)
                    {                     
                        sphereSearch.radius = maxTrackingDistance;
                        sphereSearch.origin = base.characterBody.corePosition;
                        sphereSearch.mask = LayerIndex.entityPrecise.mask;
                        sphereSearch.RefreshCandidates();
                        sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                        HurtBox[] hurtBoxes = sphereSearch.GetHurtBoxes();
                        if (hurtBoxes.Length > 0)
                        {
                            int targetIndex = UnityEngine.Random.Range(0, hurtBoxes.Length - 1);
                            HurtBox hurtBox = hurtBoxes[targetIndex];
                            if (hurtBox)
                            {
                                this.predictor.SetTargetTransform(hurtBox.transform);
                            }
                        }
                    }
                    predictor.Update();
                    if (projectileTimer >= projectileLaunchInterval)
                    {
                        projectileTimer = 0f;
                        FireProjectiles();
                    }
                }
            }

            void FireProjectiles()
            {
                // pick target position. try predicting first, then pick random if failed
                Vector3 origin = base.characterBody.corePosition;
                Vector3 targetPosition = origin;
                bool predicted = predictor.GetPredictedTargetPosition(timeToTarget, out targetPosition);
                if (!predicted)
                {
                    Vector2 offset = UnityEngine.Random.insideUnitCircle * maxLaunchDistance;
                    targetPosition = origin;
                    targetPosition.x += offset.x;
                    targetPosition.z += offset.y;
                }
                predictor = new EntityStates.TitanMonster.FireFist.Predictor(base.transform);
                // calculate trajectory (speed and direction) to get to target position
                // horizontal speed and flight time is constant. solving for vertical speed and direction
                Ray ray = new Ray(origin, targetPosition - origin);
                Vector3 point = ray.GetPoint(maxLaunchDistance);             
                if (Util.CharacterRaycast(base.gameObject, ray, out var raycastHit, maxLaunchDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    point = raycastHit.point - origin;
                }
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                g.transform.position = point;
                Vector3 toTarget = point - origin;
                Vector2 toTargetHDirection = new Vector2(toTarget.x, toTarget.z).normalized;
                float y = Trajectory.CalculateInitialYSpeed(UnityEngine.Random.Range(-timeVariance, timeVariance) + timeToTarget, toTarget.y, Physics.gravity.y * gravityCoefficient);
                Vector3 velocity = new Vector3(toTargetHDirection.x * projectileSpeed, y, toTargetHDirection.y * projectileSpeed);
                float trueSpeed = velocity.magnitude;
                Vector3 aimDirection = velocity.normalized;

                Util.PlaySound("ChirrFireSpitBomb", base.gameObject);
                for (int i = 0; i < numProjectiles; i++)
                {                  
                    Vector3 spreadDirection = Util.ApplySpread(aimDirection, minSpread, maxSpread, 1, 1);
                    ProjectileManager.instance.FireProjectile(projectilePrefab, origin, Util.QuaternionSafeLookRotation(spreadDirection), base.gameObject, base.characterBody.damage * projectileDamageCoefficient, 0f, false, DamageColorIndex.Default, null, trueSpeed, null);
                }
            }
        }
    }
}
