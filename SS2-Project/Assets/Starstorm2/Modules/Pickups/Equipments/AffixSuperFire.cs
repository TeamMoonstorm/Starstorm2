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
        public static float projectileDamageCoefficient = 2f; // fucking jank ass number. theres a billion instances of damage
        public override void Initialize()
        {
            projectilePrefab = SS2Assets.LoadAsset<GameObject>("SuperFireball", SS2Bundle.Equipments);
        }

        public override bool IsAvailable(ContentPack contentPack) => SS2Config.enableBeta;

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
        public sealed class Behavior : BaseBuffBehaviour, IOnTakeDamageServerReceiver
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffAffixSuperFire;
            private static float projectileLaunchInterval = 8f;
            private float projectileTimer;
            private TemporaryOverlayInstance temporaryOverlayInstance;

            private void OnEnable()
            {
                if (characterBody.modelLocator && characterBody.modelLocator.modelTransform)
                {
                    temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(characterBody.modelLocator.modelTransform.gameObject);
                    temporaryOverlayInstance.animateShaderAlpha = false;
                    temporaryOverlayInstance.destroyComponentOnEnd = true;
                    temporaryOverlayInstance.originalMaterial = SS2Assets.LoadAsset<Material>("matSuperFireOverlay", SS2Bundle.Equipments);
                    temporaryOverlayInstance.AddToCharacterModel(characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>());
                }
            }
            private void OnDisable()
            {
                if (temporaryOverlayInstance != null)
                    temporaryOverlayInstance.RemoveFromCharacterModel();
            }

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
                if (characterBody.characterMotor) origin = characterBody.characterMotor.capsuleHeight * Vector3.up + base.characterBody.footPosition;
                ProjectileManager.instance.FireProjectile(projectilePrefab, origin, Util.QuaternionSafeLookRotation(base.transform.forward), base.gameObject, base.characterBody.damage * projectileDamageCoefficient, 0f, false, DamageColorIndex.Default);
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                
            }
        }
    }
}
