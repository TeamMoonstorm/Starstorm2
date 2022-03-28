using Moonstorm;
using RoR2;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Chirr
{
    public class AreaHeal : BaseSkillState
    {
        [TokenModifier("SS2_CHIRR_HEAL_DESCRIPTION", StatTypes.Percentage, 0)]
        public static float healFraction = .15f;
        [TokenModifier("SS2_CHIRR_HEAL_DESCRIPTION", StatTypes.Percentage, 1)]
        public static float healTimeFraction = .2f;
        public static float baseDuration = 1f;
        public static float baseFireDelay = 0.5f;
        public static float radius = 30f;
        public static float hopVelocity = 10f;

        private float duration;
        private float fireDelay;
        private bool hasFired;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;
            fireDelay = baseFireDelay * duration;
            characterBody.SetAimTimer(2f);

            PlayAnimation("Gesture, Override", "Utility", "Utility.playbackRate", duration);
            PlayAnimation("Gesture, Additive", "Utility", "Utility.playbackRate", duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireDelay && !hasFired)
            {
                ActivateHeal();
                HopIfAirborne();
                hasFired = true;
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private void ActivateHeal()
        {
            GameObject ptr = Object.Instantiate(Resources.Load<GameObject>("prefabs/effects/TPHealNovaEffect"), transform);
            NetworkServer.Spawn(ptr);
            if (NetworkServer.active)
            {
                ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(teamComponent.teamIndex);
                float num = radius * radius;
                Vector3 position = transform.position;
                for (int i = 0; i < teamMembers.Count; i++)
                {
                    if ((teamMembers[i].transform.position - position).sqrMagnitude <= num)
                    {
                        HealthComponent healthComponent = teamMembers[i].GetComponent<HealthComponent>();
                        if (healthComponent)
                        {
                            float num2 = healthComponent.fullHealth * healFraction;
                            if (num2 > 0f)
                            {
                                healthComponent.Heal(num2, default(ProcChainMask), true);
                                if (healthComponent.body)
                                    healthComponent.body.AddTimedBuff(RoR2Content.Buffs.CrocoRegen, 1);
                            }
                        }
                    }
                }
            }
        }

        private void HopIfAirborne()
        {
            if (!characterMotor.isGrounded)
            {
                SmallHop(characterMotor, hopVelocity);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
