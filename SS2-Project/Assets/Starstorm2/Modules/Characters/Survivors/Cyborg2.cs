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
using System.Collections.Generic;
namespace SS2.Survivors
{
    public sealed class Cyborg2 : SS2Survivor, IContentPackModifier
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acCyborg2", SS2Bundle.Indev);

        //configgggggggg
        internal static int maxTeleporters = 1;
        internal static int maxBloonTraps = 1;
        internal static int maxShockMines = 3;

        internal static DeployableSlot teleporter;
        private GameObject teleporterPrefab;
        internal static DeployableSlot bloonTrap;
        private GameObject bloonTrapPrefab;
        internal static DeployableSlot shockMine;
        private GameObject shockMinePrefab;

        public static R2API.DamageAPI.ModdedDamageType applyCyborgPrime;

        public static float cooldownReduction = 0.5f;
        public static float percentHealthShieldPerSecond = 0.075f;

        public static GameObject primedEffectPrefab;
        public static GameObject primedExplosionPrefab;
        private struct ExplosionRequest
        {
            public HurtBox target;
            public Vector3 initialPosition;
            public GameObject attacker;
            public float damageTotal;
            public bool crit;
            public TeamIndex teamIndex;

        }
        private static readonly Queue<ExplosionRequest> explosionRequests = new Queue<ExplosionRequest>();
        private static float explosionTimer;
        private static float minExplosionInterval = 0.1f;
        public override void Initialize()
        {
            teleporter = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) => { return maxTeleporters; });
            bloonTrap = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) => { return maxBloonTraps; });
            shockMine = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) => { return maxShockMines; });

            SS2Assets.LoadAsset<GameObject>("ShockMine", SS2Bundle.Indev).GetComponent<RoR2.Projectile.ProjectileDeployToOwner>().deployableSlot = shockMine;
            SS2Assets.LoadAsset<GameObject>("CyborgBuffTeleporter", SS2Bundle.Indev).GetComponent<RoR2.Projectile.ProjectileDeployToOwner>().deployableSlot = teleporter;
            SS2Assets.LoadAsset<GameObject>("BloonTrap", SS2Bundle.Indev).GetComponent<RoR2.Projectile.ProjectileDeployToOwner>().deployableSlot = bloonTrap;

            applyCyborgPrime = DamageAPI.ReserveDamageType();
            SS2Assets.LoadAsset<GameObject>("MagnetProjectile", SS2Bundle.Indev).AddComponent<R2API.DamageAPI.ModdedDamageTypeHolderComponent>().Add(applyCyborgPrime);
            if (SS2Main.ScepterInstalled)
            {
                //ScepterCompat();
            }

            ModifyPrefab();

            On.RoR2.GenericSkill.RunRecharge += BuffTeleporter_AddRecharge;
            primedEffectPrefab = SS2Assets.LoadAsset<GameObject>("CyborgPrimeEffect", SS2Bundle.Indev);
            primedExplosionPrefab = SS2Assets.LoadAsset<GameObject>("CyborgPassiveExplosion", SS2Bundle.Indev);
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
            RoR2Application.onFixedUpdate += ProcessQueue;
            Stage.onStageStartGlobal += (_) => explosionRequests.Clear();
        }

        private void ProcessQueue()
        {
            explosionTimer -= Time.fixedDeltaTime;
            if (explosionTimer <= 0 && explosionRequests.Count > 0)
            {
                explosionTimer = minExplosionInterval;
                ExplosionRequest bombRequest = explosionRequests.Dequeue();

                Vector3 position = bombRequest.initialPosition;
                if(bombRequest.target)
                {
                    position = bombRequest.target.transform.position;                  
                }
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.position = position;
                blastAttack.baseDamage = bombRequest.damageTotal;
                blastAttack.baseForce = 600f;
                blastAttack.bonusForce = Vector3.zero;
                blastAttack.radius = 5f;
                blastAttack.attacker = bombRequest.attacker;
                blastAttack.inflictor = bombRequest.attacker;
                blastAttack.teamIndex = bombRequest.teamIndex;
                blastAttack.crit = bombRequest.crit;
                blastAttack.procChainMask = default(ProcChainMask); // :(
                blastAttack.procCoefficient = 1f;
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                blastAttack.damageColorIndex = DamageColorIndex.Default;
                blastAttack.damageType = DamageType.Generic;
                blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                blastAttack.Fire();
                EffectManager.SpawnEffect(primedExplosionPrefab, new EffectData
                {
                    origin = position,
                    scale = 5f,
                    rotation = Quaternion.identity
                }, true);
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
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

        // temporaryvisualeffects are hard coded :(
        // literally just holds the instance
        public sealed class PrimedBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation()]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffCyborgPrimed;

            private TemporaryVisualEffect instance;
            private void FixedUpdate()
            {
                CharacterBody.UpdateSingleTemporaryVisualEffect(ref instance, primedEffectPrefab, CharacterBody.radius, CharacterBody.HasBuff(SS2Content.Buffs.BuffCyborgPrimed));
            }
            private void OnEnable()
            {
                CharacterBody.UpdateSingleTemporaryVisualEffect(ref instance, primedEffectPrefab, CharacterBody.radius, CharacterBody.HasBuff(SS2Content.Buffs.BuffCyborgPrimed));
            }
            private void OnDisable()
            {
                CharacterBody.UpdateSingleTemporaryVisualEffect(ref instance, primedEffectPrefab, CharacterBody.radius, CharacterBody.HasBuff(SS2Content.Buffs.BuffCyborgPrimed));
            }
        }
        private void OnServerDamageDealt(DamageReport damageReport)
        {
            DamageInfo damageInfo = damageReport.damageInfo;
            CharacterBody body = damageReport.victimBody;
            CharacterBody attackerBody = damageReport.attackerBody;
            if (damageInfo.HasModdedDamageType(applyCyborgPrime)) 
            {
                if(!body.HasBuff(SS2Content.Buffs.BuffCyborgPrimed))// buffs can apparently stack even if canStack == false ?>???
                    body.AddBuff(SS2Content.Buffs.BuffCyborgPrimed);
            }
            else if (damageInfo.damage > 0 && body && attackerBody && body.HasBuff(SS2Content.Buffs.BuffCyborgPrimed))
            {
                body.RemoveBuff(SS2Content.Buffs.BuffCyborgPrimed);
                explosionRequests.Enqueue(new ExplosionRequest
                {
                    target = body.mainHurtBox,
                    attacker = attackerBody.gameObject,
                    damageTotal = 3f * attackerBody.damage,
                    crit = damageInfo.crit,
                    teamIndex = attackerBody.teamComponent.teamIndex,
                });
            }

            
        }
        private void BuffTeleporter_AddRecharge(On.RoR2.GenericSkill.orig_RunRecharge orig, GenericSkill self, float dt)
        {
            if (self.characterBody.HasBuff(SS2Content.Buffs.BuffCyborgTeleporter))
            {
                dt *= 1f / (1 - cooldownReduction);
            }
            orig(self, dt);
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