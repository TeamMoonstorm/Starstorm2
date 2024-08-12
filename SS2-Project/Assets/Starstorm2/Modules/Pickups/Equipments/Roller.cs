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
            }
            return true;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public class SquashedComponent : MonoBehaviour
        {
            public float speed = 5f;
            private Vector3 originalScale;

            private void Awake()
            {
                originalScale = transform.localScale;
                transform.localScale = new Vector3(1.25f * transform.localScale.x, 0.05f * transform.localScale.y, 1.25f * transform.localScale.z);

                StartCoroutine("EndSquash");
            }

            IEnumerator EndSquash()
            {
                yield return new WaitForSeconds(2f);

                float t = 0f;
                while (t < 1f)
                {
                    t += speed * Time.deltaTime;
                    transform.localScale = Vector3.Lerp(transform.localScale, originalScale, t);

                    yield return 0;
                }

                transform.localScale = originalScale;
                Destroy(this);

                yield return null;
            }
        }
    }
}