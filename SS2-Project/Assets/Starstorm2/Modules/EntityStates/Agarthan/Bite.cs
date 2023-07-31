using Moonstorm;
using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.DamageTypes;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Agarthan
{
    class Bite : BasicMeleeAttack
    {
        public override void OnEnter()
        {
            base.OnEnter();
            swingEffectPrefab = LemurianMonster.Bite.biteEffectPrefab;
            hitEffectPrefab = LemurianMonster.Bite.hitEffectPrefab;
            PlayCrossfade("Gesture", "Bite", "Bite.playbackRate", duration, 0.05f);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}