using SS2;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using HG;

namespace EntityStates.Lamp
{
    public class HealBeam : BaseSkillState
    {
        private static float baseDuration = 6f;
        private static float buffDuration = 4f;
        private static float tickInterval = 0.2f;
        private static float totalDamageCoefficient = 9f;
        
        private static DamageType damageType = DamageType.Generic;
        private static float procCoefficientPerTick = 0.4f;
        private static float totalHealCoefficient = 8f;
        private static float enemyPrioritySearchRadius = 19f;
        private static float searchRadius = 27f;
        private static float beamBreakDistance = 36f;
        private static int maxBeams = 1;
        private static bool ignoreMass = true;

        private static float forceMagnitude = -1.25f;
        private static float forceDamping = 0.4f;
        private static float maxHeightDiff = 15f;
        private static float forceCoefficientAtMaxHeightDiff = 2f;

        private static float selfUpSpeed = 3f;

        public static GameObject healBeamPrefab;
        public static GameObject healBeamPrefabBlue;

        private SphereSearch sphereSearch;
        private List<HurtBox> hits;
        private List<BeamController> healBeams;

        private bool isBlue;
        private float duration;
        private float originalMoveSpeed;
        private float healRate;
        private float damageRate;
        private bool shouldFlyUp;
        private Transform muzzle;

        private bool resolvedAnyBeam = false;
        public override void OnEnter()
        {
            base.OnEnter();

            PlayCrossfade("Body", "IdleBuff", 0.3f);
            Util.PlaySound("FollowerCast", gameObject);

            originalMoveSpeed = characterBody.moveSpeed;

            isBlue = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken == "SS2_SKIN_LAMP_BLUE";


            duration = baseDuration / attackSpeedStat;
            healRate = totalHealCoefficient * damageStat / duration;
            damageRate = totalDamageCoefficient * damageStat / duration;

            muzzle = FindModelChild("Muzzle");
            if (NetworkServer.active)
            {
                hits = new List<HurtBox>();

                // search for enemies (players) in a smaller radius first
                SphereSearch enemySearch = new SphereSearch();
                enemySearch.mask = LayerIndex.entityPrecise.mask;
                enemySearch.radius = enemyPrioritySearchRadius;
                enemySearch.origin = characterBody.corePosition;
                enemySearch.RefreshCandidates();
                enemySearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(teamComponent.teamIndex));
                enemySearch.FilterCandidatesByDistinctHurtBoxEntities();
                enemySearch.OrderCandidatesByDistance();
                enemySearch.GetHurtBoxes(hits);

                if (hits.Count == 0)
                {
                    TeamMask mask = TeamMask.allButNeutral;
                    sphereSearch = new SphereSearch();
                    sphereSearch.mask = LayerIndex.entityPrecise.mask;
                    sphereSearch.radius = searchRadius;
                    sphereSearch.origin = characterBody.corePosition;
                    sphereSearch.RefreshCandidates();
                    sphereSearch.FilterCandidatesByHurtBoxTeam(mask);
                    sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                    sphereSearch.OrderCandidatesByDistance();
                    sphereSearch.GetHurtBoxes(hits);
                }

                healBeams = new List<BeamController>();
                int beamCount = 0;
                foreach (HurtBox h in hits)
                {
                    CharacterBody body = h.healthComponent.body;
                    if (CanHeal(body) && beamCount < maxBeams)
                    {
                        beamCount++;
                        GameObject beam = isBlue ? healBeamPrefabBlue : healBeamPrefab;
                        GameObject beamInstance = Object.Instantiate(beam, muzzle);
                        BeamController beamController = beamInstance.GetComponent<BeamController>();
                        beamController.onTickServer.AddListener(OnTickServer);
                        beamController.tickInterval = tickInterval;
                        beamController.target = h.hurtBoxGroup.mainHurtBox;
                        beamController.ownership.ownerObject = gameObject;
                        healBeams.Add(beamController);
                        NetworkServer.Spawn(gameObject);

                        if (!shouldFlyUp && ShouldDamage(h))
                        {
                            shouldFlyUp = true;
                        }
                    }
                }
            }

            if (isAuthority)
            {
                BeamController.onBeamStartGlobal += ResolveBeam;
            }
        }

        private void ResolveBeam(BeamController beam)
        {
            if (beam.ownership.ownerObject == gameObject)
            {
                resolvedAnyBeam = true;
            }
        }

