using EntityStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using RoR2;
using RoR2.Projectile;

namespace Assets.Starstorm2.Modules.EntityStates.Mercenary
{
    public class SovereignSlash : BaseState
    {
        [SerializeField]
        public GameObject beamProjectile;

        public enum SlashComboPermutation
        {
            Slash1,
            Slash2,
            Final
        }

        public static float baseDuration = 3.5f;

        public static float mecanimDurationCoefficient;

        public static float damageCoefficient = 4f;

        public static float forceMagnitude = 16f;

        public static float selfForceMagnitude;

        public static float radius = 3f;

        public static GameObject hitEffectPrefab;

        public static GameObject swingEffectPrefab;

        public static string attackString;

        private OverlapAttack attack;

        private Animator modelAnimator;

        private float duration;

        private bool hasSlashed;

        public SlashComboPermutation slashComboPermutation;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            modelAnimator = GetModelAnimator();
            Transform modelTransform = GetModelTransform();
            attack = new OverlapAttack();
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
            attack.damage = damageCoefficient * damageStat;
            attack.hitEffectPrefab = hitEffectPrefab;
            attack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
            Util.PlaySound(attackString, base.gameObject);
            string hitboxGroupName = "";
            string animationStateName = "";

            switch (slashComboPermutation)
            {
                case SlashComboPermutation.Slash1:
                    hitboxGroupName = "DaggerLeft";
                    animationStateName = "SlashP1";
                    break;
                case SlashComboPermutation.Slash2:
                    hitboxGroupName = "DaggerLeft";
                    animationStateName = "SlashP2";
                    break;
                case SlashComboPermutation.Final:
                    hitboxGroupName = "DaggerLeft";
                    animationStateName = "SlashP3";
                    break;
            }

            if (modelTransform)
            {
                attack.hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == hitboxGroupName);
            }

            if (modelAnimator)
            {
                PlayAnimation("Gesture, Override", animationStateName, "SlashCombo.playbackRate", duration * mecanimDurationCoefficient);
                PlayAnimation("Gesture, Additive", animationStateName, "SlashCombo.playbackRate", duration * mecanimDurationCoefficient);
            }

            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(2f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if ((NetworkServer.active && modelAnimator) && (modelAnimator.GetFloat("SlashCombo.hitBoxActive") > 0.1f))
            {
                if (!hasSlashed)
                {
                    EffectManager.SimpleMuzzleFlash(swingEffectPrefab, base.gameObject, "SwingCenter", transmit: true);

                    ProjectileManager.instance.FireProjectile(
                        beamProjectile,
                        GetAimRay().origin,
                        Util.QuaternionSafeLookRotation(GetAimRay().direction),
                        gameObject,
                        damageStat * damageCoefficient,
                        0f,
                        RollCrit(),
                        DamageColorIndex.Default,
                        null,
                        80f
                    );


                    HealthComponent healthComponent = base.characterBody.healthComponent;
                    CharacterDirection component = ((Component)base.characterBody).GetComponent<CharacterDirection>();
                    if (healthComponent)
                    {
                        healthComponent.TakeDamageForce(selfForceMagnitude * component.forward, alwaysApply: true);
                    }
                    hasSlashed = true;
                }
                attack.forceVector = base.transform.forward * forceMagnitude;
                attack.Fire();
            }
            if (!(base.fixedAge >= duration) || !base.isAuthority)
            {
                return;
            }
            if (base.inputBank && base.inputBank.skill1.down)
            {
                SovereignSlash slashCombo = new SovereignSlash();
                switch (slashComboPermutation)
                {
                    case SlashComboPermutation.Slash1:
                        slashCombo.slashComboPermutation = SlashComboPermutation.Slash2;
                        break;
                    case SlashComboPermutation.Slash2:
                        slashCombo.slashComboPermutation = SlashComboPermutation.Slash1;
                        break;
                }
                outer.SetNextState(slashCombo);
            }
            else
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
