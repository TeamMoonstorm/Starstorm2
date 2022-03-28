using RoR2;
using System;
using UnityEngine;

namespace EntityStates.Chirr
{
    public class RootState : BaseState
    {
        public float timeRemaining
        {
            get { return Math.Max(duration - base.fixedAge, 0f); }
        }

        public static GameObject rootVfxPrefab;
        public float rootDuration = 4f;

        private float duration;
        private GameObject rootVfxInstance;

        public void ExtendRoot(float addDuration)
        {
            this.duration += addDuration;
            PlayRootAnimation();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (base.sfxLocator && base.sfxLocator.barkSound != "")
            {
                Util.PlaySound(base.sfxLocator.barkSound, base.gameObject);
            }
            PlayRootAnimation();
            if (base.characterBody) base.characterBody.isSprinting = false;
            if (base.characterDirection) base.characterDirection.moveVector = base.characterDirection.forward;
            if (base.rigidbodyMotor) base.rigidbodyMotor.moveVector = Vector3.zero;
        }

        private void PlayRootAnimation()
        {
            Animator modelAnimator = base.GetModelAnimator();
            if (modelAnimator)
            {
                int layerIndex = modelAnimator.GetLayerIndex("Body");
                /*modelAnimator.CrossFadeInFixedTime((UnityEngine.Random.Range(0, 2) == 0) ? "Hurt1" : "Hurt2", 0.1f);
                modelAnimator.Update(0f);*/
                AnimatorStateInfo nextAnimatorStateInfo = modelAnimator.GetNextAnimatorStateInfo(layerIndex);
                duration = Mathf.Max(rootDuration, nextAnimatorStateInfo.length);
                if (rootDuration >= 0f)
                {
                    rootVfxInstance = UnityEngine.Object.Instantiate<GameObject>(RootState.rootVfxPrefab, base.transform);
                    rootVfxInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && isAuthority)
            {

            }
        }

        public override void OnExit()
        {
            if (rootVfxInstance) EntityState.Destroy(rootVfxInstance);
            base.OnExit();
        }
    }
}
