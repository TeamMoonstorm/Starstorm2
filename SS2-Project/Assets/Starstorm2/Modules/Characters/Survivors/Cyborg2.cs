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
        public override SurvivorDef SurvivorDef => _survivorDef;
        private SurvivorDef _survivorDef;
        public override NullableRef<GameObject> MasterPrefab => _monsterMaster;
        private GameObject _monsterMaster;
        public override GameObject CharacterPrefab => _prefab;
        private GameObject _prefab;

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
        private BuffDef _buffCyborgPrimary;

        public static float cooldownReduction = 0.5f;
        public static float percentHealthShieldPerSecond = 0.075f;
        private BuffDef _buffCyborgTeleporter;


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

        public override IEnumerator LoadContentAsync()
        {
            ParallelAssetLoadCoroutineHelper helper = new ParallelAssetLoadCoroutineHelper();

            helper.AddAssetToLoad<GameObject>("CyborgBuffTeleporter", SS2Bundle.Indev);
            helper.AddAssetToLoad<GameObject>("BloonTrap", SS2Bundle.Indev);
            helper.AddAssetToLoad<GameObject>("ShockMine", SS2Bundle.Indev);
            helper.AddAssetToLoad<GameObject>("Cyborg2Body", SS2Bundle.Indev);
            helper.AddAssetToLoad<SurvivorDef>("survivorCyborg2", SS2Bundle.Indev);
            helper.AddAssetToLoad<BuffDef>("BuffCyborgPrimary", SS2Bundle.Indev);
            helper.AddAssetToLoad<BuffDef>("BuffCyborgTeleporter", SS2Bundle.Indev);

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            teleporterPrefab = helper.GetLoadedAsset<GameObject>("CyborgBuffTeleporter");
            bloonTrapPrefab = helper.GetLoadedAsset<GameObject>("BloonTrap");
            shockMinePrefab = helper.GetLoadedAsset<GameObject>("ShockMine");
            _prefab = helper.GetLoadedAsset<GameObject>("Cyborg2Body");
            _survivorDef = helper.GetLoadedAsset<SurvivorDef>("survivorCyborg2");
            _buffCyborgPrimary = helper.GetLoadedAsset<BuffDef>("BuffCyborgPrimary");
            _buffCyborgTeleporter = helper.GetLoadedAsset<BuffDef>("BuffCyborgTeleporter");
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void ScepterCompat()
        {
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterSubmission"), "NemmandoBody", SkillSlot.Special, 0);
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterBossAttack"), "NemmandoBody", SkillSlot.Special, 1);
        }

        public void ModifyPrefab()
        {
            var cb = _prefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
            cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.buffDefs.AddSingle(_buffCyborgPrimary);
        }

        public sealed class CyborgTeleBuffBehavior : BuffBehaviour
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