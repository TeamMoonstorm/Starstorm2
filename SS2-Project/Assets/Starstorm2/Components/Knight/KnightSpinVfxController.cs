using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class KnightSpinVfxController : MonoBehaviour
    {
        public enum SpinEffectState
        {
            Null,
            Start,
            Spin,
            Finisher
        }

        public SpinEffectState currentSES = SpinEffectState.Null;
        [SerializeField]
        private ChildLocator childLocator;
        [SerializeField]
        private string muzzleName;
        [SerializeField]
        private float intervalUntilPlaySpin, intervalUntilPlaySpinFinisher;
        private bool super;
        private float duration;

        private GameObject vfxInstancedPrefab;
        private EffectManagerHelper vfxInstanceHelper;
        private ScaleParticleSystemDuration vfxParticleScaler;



        public void StartVfxSequence(bool super, float duration)
        {
            this.super = super;
            this.duration = duration;
            StartCoroutine(VfxSequence());
        }

        public void ClearState()
        {
            currentSES = SpinEffectState.Null;
            super = false;
            duration = 0f;
        }

        private void PlayEffect(GameObject vfxPrefab, float duration)
        {
            Transform transform = childLocator.FindChild(childLocator.FindChildIndex(muzzleName));
            if (!EffectManager.ShouldUsePooledEffect(vfxPrefab))
            {
                vfxInstancedPrefab = UnityEngine.Object.Instantiate<GameObject>(vfxPrefab, transform);
            }
            else
            {
                vfxInstanceHelper = EffectManager.GetAndActivatePooledEffect(vfxPrefab, transform, true);
                vfxInstancedPrefab = vfxInstanceHelper.gameObject;
            }

            vfxParticleScaler = vfxInstancedPrefab.GetComponent<ScaleParticleSystemDuration>();
            if (vfxParticleScaler)
            {
                vfxParticleScaler.newDuration = duration;
            }
        }

        /*this doesnt work*/
        IEnumerator VfxSequence()
        {
            currentSES = SpinEffectState.Start;
            //put starting effect here
            yield return new WaitForSeconds(intervalUntilPlaySpin * duration);

            currentSES = SpinEffectState.Spin;
            PlayEffect(super ? SS2.Survivors.Knight.KnightSpinEffect : SS2.Survivors.Knight.KnightSpinEffect, intervalUntilPlaySpinFinisher * duration);
            yield return new WaitForSeconds(intervalUntilPlaySpinFinisher * duration);

            currentSES = SpinEffectState.Finisher;
            //put ending effect here
            yield return null;
        }

        protected virtual void PlaySwingEffect() { return; }
    }
}
