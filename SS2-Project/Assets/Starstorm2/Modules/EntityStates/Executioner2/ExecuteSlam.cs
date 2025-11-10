using System.Collections.Generic;
using UnityEngine;
using RoR2;
using SS2;
using SS2.Components;
using UnityEngine.Networking;
namespace EntityStates.Executioner2
{
    public class ExecuteSlam : BaseSkillState
    {
        public static float baseDamageCoefficient;
        public static float slamRadius;
        public static float procCoefficient;
        public static float recoil;
        private static float maxDuration = 10f;
        private static float acceleration = 1f;
        private static float walkSpeedCoefficient = 1.5f;
        private static float verticlalSpeed = 100f;
        private static float maxVerticalSpeed = 300f;
        private static float durationForMaxSpeed = 3f;
        public static GameObject slamEffect;
        public static GameObject slamEffectMastery;
        public static GameObject impactEffectPrefab;
        public static GameObject soloImpactEffectPrefab;
        public static GameObject cameraEffectPrefab;
        public static float duration = 1f;
        public Vector3 dashVector = Vector3.zero;
        private bool hasImpacted;
        private ExecutionerController exeController;

        private CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private static float cameraLerpDuration = 0.3f;
        private CharacterCameraParamsData slamCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 88f,
            minPitch = -88f,
            pivotVerticalOffset = 1.37f,
            idealLocalCameraPos = slamCameraPosition,
            wallCushion = 0.1f,
        };
        private static Vector3 slamCameraPosition = new Vector3(0, 0, -9f);

        private Transform collisionTransform;
        public override void OnEnter()
        {
            base.OnEnter();

            exeController = GetComponent<ExecutionerController>();
            if (exeController != null)
            {
                exeController.meshExeAxe.SetActive(true);
            }

            characterMotor.walkSpeedPenaltyCoefficient = walkSpeedCoefficient;
            characterBody.hideCrosshair = true;
            PlayAnimation("FullBody, Override", "SpecialSwing", "Special.playbackRate", duration * 0.8f);

            characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            characterBody.isSprinting = true;

            characterBody.SetAimTimer(duration);

            

            if (isAuthority)
            {
                gameObject.AddComponent<TeleportHandler>().state = this;
                characterMotor.onHitGroundAuthority += OnGroundHit;
                characterMotor.onMovementHit += OnMovementHit;

                collisionTransform = FindModelChild("SlamCollision");

                CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
                {
                    cameraParamsData = slamCameraParams,
                    priority = 1f
                };
                camOverrideHandle = cameraTargetParams.AddParamsOverride(request, cameraLerpDuration);
            }
        }

        private class TeleportHandler : MonoBehaviour, ITeleportHandler
        {
            public ExecuteSlam state;
            public void OnTeleport(Vector3 oldPosition, Vector3 newPosition)
            {
                state.DoImpactAuthority();
                Destroy(this);
            }
        }
        private void OnGroundHit(ref CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            DoImpactAuthority();
        }

