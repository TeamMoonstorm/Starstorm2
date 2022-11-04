using UnityEngine;
using R2API;
using RoR2;
using UnityEngine.AddressableAssets;
using Moonstorm.Starstorm2;

namespace EntityStates.Wayfarer
{
    public class CreateBuffWard : BaseSkillState
    {
        private static float baseDuration = 1.5f;

        private Animator animator;
        private static GameObject buffPrefab;

        private static GameObject attachment;
        //private static NetworkedBodyAttachment attachment;
        private static bool hasPlacedWard = false;

        private GameObject attachmentGameObject;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            duration = baseDuration / attackSpeedStat;
            PlayCrossfade("FullBody, Override", "Ward", "Ward.playbackRate", baseDuration, 0.2f);

        }
         
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!buffPrefab && fixedAge >= baseDuration * 0.2f)
            {
                buffPrefab = Object.Instantiate(SS2Assets.LoadAsset<GameObject>("WayfarerWard"), characterBody.transform);
                buffPrefab.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                buffPrefab.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(characterBody.gameObject);
                //hasPlacedWard = true;
                Debug.Log("placing attachment!"); // THIS IS WHERE YOU LEFT OFF!!!
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
                //Object.Destroy(buffPrefab);
                //Object.Destroy(attachmentGameObject);
                //attachmentGameObject = null;
                //attachment = null;
            }
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
