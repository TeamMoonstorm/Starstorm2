using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using SS2.Components;
using System;
namespace SS2.Equipments
{
    public class AffixPurple : SS2EliteEquipment
    {
        
        public override SS2AssetRequest<EliteAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<EliteAssetCollection>("acAffixPurple", SS2Bundle.Equipments);
        public static DotController.DotIndex poisonDotIndex;
        public static DamageAPI.ModdedDamageType poison;
        private static GameObject poisonEffect;
        private static GameObject projectilePrefab;
        private static GameObject explosionEffect;

        // TODO: Make configurable
        private static float projectileDamageCoefficient = .3f;
        private static float onHitRadius = 1.2f;
        private static float onHitDamageCoefficient = 0.1f;
        private static float poisonDamageCoefficient = 0.15f;
        private static float poisonDuration = 3f;

        public override void Initialize()
        {
            explosionEffect = SS2Assets.LoadAsset<GameObject>("PoisonPurpleExplosionEffect", SS2Bundle.Equipments);
            poisonEffect = SS2Assets.LoadAsset<GameObject>("PoisonPurpleEffect", SS2Bundle.Equipments);
            projectilePrefab = SS2Assets.LoadAsset<GameObject>("PoisonBombProjectile", SS2Bundle.Equipments);
            var dot = SS2Assets.LoadAsset<GameObject>("PoisonBombDotZone", SS2Bundle.Equipments);
            
            // damage coefficient is 1 per second so we can multiply it when applying
            poisonDotIndex = DotAPI.RegisterDotDef(0.25f, 0.25f, DamageColorIndex.Void, AssetCollection.FindAsset<BuffDef>("bdPurplePoison"));
            poison = DamageAPI.ReserveDamageType();

            dot.GetComponent<ProjectileExpandingDotZone>().moddedDamageType = poison;
            IL.RoR2.DotController.EvaluateDotStacksForType += EvaluateDotStacksForType;
            On.RoR2.GlobalEventManager.OnHitAllProcess += OnHitAllProcess;
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
        }
        public override bool IsAvailable(ContentPack contentPack) => SS2Config.enableBeta;
        public override bool Execute(EquipmentSlot slot)
        {
            return false;
        }
        public override void OnEquipmentLost(CharacterBody body)
        {
        }
        public override void OnEquipmentObtained(CharacterBody body)
        {
        }
        // aoe on hit
        private void OnHitAllProcess(On.RoR2.GlobalEventManager.orig_OnHitAllProcess orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            orig(self, damageInfo, hitObject);
            if (damageInfo.procCoefficient > 0 && damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody body) && body.HasBuff(SS2Content.Buffs.bdElitePurple))
            {
                float radius = onHitRadius;// * damageInfo.procCoefficient;
                EffectManager.SpawnEffect(explosionEffect, new EffectData
                {
                    origin = damageInfo.position,
                    scale = radius,
                    rotation = Util.QuaternionSafeLookRotation(damageInfo.force)
                }, true);
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.position = damageInfo.position;
                blastAttack.baseDamage = damageInfo.damage * onHitDamageCoefficient;
                blastAttack.baseForce = 0f;
                blastAttack.radius = radius;
                blastAttack.attacker = damageInfo.attacker;
                blastAttack.inflictor = null;
                blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
                blastAttack.crit = damageInfo.crit;
                blastAttack.procChainMask = damageInfo.procChainMask;
                blastAttack.procCoefficient = 0f;
                blastAttack.damageColorIndex = DamageColorIndex.Void;
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                blastAttack.damageType = DamageType.Generic;
                blastAttack.AddModdedDamageType(poison);
                blastAttack.Fire();
            }
        }
        // apply poison from damageType
        private void OnServerDamageDealt(DamageReport report)
        {
            var victimBody = report.victimBody;
            var damageInfo = report.damageInfo;
            if (damageInfo.HasModdedDamageType(poison))// && damageInfo.attacker && damageInfo.attacker.TryGetComponent<CharacterBody>(out var body)) && body.HasBuff(SS2Content.Buffs.bdElitePurple))
            {
                InflictDotInfo inflictDotInfo = new InflictDotInfo
                {
                    attackerObject = damageInfo.attacker,
                    victimObject = victimBody.gameObject,
                    damageMultiplier = poisonDamageCoefficient,
                    duration = poisonDuration,
                    dotIndex = poisonDotIndex,
                };
                DotController.InflictDot(ref inflictDotInfo);
                DotController dotController = DotController.FindDotController(victimBody.gameObject);
                if (!dotController) return;
                int j = 0;
                List<DotController.DotStack> dotStackList = dotController.dotStackList;
                while (j < dotStackList.Count)
                {
                    if (dotStackList[j].dotIndex == poisonDotIndex)
                    {
                        dotStackList[j].timer = Mathf.Max(dotStackList[j].timer, dotStackList[j].totalDuration);
                    }
                    j++;
                }
            }
        }
        // multiply poison dot damage by victim health fraction
        private void EvaluateDotStacksForType(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);

