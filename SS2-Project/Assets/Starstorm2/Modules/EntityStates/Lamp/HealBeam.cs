﻿using Moonstorm.Starstorm2;
using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Lamp
{
    public class HealBeam : BaseSkillState
    {
        public static float baseDuration;
        private float duration;
        public static float healCoefficient = 5f;
        public static GameObject healBeamPrefab;
        public HurtBox target;
        private HealBeamController healBeamController;
        private float lineWidthRefVelocity;

        private float originalMoveSpeed;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayCrossfade("Body", "IdleBuff", 0.3f);

            originalMoveSpeed = characterBody.moveSpeed;

            duration = baseDuration / attackSpeedStat;
            float healRate = healCoefficient * damageStat / duration;
            Ray aimRay = GetAimRay();
            Transform transform = FindModelChild("Muzzle");
            if (NetworkServer.active)
            {
                BullseyeSearch bullseyeSearch = new BullseyeSearch();
                bullseyeSearch.teamMaskFilter = TeamMask.none;
                if (teamComponent)
                    bullseyeSearch.teamMaskFilter.AddTeam(teamComponent.teamIndex);
                bullseyeSearch.filterByLoS = false;
                bullseyeSearch.maxDistanceFilter = 25f;
                bullseyeSearch.maxAngleFilter = 180f;
                bullseyeSearch.searchOrigin = aimRay.origin;
                bullseyeSearch.searchDirection = aimRay.direction;
                bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
                bullseyeSearch.RefreshCandidates();
                bullseyeSearch.FilterOutGameObject(gameObject);
                target = bullseyeSearch.GetResults().FirstOrDefault();
                if (transform && target)
                {
                    GameObject beamInstance = Object.Instantiate(healBeamPrefab, transform);
                    healBeamController = beamInstance.GetComponent<HealBeamController>();
                    healBeamController.healRate = healRate;
                    healBeamController.target = target;
                    healBeamController.ownership.ownerObject = gameObject;
                    NetworkServer.Spawn(gameObject);
                    target.healthComponent.body.AddTimedBuff(SS2Content.Buffs.bdLampBuff.buffIndex, duration);
                }
                else
                    outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            if (healBeamController)
                healBeamController.BreakServer();
            characterBody.moveSpeed = originalMoveSpeed;
            if (target)
                target.healthComponent.body.RemoveBuff(SS2Content.Buffs.bdLampBuff.buffIndex);
            base.OnExit();
            PlayCrossfade("Body", "Idle", 0.3f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.moveSpeed = originalMoveSpeed * 0.1f;
            if ((fixedAge >= duration && isAuthority) || (fixedAge >= duration * 0.05f && target.healthComponent.alive == false))
                outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            HurtBoxReference.FromHurtBox(target).Write(writer);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            HurtBoxReference hurtBoxReference = default(HurtBoxReference);
            hurtBoxReference.Read(reader);
            GameObject gameObject = hurtBoxReference.ResolveGameObject();
            target = ((gameObject != null) ? gameObject.GetComponent<HurtBox>() : null);
        }
    }
}
