using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Nemmando
{
    public class Appear : BaseState
    {
        private static GameObject spawnEffectPrefab = EntityStates.NullifierMonster.SpawnState.spawnEffectPrefab;

        public static float duration = 2.5f; //Please get this on a state machine configuraton thing in the unity project -Anreol
        public static float delayBeforeAimAnimatorWeight;
        private Animator modelAnimator;
        private CharacterModel characterModel;
        public override void OnEnter()
        {
            base.OnEnter();
            //base.characterBody.enabled = true;
            //Lets undo what we did over at Spawn state
            this.characterModel = null;
            if (base.characterBody.modelLocator && base.characterBody.modelLocator.modelTransform)
            {
                this.characterModel = base.characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
            }
            if (characterModel)
            {
                characterModel.invisibilityCount--;
            }
            modelAnimator = GetModelAnimator();
            if (this.modelAnimator)
            {
                this.modelAnimator.SetFloat(AnimationParameters.aimWeight, 0f);
            }
            base.PlayAnimation("FullBody, Override", "Spawn", "Spawn.playbackRate", Appear.duration);
            if (Appear.spawnEffectPrefab)
            {
                Util.PlaySound(EntityStates.NullifierMonster.SpawnState.spawnSoundString, gameObject);
                EffectManager.SimpleMuzzleFlash(Appear.spawnEffectPrefab, base.gameObject, "PortalSpawn", false);
            }

            /*ansform modelTransform = base.GetModelTransform();
			if (modelTransform)
			{
				modelTransform.GetComponent<PrintController>().enabled = true;
			}*/

            if (NetworkServer.active) characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, duration * 1.2f);
        }
        public override void Update()
        {
            base.Update();
            if (this.modelAnimator)
            {
                this.modelAnimator.SetFloat(AnimationParameters.aimWeight, Mathf.Clamp01((base.age - Appear.delayBeforeAimAnimatorWeight) / Appear.duration));
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= Appear.duration)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            if (this.modelAnimator)
            {
                this.modelAnimator.SetFloat(AnimationParameters.aimWeight, 1f);
            }
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority() //Shouldnt ever die but just in case lol
        {
            return InterruptPriority.Death;
        }
    }
}