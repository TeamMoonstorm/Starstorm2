using RoR2;
using UnityEngine;

namespace EntityStates.Nemmando
{
    public class Spawn : BaseState
    {
        public static float minimumIdleDuration = 4f;
        public static GameObject spawnEffectPrefab;
        private Animator modelAnimator;
        private CharacterModel characterModel;
        public override void OnEnter()
        {
            base.OnEnter();
            //base.characterBody.enabled = false;
            //We know its nemmando and that he has a model... but lets make it generic
            this.characterModel = null;
            if (base.characterBody.modelLocator && base.characterBody.modelLocator.modelTransform)
            {
                this.characterModel = base.characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
            }
            if (characterModel)
            {
                characterModel.invisibilityCount++;
            }
            modelAnimator = GetModelAnimator();
            EffectManager.SpawnEffect(Spawn.spawnEffectPrefab, new EffectData
            {
                origin = base.characterBody.footPosition
            }, false);
        }
        public override void OnExit()
        {
            if (this.modelAnimator)
            {
                this.modelAnimator.SetFloat(AnimationParameters.aimWeight, 1f);
            }
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge >= Spawn.minimumIdleDuration && (base.inputBank.moveVector.sqrMagnitude >= Mathf.Epsilon || base.inputBank.CheckAnyButtonDown()))
            {
                this.outer.SetNextState(new Appear());
            }
        }
    }
}