        private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            if (isAuthority)
                DoImpactAuthority();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority)
                FixedUpdateAuthority();
        }

        private void FixedUpdateAuthority()
        {
            characterDirection.forward = dashVector;
            HandleMovement();

            bool shouldImpactThisFrame = fixedAge > maxDuration || characterMotor.Motor.GroundingStatus.IsStableOnGround;
            // fuck nonalloc shit makes no sense
            Collider[] hits = Physics.OverlapBox(collisionTransform.position, collisionTransform.lossyScale * 0.5f, collisionTransform.rotation, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i])
                {
                    HurtBox hurtBox = hits[i].GetComponent<HurtBox>();

                    if (hurtBox)
                    {
                        if (hurtBox.healthComponent.gameObject != gameObject)
                        {
                            shouldImpactThisFrame = true;
                            break;
                        }
                    }
                    else
                    {
                        // we hit da world
                        shouldImpactThisFrame = true;
                        break;
                    }
                }
            }
            if (shouldImpactThisFrame)
            {
                DoImpactAuthority();
            }
        }

        private static bool FUCK = true;
        public void HandleMovement()
        {
            float t = Mathf.Clamp01(fixedAge / durationForMaxSpeed);
            float speed = Mathf.Lerp(verticlalSpeed, maxVerticalSpeed, t);
            characterMotor.rootMotion += dashVector * speed * Time.fixedDeltaTime;
            characterMotor.moveDirection = FUCK ? Vector3.zero : inputBank.moveVector;
            characterMotor.velocity = Vector3.zero;

            characterDirection.targetVector = dashVector;
            characterDirection.moveVector = Vector3.zero;
            characterDirection.forward = dashVector;
        }

        private void DoImpactAuthority()
        {
            if (hasImpacted) return;
            hasImpacted = true;
            // should REALLY just do TakeDamage on everything the spheresearch hits

            SphereSearch search = new SphereSearch();
            List<HurtBox> hits = new List<HurtBox>();
            List<HealthComponent> hitTargets = new List<HealthComponent>();
            Vector3 position = characterBody.footPosition;

            float damage = baseDamageCoefficient;
            float procMultiplier = 1;
            bool soloTarget = false;

            search.ClearCandidates();
            search.mask = LayerIndex.entityPrecise.mask;
            search.origin = position;
            search.radius = slamRadius;
            search.RefreshCandidates();
            search.FilterCandidatesByDistinctHurtBoxEntities();
            search.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(teamComponent.teamIndex));
            search.GetHurtBoxes(hits);
            hitTargets.Clear();
            
            foreach (HurtBox h in hits)
            {
                HealthComponent hp = h.healthComponent;
                if (hp && !hitTargets.Contains(hp))
                    hitTargets.Add(hp);
            }
            if (hitTargets.Count == 1)
            {
                damage *= 2f;
                procMultiplier = 2;
                soloTarget = true;
                OnHitSingleTargetAuthority();
            }

            bool crit = RollCrit();
            DamageTypeCombo damageType = DamageType.Generic;
            damageType.damageSource = DamageSource.Special;
            BlastAttack blast = new BlastAttack()
            {
                radius = slamRadius,
                procCoefficient = procCoefficient * procMultiplier,
                position = position,
                attacker = gameObject,
                teamIndex = teamComponent.teamIndex,
                crit = crit,
                baseDamage = characterBody.damage * damage,
                damageColorIndex = DamageColorIndex.Default,
                falloffModel = BlastAttack.FalloffModel.None,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                impactEffect = EffectCatalog.FindEffectIndexFromPrefab(soloTarget ? soloImpactEffectPrefab : impactEffectPrefab),
                damageType = damageType,
            };
            var result = blast.Fire();
            Vector3 hitPosition = result.hitCount > 0 ? result.hitPoints[0].hitPosition : position; // XDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD

            AddRecoil(-0.4f * recoil, -0.8f * recoil, -0.3f * recoil, 0.3f * recoil);

            if (slamEffect)
            {
                Transform muzzle = FindModelChild("AxeSlam");
                if (muzzle)
                {
                    position = muzzle.position;
                }
                if (exeController.inMasterySkin)
                {
                    EffectManager.SimpleEffect(slamEffectMastery, position, Quaternion.identity, true);
                }
                else
                {
                    EffectManager.SimpleEffect(slamEffect, position, Quaternion.identity, true);
                }
            }
            outer.SetNextState(new ExecuteImpact { soloTarget = soloTarget, targetPosition = hitPosition, dashVector = dashVector, hitAnyEnemies = result.hitCount > 0 });
            if (characterMotor)
            {
                characterMotor.velocity = Vector3.zero;
            }
        }

        public virtual void OnHitSingleTargetAuthority()
        {
            GameObject cameraEffect = GameObject.Instantiate(cameraEffectPrefab, characterBody.transform);
            cameraEffect.GetComponent<LocalCameraEffect>().targetCharacter = gameObject;
        }
        

        public override void OnExit()
        {
            base.OnExit();
            characterBody.hideCrosshair = false;
            characterMotor.walkSpeedPenaltyCoefficient = 1f;

            characterBody.bodyFlags -= CharacterBody.BodyFlags.IgnoreFallDamage;

            if(outer.nextState is not ExecuteImpact)
            {
                PlayAnimation("FullBody, Override", "BufferEmpty");
            }

            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 1.2f);
            }
            if (isAuthority)
            {
                characterMotor.onHitGroundAuthority -= OnGroundHit;
                characterMotor.onMovementHit -= OnMovementHit;

                if (gameObject.TryGetComponent(out TeleportHandler tp))
                {
                    Destroy(tp);
                }
            }

            
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }

    public class ExecuteSlamScepter : ExecuteSlam
    {
        private static float cooldownRefreshFraction = 0.5f;
        public override void OnHitSingleTargetAuthority()
        {
            base.OnHitSingleTargetAuthority();

            skillLocator.special?.RunRecharge(skillLocator.special.CalculateFinalRechargeInterval() * cooldownRefreshFraction);
        }
    }
    public class ExecuteImpact : BaseCharacterMain
    {
        private static int chargesToGrant = 3;
        public bool soloTarget;
        public bool hitAnyEnemies;
        public Vector3 targetPosition;
        public Vector3 dashVector;
        private static float baseHitPauseDuration = 2f;
        private static float hitPauseDurationSolo = .45f;
        private static float duration = 0.5f;
        private static float axeFadeOutDuratoin = .9f;
        private static float ionOrbSpeed = 30f;

        private float hitPauseDuration;
        private ExecutionerController exeController;
        private Vector3 direction;
        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(soloTarget);
            writer.Write(targetPosition);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            soloTarget = reader.ReadBoolean();
            targetPosition = reader.ReadVector3();
        }
        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.SetAimTimer(1f);
            PlayAnimation("FullBody, Override", "SpecialImpact");
            hitPauseDuration = soloTarget ? hitPauseDurationSolo : baseHitPauseDuration;

            exeController = GetComponent<ExecutionerController>();
            exeController.AxeFadeOut(axeFadeOutDuratoin);

            if (NetworkServer.active && soloTarget)
            {
                for (int i = 0; i < chargesToGrant; i++)
                {
                    SS2.Orbs.ExecutionerIonOrb ionOrb = new SS2.Orbs.ExecutionerIonOrb();
                    ionOrb.speed = ionOrbSpeed;
                    ionOrb.origin = targetPosition;
                    ionOrb.target = characterBody.mainHurtBox;
                    RoR2.Orbs.OrbManager.instance.AddOrb(ionOrb);
                }
            }

            if (!hitAnyEnemies)
            {
                direction = characterDirection.forward; // wtf is even going on anymore
            }
            else if(soloTarget)
            {
                direction = targetPosition - transform.position;
            }
            else
            {
                direction = dashVector;
            }
            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge >= hitPauseDuration)
            {
                if(characterMotor)
                    characterMotor.moveDirection = inputBank.moveVector;
            }
            else
            {
                if(characterMotor)
                {
                    characterMotor.velocity = Vector3.zero;
                }
            }
            if (characterDirection)
            {
                characterDirection.targetVector = direction; // idk which one makes it not suck
                characterDirection.moveVector = Vector3.zero;
                characterDirection.forward = direction;
            }
                

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (gameObject.TryGetComponent(out ExecutionerController exeController))
            {
                exeController.meshExeAxe.SetActive(false);
            }
        }
    }
}

