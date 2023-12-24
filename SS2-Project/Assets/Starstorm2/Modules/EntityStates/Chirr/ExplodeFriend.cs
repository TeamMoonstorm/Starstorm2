using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moonstorm.Starstorm2.Components;
using UnityEngine;
using System;
using RoR2;
using UnityEngine.Networking;
using Moonstorm.Starstorm2;

namespace EntityStates.Chirr
{
    public class ExplodeFriend : BaseSkillState
    {
        public static float baseDuration = 0.5f;
        public static float minDamageCoef = 4f;
        public static float maxDamageCoef = 30f;
        public static float minRange = 2f;
        public static float maxRange = 13f;
        public static float procCoefficient;

        private ChirrFriendController controller;
        private ChirrFriendTracker tracker;
        private CharacterMaster friend;
        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            tracker = GetComponent<ChirrFriendTracker>();
            controller = characterBody.master.GetComponent<ChirrFriendController>();

            if (tracker && isAuthority)
            {
                friend = tracker.friendOwnership.currentFriend.master;
            }

            if (friend != null)
                Detonate();

            controller.RemoveFriend(friend);

            StartAimMode();

            //sounds, anim, etc.
            //maybe shoot an orb and explode friend on receive?
            //just need to prove this is more fun / healthy way to handle befriend. polish later.

            outer.SetNextStateToMain();
            //that's the entire state. we're skipping the duration for this PoC even.
        }

        public void Detonate()
        {
            float damage = Util.Remap(friend.bodyInstanceObject.GetComponent<CharacterBody>().GetBuffCount(SS2Content.Buffs.BuffChirrWither), 0f, 20f, minDamageCoef, maxDamageCoef);
            float range = Util.Remap(friend.bodyInstanceObject.GetComponent<CharacterBody>().GetBuffCount(SS2Content.Buffs.BuffChirrWither), 0f, 20f, minRange, maxRange);
            bool crit = RollCrit();

            Debug.Log("IM FUCKING BLASTING :: " + damage + "d :: " + range + "r");

            BlastAttack blast = new BlastAttack()
            {
                radius = range,
                procCoefficient = procCoefficient,
                position = friend.transform.position,
                attacker = gameObject,
                teamIndex = teamComponent.teamIndex,
                crit = crit,
                baseDamage = damage * characterBody.damage,
                damageColorIndex = DamageColorIndex.Poison,
                falloffModel = BlastAttack.FalloffModel.None,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                damageType = DamageType.Stun1s
            };
            blast.Fire();

            DamageInfo damageInfo = new DamageInfo()
            {
                damage = damage * characterBody.damage,
                crit = crit,
                inflictor = gameObject,
                attacker = gameObject,
                position = friend.transform.position,
                force = Vector3.zero,
                procCoefficient = procCoefficient,
                damageColorIndex = DamageColorIndex.Poison,
                damageType = DamageType.Stun1s
            };

            friend.bodyInstanceObject.GetComponent<HealthComponent>().TakeDamage(damageInfo);
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