            // Locate the following line and modify the damage
            // damageInfo.damage = list[i].totalDamage
            // We allow for gaps between the ldfld (totalDamage) and stfld (damageInfo)
            // in case someone has injected their own instructions. Exactly like we're doing!
            bool b = c.TryGotoNext(
                x => x.MatchLdfld<DotController.PendingDamage>(nameof(DotController.PendingDamage.totalDamage))
            ) && c.TryGotoNext(
                x => x.MatchStfld<DamageInfo>(nameof(DamageInfo.damage))
            );
            if (b)
            {
                c.Emit(OpCodes.Ldarg_0); // dotcontroller
                c.Emit(OpCodes.Ldarg_1); // dotindex
                c.EmitDelegate<Func<float, DotController, DotController.DotIndex, float>>((dmg, dc, dot) =>
                {
                    if (dot == poisonDotIndex)
                    {
                        float remap = Util.Remap(dc.victimHealthComponent.health / dc.victimHealthComponent.fullHealth, 0, 1, .05f, 1); // min 5% of og damage
                        dmg *= remap;
                        if (dmg < 1) dmg = 1;
                    }
                    return dmg;
                });
            }
            else
            {
                SS2Log.Fatal("AffixPurple.EvaluateDotStacksForType: ILHook failed.");
            }
        }
        // basically temporaryvisualeffect
        public class PurplePoisonDebuff : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdPurplePoison;

            private GameObject effectInstace;
            private ParticleSystem ps;
            private void OnEnable()
            {
                if (!effectInstace)
                    effectInstace = GameObject.Instantiate(poisonEffect);
                ps = effectInstace.GetComponent<ParticleSystem>();
                ps.Play(true);
            }

            private void OnDisable()
            {
                ps.Stop(true);
            }

            private void OnDestroy()
            {
                if (effectInstace) Destroy(effectInstace);
            }
        }
        // fire projectiles with prediction
        public sealed class AffixPurpleBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdElitePurple;
            private static float projectileLaunchInterval = 13f;
            private static float maxTrackingDistance = 60f;
            private static float minDistance = 8f;
            private static float maxLaunchDistance = 40f;
            private static float timeToTarget = 1f;
            private static float timeVariance = 0.3f;
            private static float minSpread = 1f;
            private static float maxSpread = 3f;
            private static float spreadPerProjectile = 18f;           
            
            private float projectileTimer;
            private EntityStates.TitanMonster.FireFist.Predictor predictor; // might as well use this lol
            private SphereSearch sphereSearch;
            private int numProjectiles;
            private void OnEnable()
            {
                predictor = new EntityStates.TitanMonster.FireFist.Predictor(base.transform);
                sphereSearch = new SphereSearch();
                switch(base.characterBody.hullClassification)
                {
                    case HullClassification.Human: 
                        numProjectiles = 1;
                        break;
                    case HullClassification.Golem:
                        numProjectiles = 2;
                        break;
                    case HullClassification.BeetleQueen:
                        numProjectiles = 3;
                        break;
                    default:
                        numProjectiles = 2;
                        break;
                }
                projectileTimer = projectileLaunchInterval - 6f;
            }

            private void FixedUpdate()
            {
                if (base.characterBody.healthComponent.alive && NetworkServer.active)
                {
                    projectileTimer += Time.fixedDeltaTime;

                    // Periodically launch projectiles that create poison AOE fields
                    if(!predictor.hasTargetTransform && projectileTimer >= projectileLaunchInterval - 0.5f)
                    {                     
                        sphereSearch.radius = maxTrackingDistance;
                        sphereSearch.origin = base.characterBody.corePosition;
                        sphereSearch.mask = LayerIndex.entityPrecise.mask;
                        sphereSearch.RefreshCandidates();
                        TeamMask mask = TeamMask.allButNeutral;
                        mask.RemoveTeam(base.characterBody.teamComponent.teamIndex);
                        sphereSearch.FilterCandidatesByHurtBoxTeam(mask);
                        sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();                      
                        HurtBox[] hurtBoxes = sphereSearch.GetHurtBoxes();
                        if (hurtBoxes.Length > 0)
                        {
                            int targetIndex = UnityEngine.Random.Range(0, hurtBoxes.Length - 1);
                            HurtBox hurtBox = hurtBoxes[targetIndex];
                            if (hurtBox)
                            {
                                this.predictor.SetTargetTransform(hurtBox.transform);
                            }
                        }
                    }
                    predictor.Update();
                    if (projectileTimer >= projectileLaunchInterval)
                    {
                        projectileTimer = 0f;
                        FireProjectiles();
                    }
                }
            }

            void FireProjectiles()
            {
                // pick target position. try predicting first, then pick random if failed
                Vector3 origin = base.characterBody.corePosition;
                Vector3 targetPosition = origin;
                float timeToTarget = UnityEngine.Random.Range(-timeVariance, timeVariance) + AffixPurpleBehavior.timeToTarget;
                bool predicted = predictor.isPredictionReady && predictor.GetPredictedTargetPosition(timeToTarget, out targetPosition);
                if (!predicted)
                {
                    Vector2 offset = UnityEngine.Random.insideUnitCircle * maxLaunchDistance;
                    targetPosition = origin;
                    targetPosition.x += offset.x;
                    targetPosition.z += offset.y;
                }
                predictor = new EntityStates.TitanMonster.FireFist.Predictor(base.transform);
                // calculate trajectory (speed and direction) to get to target position
                // horizontal speed and flight time is constant. solving for vertical speed and direction

                //raycasting. maybe good maybe not
                //Ray ray = new Ray(origin, targetPosition - origin);
                //Vector3 point = ray.GetPoint(maxLaunchDistance);             
                //if (Util.CharacterRaycast(base.gameObject, ray, out var raycastHit, maxLaunchDistance, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore))
                //{
                //    point = raycastHit.point;
                //}
                Vector3 point = targetPosition;
                Vector3 toTarget = point - origin;
                Vector2 toTargetH = new Vector2(toTarget.x, toTarget.z);
                float hDistance = toTargetH.magnitude;
                Vector2 toTargetHDirection = toTargetH / hDistance;
                hDistance = Mathf.Clamp(hDistance, minDistance, maxLaunchDistance);                                      
                float vSpeed = Trajectory.CalculateInitialYSpeed(timeToTarget, toTarget.y);
                float hSpeed = hDistance / timeToTarget;
                Vector3 velocity = new Vector3(toTargetHDirection.x * hSpeed, vSpeed, toTargetHDirection.y * hSpeed);
                float trueSpeed = velocity.magnitude;
                Vector3 aimDirection = velocity.normalized;

                for (int i = 0; i < numProjectiles; i++)
                {                  

                    Vector3 spreadDirection = Util.ApplySpread(aimDirection, minSpread + spreadPerProjectile*i, maxSpread + spreadPerProjectile*i, 1, 1);
                    ProjectileManager.instance.FireProjectile(projectilePrefab, origin, Util.QuaternionSafeLookRotation(spreadDirection), base.gameObject, base.characterBody.damage * projectileDamageCoefficient, 0f, false, DamageColorIndex.Default, null, trueSpeed, null);
                }
            }
        }
    }
}
