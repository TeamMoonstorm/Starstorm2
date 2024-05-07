using RoR2;
using UnityEngine;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using R2API;
using MSU;
using System.Collections;
using EntityStates.Cyborg2;
using System.Linq.Expressions;
using RoR2.ContentManagement;
using SS2.Buffs;
using UnityEngine.Networking;

#if DEBUG
namespace SS2.Survivors
{
    public sealed class Cyborg2 : SS2Survivor, IContentPackModifier
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acCyborg2", SS2Bundle.Indev);

        //configgggggggg
        internal static int maxTeleporters = 1;
        internal static int maxBloonTraps = 1;
        internal static int maxShockMines = 6;

        internal static DeployableSlot teleporter;
        private GameObject teleporterPrefab;
        internal static DeployableSlot bloonTrap;
        private GameObject bloonTrapPrefab;
        internal static DeployableSlot shockMine;
        private GameObject shockMinePrefab;

        public static float cooldownReduction = 0.5f;
        public static float percentHealthShieldPerSecond = 0.075f;
        

        public override void Initialize()
        {
            teleporter = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) => { return maxTeleporters; });
            bloonTrap = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) => { return maxBloonTraps; });
            shockMine = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) => { return maxShockMines; });

            teleporterPrefab.GetComponent<RoR2.Projectile.ProjectileDeployToOwner>().deployableSlot = teleporter;
            bloonTrapPrefab.GetComponent<RoR2.Projectile.ProjectileDeployToOwner>().deployableSlot = bloonTrap;
            shockMinePrefab.GetComponent<RoR2.Projectile.ProjectileDeployToOwner>().deployableSlot = shockMine;

            if (SS2Main.ScepterInstalled)
            {
                //ScepterCompat();
            }

            ModifyPrefab();
            On.RoR2.GenericSkill.RunRecharge += BuffTeleporter_AddRecharge;
        }

        private void BuffTeleporter_AddRecharge(On.RoR2.GenericSkill.orig_RunRecharge orig, GenericSkill self, float dt)
        {
            if (self.characterBody.HasBuff(SS2Content.Buffs.BuffCyborgTeleporter))
            {
                dt *= 1f / (1 - cooldownReduction);
            }
            orig(self, dt);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void ScepterCompat()
        {
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterSubmission"), "NemmandoBody", SkillSlot.Special, 0);
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterBossAttack"), "NemmandoBody", SkillSlot.Special, 1);
        }

        public void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
            cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
        }

        public sealed class CyborgTeleBuffBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation()]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffCyborgTeleporter;

            private void FixedUpdate()
            {
                if (!NetworkServer.active)
                    return;

                float maxHP = CharacterBody.healthComponent.fullHealth;
                CharacterBody.healthComponent.AddBarrier(maxHP * percentHealthShieldPerSecond * Time.fixedDeltaTime);
            }
        }
    }
}
#endif