        private void OnTickServer(BeamController beam)
        {
            HurtBox target = beam.target;
            float tickInterval = beam.tickInterval;

            if (!ShouldDamage(target))
            {
                target.healthComponent.Heal(healRate * tickInterval, default(ProcChainMask));
            }
            else
            {
                DamageInfo damageInfo = new DamageInfo();
                damageInfo.damage = damageRate * tickInterval;
                damageInfo.crit = characterBody.RollCrit();
                damageInfo.attacker = gameObject;
                damageInfo.inflictor = beam.gameObject;
                damageInfo.position = target.transform.position;
                damageInfo.force = Vector3.zero;
                damageInfo.procChainMask = default(ProcChainMask);
                damageInfo.procCoefficient = procCoefficientPerTick;
                damageInfo.damageType = damageType;
                damageInfo.damageColorIndex = DamageColorIndex.Default;
                target.healthComponent.TakeDamage(damageInfo);
                GlobalEventManager.instance.OnHitEnemy(damageInfo, target.healthComponent.gameObject);
                GlobalEventManager.instance.OnHitAll(damageInfo, target.healthComponent.gameObject);
            }
            

            target.healthComponent.body.AddTimedBuff(SS2Content.Buffs.bdLampBuff.buffIndex, buffDuration);
        }

        private bool CanHeal(CharacterBody body)
        {
            return body.master && body.healthComponent.alive && !body.hasCloakBuff && body.bodyIndex != base.characterBody.bodyIndex && body.bodyIndex != SS2.Monsters.LampBoss.BodyIndex && body.bodyIndex != SS2.Monsters.Lamp.BodyIndex; /// characyerbody typefields !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

        private bool ShouldDamage(HurtBox target)
        {
            return FriendlyFireManager.ShouldSeekingProceed(target.healthComponent, teamComponent.teamIndex);
        }

        public override void OnExit()
        {
            PlayCrossfade("Body", "Idle", 0.3f);
            BeamController.onBeamStartGlobal -= ResolveBeam;
            if (NetworkServer.active && healBeams.Count > 0)
            {
                foreach (BeamController beam in healBeams)
                {
                    beam.BreakServer();
                }
            }
            characterBody.moveSpeed = originalMoveSpeed;
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.moveSpeed = originalMoveSpeed * 0.1f; // yuck but theres no wlakspeedcoeinfih for rigidbodymotor and idc

            if (shouldFlyUp)
                rigidbodyMotor.AddDisplacement(Vector3.up * selfUpSpeed * Time.fixedDeltaTime);

            if (NetworkServer.active)
            {
                for (int i = 0; i < healBeams.Count; i++)
                {
                    var beam = healBeams[i];
                    if (beam && beam.target)
                    {
                        Vector3 between = beam.target.transform.position - transform.position;
                        bool hasLoS = !Physics.Linecast(beam.target.transform.position, transform.position, out RaycastHit raycastHit, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
                        if (!hasLoS || between.sqrMagnitude > beamBreakDistance * beamBreakDistance)
                        {
                            beam.BreakServer();
                        }
                        else
                        {
                            if (ShouldDamage(beam.target))
                            {
                                Lifto(beam.target);
                            }
                        }

                    }
                }
            }

            if (isAuthority)
            {
                bool shouldExit = fixedAge >= duration;
                if (resolvedAnyBeam && BeamController.GetBeamCountForOwner(gameObject) == 0)
                {
                    shouldExit = true;
                }
                if (shouldExit)
                {
                    outer.SetNextStateToMain();
                }
                
            }
        }

        // TODO: Add forces if multiple beams are targetting one body, instead of separate damage
        // ALSO todo::::::::::::::::: fizz force flags
        
        protected void Lifto(HurtBox hurtBox)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            if (!hurtBox)
            {
                return;
            }

            HealthComponent healthComponent = hurtBox.healthComponent;
            if (healthComponent && healthComponent.body && hurtBox.transform)
            {
                CharacterMotor characterMotor = healthComponent.body.characterMotor;

                Vector3 between = hurtBox.transform.position - transform.position;  
                float heightDiff = Mathf.Abs(between.y);
                Vector3 forceDirection = new Vector3(0, between.y, 0);
                forceDirection = forceDirection.normalized;

                float forceCoefficient = Mathf.Clamp(heightDiff / maxHeightDiff, 1f, forceCoefficientAtMaxHeightDiff);
                Vector3 forceVector = forceDirection * forceMagnitude * forceCoefficient;

                Vector3 currentVerticalVelocity = Vector3.zero;
                float mass = 0f;
                bool useGravity = false;
                if (characterMotor)
                {
                    useGravity = characterMotor.useGravity;
                    currentVerticalVelocity = Vector3.up * characterMotor.velocity.y;
                    mass = characterMotor.mass;
                }
                else
                {
                    Rigidbody rigidbody = healthComponent.body.rigidbody;
                    useGravity = rigidbody.useGravity;
                    if (rigidbody)
                    {
                        currentVerticalVelocity = Vector3.up * rigidbody.velocity.y;
                        mass = rigidbody.mass;
                    }
                }
                if (useGravity)
                {
                    currentVerticalVelocity.y += Physics.gravity.y * Time.fixedDeltaTime;
                }
                if (ignoreMass)
                {
                    forceVector *= mass;
                }
                healthComponent.TakeDamageForce(forceVector - currentVerticalVelocity * forceDamping * mass * forceCoefficient, true, false);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
