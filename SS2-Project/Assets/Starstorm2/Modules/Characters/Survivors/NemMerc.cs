using RoR2;
using UnityEngine;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using R2API;
using RoR2.Projectile;
namespace Moonstorm.Starstorm2.Survivors
{
    //[DisabledContent]
    public sealed class NemMerc : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemMercBody", SS2Bundle.Indev);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemmandoMonsterMaster", SS2Bundle.Indev);
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("survivorNemMerc", SS2Bundle.Indev);

        // configggggggg
        public static int maxClones = 1;
        public static int maxHolograms = 10;
        public static DeployableSlot clone;
        public static DeployableSlot hologram;
        public override void Initialize()
        {
            base.Initialize();
            this.CreateDeployables();
            if (Starstorm.ScepterInstalled)
            {
                //ScepterCompat();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void ScepterCompat()
        {
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterSubmission"), "NemmandoBody", SkillSlot.Special, 0);
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterBossAttack"), "NemmandoBody", SkillSlot.Special, 1);
        }

        public override void ModifyPrefab()
        {
            var cb = SS2Assets.LoadAsset<GameObject>("NemMercBody", SS2Bundle.Indev).GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
            cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();

            GameObject g1 = SS2Assets.LoadAsset<GameObject>("KnifeProjectile", SS2Bundle.Indev);
            g1.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(DamageTypes.RedirectHologram.damageType);
        }

        public void CreateDeployables()
        {
            clone = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) => { return maxHolograms + self.inventory.GetItemCount(DLC1Content.Items.EquipmentMagazineVoid); });
            hologram = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) => { return maxHolograms; });
        }
    }
}
