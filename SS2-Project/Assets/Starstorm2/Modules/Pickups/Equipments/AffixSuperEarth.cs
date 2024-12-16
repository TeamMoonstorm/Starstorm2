using MSU;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Projectile;
namespace SS2.Equipments
{
    // idk how to make this depend on DLC1
    public class AffixSuperEarth : SS2EliteEquipment
    {
        public override SS2AssetRequest<EliteAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<EliteAssetCollection>("acSuperEarth", SS2Bundle.Equipments);
        public static GameObject projectilePrefab;
        public static float projectileDamageCoefficient = 1f;
        public override void Initialize()
        {
            projectilePrefab = SS2Assets.LoadAsset<GameObject>("SuperEarthSunder", SS2Bundle.Equipments);
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
            body.RemoveBuff(DLC1Content.Buffs.EliteEarth);
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
            body.AddBuff(DLC1Content.Buffs.EliteEarth);
        }

        public sealed class Behavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffAffixSuperEarth;
            private TemporaryOverlayInstance temporaryOverlayInstance;
            private static float projectileLaunchInterval = 12f;
            private static int projectileCount = 3;
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
                Vector3 origin = base.characterBody.corePosition;
                float startAngle = UnityEngine.Random.Range(0, 360f / projectileCount);
                for (int i = 0; i < projectileCount; i++)
                {
                    float childAngle = startAngle + (i * 360f / projectileCount);
                    Vector3 rotation = Quaternion.Euler(0, childAngle, 0) * Vector3.forward;
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                    {
                        projectilePrefab = projectilePrefab,
                        position = origin + rotation,
                        rotation = Util.QuaternionSafeLookRotation(rotation),
                        procChainMask = default(ProcChainMask),
                        owner = base.gameObject,
                        damage = base.characterBody.damage * projectileDamageCoefficient,
                        crit = base.characterBody.RollCrit(),
                        force = 500f,
                        damageColorIndex = DamageColorIndex.Default,
                    };
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
            }
            private void OnEnable()
            {
                if (characterBody.modelLocator && characterBody.modelLocator.modelTransform)
                {
                    temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(characterBody.modelLocator.modelTransform.gameObject);
                    temporaryOverlayInstance.animateShaderAlpha = false;
                    temporaryOverlayInstance.destroyComponentOnEnd = true;
                    temporaryOverlayInstance.originalMaterial = SS2Assets.LoadAsset<Material>("matSuperEarthOverlay", SS2Bundle.Equipments);
                    temporaryOverlayInstance.AddToCharacterModel(characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>());
                }
            }
            private void OnDisable()
            {
                if (temporaryOverlayInstance != null)
                    temporaryOverlayInstance.RemoveFromCharacterModel();
            }

        }
    }
}
