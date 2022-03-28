using UnityEngine;

namespace EntityStates.Wayfarer
{
    class CreateBuffWard : BaseSkillState
    {
        public static float baseDuration = 3.5f;
        public static float radius = 30f;

        private Animator animator;
        private GameObject buffPrefab;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            PlayCrossfade("FullBody, Override", "Ward", "Ward.playbackRate", baseDuration / attackSpeedStat, 0.2f);

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!buffPrefab && animator.GetFloat("Ward.active") > 0.5)
            {
                /*buffPrefab = UnityEngine.Object.Instantiate(Monsters.WayfarerOld.wayfarerBuffWardPrefab);
                buffPrefab.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                buffPrefab.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject);*/
            }

            if (fixedAge >= baseDuration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            if (buffPrefab)
            {
                Object.Destroy(buffPrefab);
            }
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
