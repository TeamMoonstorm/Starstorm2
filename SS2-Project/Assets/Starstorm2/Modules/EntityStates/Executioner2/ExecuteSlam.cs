using System.Collections.Generic;
using UnityEngine;
using RoR2;
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
        private static float walkSpeedCoefficient = 1f;
        private static float maxAngle = 30f;
        private static float verticlalSpeed = 100f;
        public static GameObject slamEffect;
        public static GameObject slamEffectMastery;
        public static GameObject impactEffectPrefab;
        public static GameObject soloImpactEffectPrefab;
        public static GameObject cameraEffectPrefab;
        public static float duration = 1f;
        private Vector3 dashVector = Vector3.zero;
        private bool hasImpacted;
        private ExecutionerController exeController;

        private CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private CharacterCameraParamsData slamCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 88f,
            minPitch = -88f,
            pivotVerticalOffset = 1.37f,
            idealLocalCameraPos = slamCameraPosition,
            wallCushion = 0.1f,
        };
        private static Vector3 slamCameraPosition = new Vector3(0, 0, -9f);
        

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
            characterMotor.onHitGroundAuthority += OnGroundHit;
            characterMotor.onMovementHit += OnMovementHit;
            characterBody.isSprinting = true;

            characterBody.SetAimTimer(duration);


            if (isAuthority)
            {
                CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
                {
                    cameraParamsData = slamCameraParams,
                    priority = 1f
                };
                camOverrideHandle = cameraTargetParams.AddParamsOverride(request, 0.1f);

                dashVector = Vector3.down;
                dashVector = Vector3.RotateTowards(dashVector, inputBank.aimDirection, maxAngle * Mathf.Deg2Rad, 0f);
            }
        }

        private void OnGroundHit(ref CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            DoImpactAuthority();
        }

        private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            if(isAuthority)
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

            if (fixedAge > maxDuration || characterMotor.Motor.GroundingStatus.IsStableOnGround)
            {
                DoImpactAuthority();
            }
        }

        public void HandleMovement()
        {
            characterMotor.rootMotion += dashVector * verticlalSpeed * Time.fixedDeltaTime;
            characterMotor.moveDirection = inputBank.moveVector;
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
                GameObject cameraEffect = GameObject.Instantiate(cameraEffectPrefab, characterBody.transform);
                cameraEffect.GetComponent<LocalCameraEffect>().targetCharacter = gameObject;
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
                if (exeController.inMasterySkin)
                {
                    EffectManager.SimpleEffect(slamEffectMastery, position, Quaternion.identity, true);
                }
                else
                {
                    EffectManager.SimpleEffect(slamEffect, position, Quaternion.identity, true);
                }
            }
            outer.SetNextState(new ExecuteImpact { giveCharges = soloTarget, targetPosition = hitPosition });
        }
        

        public override void OnExit()
        {
            base.OnExit();
            characterBody.hideCrosshair = false;
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            PlayAnimation("FullBody, Override", "SpecialImpact");
            if (exeController != null)
            {
                exeController.meshExeAxe.SetActive(false);
            }
            characterMotor.onHitGroundAuthority -= OnGroundHit;
            characterBody.bodyFlags -= CharacterBody.BodyFlags.IgnoreFallDamage;
            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 1.2f);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }

    public class ExecuteImpact : BaseSkillState
    {
        public static int chargesToGrant = 3;
        public bool giveCharges;
        public Vector3 targetPosition;

        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(giveCharges);
            writer.Write(targetPosition);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            giveCharges = reader.ReadBoolean();
            targetPosition = reader.ReadVector3();
        }
        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.SetAimTimer(1f);

            if(NetworkServer.active && giveCharges)
            {
                for (int i = 0; i < chargesToGrant; i++)
                {
                    SS2.Orbs.ExecutionerIonOrb ionOrb = new SS2.Orbs.ExecutionerIonOrb();
                    ionOrb.origin = targetPosition;
                    ionOrb.target = characterBody.mainHurtBox;
                    RoR2.Orbs.OrbManager.instance.AddOrb(ionOrb);
                }
            }
            outer.SetNextStateToMain();
        }
    }
}

