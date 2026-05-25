using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Equipments
{
    // needs spawn, idle, pickup, and self-destruct sound effects
    public class BallLightning : SS2Equipment
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acBallLightning", SS2Bundle.Equipments);

        private static GameObject projectilePrefab;
        private static GameObject muzzleflashEffectPrefab;
        private static GameObject attachmentPrefab;

        public static float selfDestructTime = 15f;
        public static float projectileDamageCoefficient = 1f;

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override void Initialize()
        {
            projectilePrefab = AssetCollection.FindAsset<GameObject>("BallLightningProjectile");
            muzzleflashEffectPrefab = AssetCollection.FindAsset<GameObject>("MuzzleflashBallLightning");

            On.RoR2.GenericPickupController.BodyHasPickupPermission += GenericPickupController_BodyHasPickupPermission;
        }

        // if the pickup is an equipment and the body has BallLightning, return false
        // youre stuck with it
        private bool GenericPickupController_BodyHasPickupPermission(On.RoR2.GenericPickupController.orig_BodyHasPickupPermission orig, CharacterBody body, UniquePickup pickupState)
        {
            if (body && body.inventory && body.inventory.currentEquipmentIndex == SS2Content.Equipments.BallLightning.equipmentIndex)
            {
                var pickupDef = PickupCatalog.GetPickupDef(pickupState.pickupIndex);
                if (pickupDef != null && pickupDef.equipmentIndex != EquipmentIndex.None)
                {
                    return false;
                }
            }

            return orig(body, pickupState);
        }

        public override bool Execute(EquipmentSlot slot)
        {
            if (NetworkServer.active)
            {
                Ray aimRay = slot.inputBank.GetAimRay();

                EffectManager.SimpleEffect(muzzleflashEffectPrefab, aimRay.origin, Quaternion.identity, true);

                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                fireProjectileInfo.projectilePrefab = projectilePrefab;
                fireProjectileInfo.position = aimRay.origin;
                fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
                fireProjectileInfo.owner = slot.gameObject;
                fireProjectileInfo.damage = slot.characterBody.damage * projectileDamageCoefficient;

                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }

            slot.subcooldownTimer = 1f;
            return true;
        }

        // refresh equipment cooldown
        public override void OnEquipmentObtained(CharacterBody body)
        {
            body.AddItemBehavior<BallLightningBehavior>(1);
            body.inventory.RestockActiveEquipmentCharges(1);
            
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
            body.AddItemBehavior<BallLightningBehavior>(0);
        }

        public class BallLightningBehavior : CharacterBody.ItemBehavior
        {
            private GameObject attachment;
            private void FixedUpdate()
            {
                bool shouldAttach = stack > 0;
                bool isAttached = attachment;

                if (shouldAttach != isAttached)
                {
                    if (shouldAttach)
                    {
                        attachment = GameObject.Instantiate<GameObject>(attachmentPrefab);
                        var networkedBodyAttachment = attachment.GetComponent<NetworkedBodyAttachment>();
                        networkedBodyAttachment.AttachToGameObjectAndSpawn(body.gameObject);
                    }
                }
            }

            private void OnDisable()
            {
                if (attachment)
                {
                    Destroy(attachment);
                }
            }

        }
    }
}