using MSU;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using RoR2.Projectile;
using UnityEngine.Networking;
using RoR2.Navigation;
namespace SS2.Equipments
{
    public class AffixSuperIce : SS2EliteEquipment
    {
        public override SS2AssetRequest<EliteAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<EliteAssetCollection>("acSuperIce", SS2Bundle.Equipments);
        public static GameObject projectilePrefab;
        public override void Initialize()
        {
            BuffOverlays.AddBuffOverlay(SS2Assets.LoadAsset<BuffDef>("BuffAffixSuperIce", SS2Bundle.Equipments), SS2Assets.LoadAsset<Material>("matSuperIceOverlay", SS2Bundle.Equipments));
            projectilePrefab = SS2Assets.LoadAsset<GameObject>("SuperIceMeteorPrediction", SS2Bundle.Equipments);
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
            body.RemoveBuff(RoR2Content.Buffs.AffixWhite);
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
            body.AddBuff(RoR2Content.Buffs.AffixWhite);
        }

        public sealed class Behavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffAffixSuperIce;
            private static float projectileLaunchInterval = 5f;
            private static float maxMeteorDistance = 150f;
            private static float projectileDamageCoefficient = 1f;
            private float projectileTimer;

            private void FixedUpdate()
            {
                if (base.characterBody.healthComponent.alive && NetworkServer.active)
                {
                    projectileTimer += Time.fixedDeltaTime;
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
                Vector3 targetPosition = origin + UnityEngine.Random.insideUnitSphere * maxMeteorDistance;
                Vector3 between = targetPosition - origin;
                float distance = Mathf.Min(between.magnitude, maxMeteorDistance);
                Vector3 target = between.normalized * distance + origin;
                if (SceneInfo.instance && SceneInfo.instance.groundNodes)
                {
                    NodeGraph nodes = SceneInfo.instance.groundNodes;
                    if (nodes.GetNodePosition(nodes.FindClosestNode(target, HullClassification.Human), out Vector3 node))
                    {
                        target = node;
                    }
                }
                else if (Physics.Raycast(target, Vector3.down, out RaycastHit raycastHit, 50f, LayerIndex.world.mask))
                {
                    target = raycastHit.point;
                }
                ProjectileManager.instance.FireProjectile(projectilePrefab, target, Util.QuaternionSafeLookRotation(base.transform.forward), base.gameObject, base.characterBody.damage * projectileDamageCoefficient, 0f, false, DamageColorIndex.Default);
            }
        }
    }
}
