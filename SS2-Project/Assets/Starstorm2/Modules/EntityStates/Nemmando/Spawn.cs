using Moonstorm.Starstorm2;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Nemmando
{
    public class Spawn : BaseState
    {
        public static float minimumIdleDuration = 3.5f;
        public static GameObject spawnEffectPrefab;
        public static GameObject spawnEffectFakePod;
        private Animator modelAnimator;
        private CharacterModel characterModel;
        private bool hasSpawnedPrefab = false;
        public override void OnEnter()
        {
            base.OnEnter();
            //base.characterBody.enabled = false;
            //We know its nemmando and that he has a model... but lets make it generic
            if (NetworkServer.active) characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, minimumIdleDuration);
            characterModel = null;
            if (characterBody.modelLocator && characterBody.modelLocator.modelTransform)
            {
                characterModel = characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
            }
            if (characterModel)
            {
                characterModel.invisibilityCount++;
            }
            modelAnimator = GetModelAnimator();
            
            /*EffectManager.SpawnEffect(Spawn.spawnEffectPrefab, new EffectData
            {
                origin = base.characterBody.footPosition
            }, false);*/
        }
        public override void OnExit()
        {
            if (modelAnimator)
            {
                modelAnimator.SetFloat(AnimationParameters.aimWeight, 1f);
            }
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= minimumIdleDuration)
            {
                /*if (!hasSpawnedPrefab)
                {
                    Object.Instantiate(SS2Assets.LoadAsset<GameObject>("NemSpawnPrefab", SS2Bundle.NemCommando), characterBody.corePosition, characterBody.transform.rotation);
                    hasSpawnedPrefab = true;
                }
                if (inputBank.interact.down)*/
                outer.SetNextState(new Appear());
            }
        }
    }
}