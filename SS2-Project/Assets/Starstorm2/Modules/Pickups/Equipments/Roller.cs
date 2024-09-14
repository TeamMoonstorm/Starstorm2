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
    public class Roller : SS2Equipment
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acRoller", SS2Bundle.Indev);

        private static GameObject rollerProjectile;

        public static float duration = 6;
        public static int totalTicks = 24;
        public static float damageCoef = 1f;
        public static float damageStat = 1f;
        public static float force = 400f;


        public override void Initialize()
        {
            rollerProjectile = AssetCollection.FindAsset<GameObject>("RollerProjectile");
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override bool Execute(EquipmentSlot slot)
        {
            if (NetworkServer.active)
            {
                Debug.Log("Firing Roller");
                Debug.Log(rollerProjectile);

                Ray aimRay = slot.inputBank.GetAimRay();

                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                fireProjectileInfo.damageTypeOverride = DamageType.Generic;
                fireProjectileInfo.projectilePrefab = rollerProjectile;
                fireProjectileInfo.position = aimRay.origin;
                fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
                fireProjectileInfo.owner = slot.gameObject;
                fireProjectileInfo.damage =  damageStat * damageCoef;
                fireProjectileInfo.force = force;

                ProjectileManager.instance.FireProjectile(fireProjectileInfo);

                Debug.Log("Roller Projectile Fired");
            }
            return true;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }
    }
}