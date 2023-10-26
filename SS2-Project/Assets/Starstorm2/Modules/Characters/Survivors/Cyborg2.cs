using RoR2;
using UnityEngine;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using R2API;
namespace Moonstorm.Starstorm2.Survivors
{
    [DisabledContent]
    public sealed class Cyborg2 : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("Cyborg2Body", SS2Bundle.Indev);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemmandoMonsterMaster", SS2Bundle.NemCommando);
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("survivorCyborg2", SS2Bundle.Indev);

        //configgggggggg
        public static int maxTeleporters = 1;
        public static int maxBloonTraps = 1;
        public static int maxShockMines = 6;

        public static DeployableSlot teleporter;
        public static DeployableSlot bloonTrap;
        public static DeployableSlot shockMine;
        public override void Initialize()
        {
            base.Initialize();

            teleporter = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) => { return maxTeleporters; });
            bloonTrap = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) => { return maxBloonTraps; });
            shockMine = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) => { return maxShockMines; });

            GameObject g1 = SS2Assets.LoadAsset<GameObject>("CyborgBuffTeleporter", SS2Bundle.Indev);
            g1.GetComponent<RoR2.Projectile.ProjectileDeployToOwner>().deployableSlot = teleporter;
            GameObject g2 = SS2Assets.LoadAsset<GameObject>("BloonTrap", SS2Bundle.Indev);
            g2.GetComponent<RoR2.Projectile.ProjectileDeployToOwner>().deployableSlot = bloonTrap;
            GameObject g3 = SS2Assets.LoadAsset<GameObject>("ShockMine", SS2Bundle.Indev);
            g3.GetComponent<RoR2.Projectile.ProjectileDeployToOwner>().deployableSlot = shockMine;

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
            var cb = BodyPrefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
            cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
        }
    }
}
