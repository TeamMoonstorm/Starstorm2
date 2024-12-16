using MSU;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using RoR2.Projectile;
using UnityEngine.Networking;
using RoR2.Navigation;
using System.Collections.Generic;
namespace SS2.Equipments
{
    public class AffixSuperIce : SS2EliteEquipment
    {
        public override SS2AssetRequest<EliteAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<EliteAssetCollection>("acSuperIce", SS2Bundle.Equipments);
        public static GameObject projectilePrefab;
        public override void Initialize()
        {            
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
            private static float projectileLaunchInterval = 4f;
            private static float maxMeteorDistance = 150f;
            private static float minimumDistanceBetweenMeteors = 32f;
            private static float delayBeforeExpiringAPreviousStrikePoint = 24f;
            private static int maxPlacementAttempts = 4;
            private static float projectileDamageCoefficient = 1f;
            private float projectileTimer;
            private TemporaryOverlayInstance temporaryOverlayInstance;
            private List<TimedServerPositionData> previousLightningPositions = new List<TimedServerPositionData>();
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
                //pick target. has a few attempts to try to choose a point away from other meteors
                Vector3 origin = base.characterBody.corePosition + Vector3.up * 2f;
                Vector3 target = origin + UnityEngine.Random.insideUnitSphere * maxMeteorDistance;
                for (int i = 0; i < maxPlacementAttempts; i++)
                {                                
                    Vector3 between = target - origin;
                    float distance = Mathf.Min(between.magnitude, maxMeteorDistance);
                    target = between.normalized * distance + origin;
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
                    if (!IsStrikePointTooCloseToPreviousPoint(target))
                    {
                        break;
                    }
                }
                previousLightningPositions.Add(new TimedServerPositionData(target, Run.instance.time));              
                ProjectileManager.instance.FireProjectile(projectilePrefab, target, Util.QuaternionSafeLookRotation(base.transform.forward), base.gameObject, base.characterBody.damage * projectileDamageCoefficient, 0f, false, DamageColorIndex.Default);
            }

            private void OnEnable()
            {
                if (characterBody.modelLocator && characterBody.modelLocator.modelTransform)
                {
                    temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(characterBody.modelLocator.modelTransform.gameObject);
                    temporaryOverlayInstance.animateShaderAlpha = false;
                    temporaryOverlayInstance.destroyComponentOnEnd = true;
                    temporaryOverlayInstance.originalMaterial = SS2Assets.LoadAsset<Material>("matSuperIceOverlay", SS2Bundle.Equipments);
                    temporaryOverlayInstance.AddToCharacterModel(characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>());
                }
            }
            private void OnDisable()
            {
                if (temporaryOverlayInstance != null)
                    temporaryOverlayInstance.RemoveFromCharacterModel();
            }

            private bool IsStrikePointTooCloseToPreviousPoint(Vector3 prospectivePosition)
            {
                float currentTime = (Run.instance != null) ? Run.instance.time : Time.time;
                float sqrMagnitude = minimumDistanceBetweenMeteors * minimumDistanceBetweenMeteors;
                int count = previousLightningPositions.Count - 1;
                while (count >= 0 && previousLightningPositions.Count > 0)
                {
                    if (currentTime - previousLightningPositions[count].timeGenerated >= delayBeforeExpiringAPreviousStrikePoint)
                    {
                        previousLightningPositions.RemoveAt(count);
                    }
                    else if ((previousLightningPositions[count].position - prospectivePosition).sqrMagnitude <= sqrMagnitude)
                    {
                        return true;
                    }
                    count--;
                }
                return false;
            }

            public struct TimedServerPositionData
            {
                public TimedServerPositionData(Vector3 _pos, float _time)
                {
                    this.position = _pos;
                    this.timeGenerated = _time;
                }
                public Vector3 position;

                public float timeGenerated;
            }
        }
    }
}
