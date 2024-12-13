using MSU;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Projectile;
namespace SS2.Equipments
{
    public class AffixSuperFire : SS2EliteEquipment
    {
        public override SS2AssetRequest<EliteAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<EliteAssetCollection>("acSuperFire", SS2Bundle.Equipments);
        public static GameObject projectilePrefab;
        public static float projectileDamageCoefficient = 1f; // fucking jank ass number. theres a billion instances of damage
        public override void Initialize()
        {
            BuffOverlays.AddBuffOverlay(SS2Content.Buffs.BuffAffixSuperFire, SS2Assets.LoadAsset<Material>("matSuperFireOverlay", SS2Bundle.Equipments));
            projectilePrefab = SS2Assets.LoadAsset<GameObject>("SuperFireball", SS2Bundle.Equipments);
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
            body.RemoveBuff(RoR2Content.Buffs.AffixRed);
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
            body.AddBuff(RoR2Content.Buffs.AffixRed);
        }

        // guess im copying affixpurple
        public sealed class Behavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffAffixSuperFire;
            private static float projectileLaunchInterval = 8f;
            private static float maxTrackingDistance = 90f;
            private static float minDistance = 8f;
            private static float maxLaunchDistance = 50f;
            private static float bounceVelocity = 45f;
            private static float minSpread = 1f;
            private static float maxSpread = 3f;

            private float projectileTimer;
            private EntityStates.TitanMonster.FireFist.Predictor predictor; // might as well use this lol
            private SphereSearch sphereSearch;
            private void OnEnable()
            {
                predictor = new EntityStates.TitanMonster.FireFist.Predictor(base.transform);
                sphereSearch = new SphereSearch();
                projectileTimer = projectileLaunchInterval;
            }

            private void FixedUpdate()
            {
                if (base.characterBody.healthComponent.alive && NetworkServer.active)
                {
                    projectileTimer += Time.fixedDeltaTime;

                    // Periodically launch projectiles that create poison AOE fields
                    if (!predictor.hasTargetTransform && projectileTimer >= projectileLaunchInterval - 0.5f)
                    {
                        sphereSearch.radius = maxTrackingDistance;
                        sphereSearch.origin = base.characterBody.corePosition;
                        sphereSearch.mask = LayerIndex.entityPrecise.mask;
                        sphereSearch.RefreshCandidates();
                        TeamMask mask = TeamMask.allButNeutral;
                        mask.RemoveTeam(base.characterBody.teamComponent.teamIndex);
                        sphereSearch.FilterCandidatesByHurtBoxTeam(mask);
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
                Vector3 origin = base.characterBody.corePosition + Vector3.up * 2f;
                if (characterBody.characterMotor) origin = characterBody.characterMotor.capsuleHeight * Vector3.up + base.characterBody.footPosition;
                ProjectileManager.instance.FireProjectile(projectilePrefab, origin, Util.QuaternionSafeLookRotation(base.transform.forward), base.gameObject, base.characterBody.damage * projectileDamageCoefficient, 0f, false, DamageColorIndex.Default);
            }

        }
    }
}
