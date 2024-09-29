using SS2;
using RoR2;
using RoR2.ContentManagement;
using MSU;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;
namespace SS2.Survivors
{
    public class Chef : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<VanillaSurvivorAssetCollection> assetRequest => SS2Assets.LoadAssetAsync<VanillaSurvivorAssetCollection>("acChef", SS2Bundle.Indev);

        private static float up = 2;
        private DamageAPI.ModdedDamageType oil;
        private GameObject hehe;
        public override void Initialize()
        {
            //oil = DamageAPI.ReserveDamageType();
            //hehe = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Chef/ChefGlazeProjectile.prefab").WaitForCompletion();
            //if(hehe)
            //{
            //    SS2Log.Info("Oil floats on water");
            //    hehe.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(oil);

            //    GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            //}
            
        }
        private void GlobalEventManager_onServerDamageDealt(DamageReport obj)
        {
            if(obj.damageInfo.HasModdedDamageType(oil) && obj.victimBody)
            {
                obj.victimBody.AddBuff(SS2Content.Buffs.bdOil);
            }
        }
        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public class Behavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdOil;

            private CharacterMotor characterMotor;
            private void Start()
            {
                characterMotor = base.GetComponent<CharacterMotor>();
            }

            private void FixedUpdate()
            {
                if (!characterBody.HasBuff(RoR2Content.Buffs.Weak))
                {
                    characterBody.RemoveBuff(SS2Content.Buffs.bdOil);
                    return;
                }
                if (hasAnyStacks && characterMotor && characterBody.HasBuff(SS2Content.Buffs.BuffStorm))
                {
                    if (characterMotor.isGrounded) characterMotor.Motor.ForceUnground();
                    characterMotor.velocity.y = Mathf.Max(up, characterMotor.velocity.y);
                }               
            }
        }
    }
}
