using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Duke
{
    public class StunCone : BaseSkillState
    {
        private static float baseDuration = 0.8f;
        private static float stunDuration = 2f;
        private static float coneAngle = 60f;
        private static float coneRange = 20f;
        private static string soundString = "Play_loader_m1_swing";

        public static GameObject effectPrefab;

        private float duration;
        private bool hasFired;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            hasFired = false;

            Util.PlayAttackSpeedSound(soundString, gameObject, attackSpeedStat);
            PlayAnimation("Gesture, Override", "StunCone", "StunCone.playbackRate", duration);
            characterBody.SetAimTimer(2f);

            FireCone();
        }

        private void FireCone()
        {
            if (hasFired) return;
            hasFired = true;

            if (effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(effectPrefab, gameObject, "Muzzle", false);
            }

            if (!NetworkServer.active) return;

            Ray aimRay = GetAimRay();

            var search = new BullseyeSearch();
            search.teamMaskFilter = TeamMask.GetEnemyTeams(GetTeam());
            search.filterByLoS = true;
            search.maxDistanceFilter = coneRange;
            search.minAngleFilter = 0f;
            search.maxAngleFilter = coneAngle;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.filterByDistinctEntity = true;
            search.searchOrigin = aimRay.origin;
            search.searchDirection = aimRay.direction;
            search.RefreshCandidates();

            foreach (HurtBox hurtBox in search.GetResults())
            {
                if (!hurtBox || !hurtBox.healthComponent || !hurtBox.healthComponent.alive) continue;

                CharacterBody victimBody = hurtBox.healthComponent.body;
                if (!victimBody) continue;

                // Apply Duke stun buff (marker for ricochet)
                victimBody.AddTimedBuff(SS2.SS2Content.Buffs.bdDukeStun, stunDuration);

                // Apply actual stun state via SetStateOnHurt
                if (victimBody.TryGetComponent(out SetStateOnHurt setStateOnHurt))
                {
                    if (setStateOnHurt.canBeStunned)
                    {
                        setStateOnHurt.SetStun(stunDuration);
                    }
                }

                Debug.Log("[Duke] StunCone: Stunned " + victimBody.name + " for " + stunDuration + "s.");
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
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
