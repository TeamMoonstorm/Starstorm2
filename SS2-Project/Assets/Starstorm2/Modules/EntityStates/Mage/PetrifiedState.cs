using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using SS2;

namespace EntityStates
{
    public class PetrifiedState : BaseState //, SetStateOnHurt.ICustomState
    {
        public static GameObject enterEffectPrefab;
        public static GameObject exitEffectPrefab;
        public static GameObject modelEffectPrefab;
        public static Material overlayMaterial;
        private static float freezeDuration = 5f;
        private static string enterSoundString = "";
        private static string exitSoundString = "";

        private static float bodyRadiusMin = 0.73f;
        private static float bodyRadiusMax = 4.5f;
        private static float particleSizeMin = 1f;
        private static float particleSizeMax = 3f;

        private Animator modelAnimator;
        private TemporaryOverlayInstance temporaryOverlay;
        private List<GameObject> rendererEffectInstances;
        private static bool mainRendererOnly = true;
        public bool reEnteredState = false;
        public override void OnEnter()
        {
            base.OnEnter();

            rendererEffectInstances = new List<GameObject>();

            if (sfxLocator && sfxLocator.barkSound != "")
            {
                Util.PlaySound(sfxLocator.barkSound, gameObject);
            }
            Util.PlaySound(enterSoundString, gameObject);
            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                CharacterModel component = modelTransform.GetComponent<CharacterModel>();
                if (component)
                {
                    temporaryOverlay = TemporaryOverlayManager.AddOverlay(gameObject);
                    temporaryOverlay.duration = freezeDuration;
                    temporaryOverlay.originalMaterial = overlayMaterial;
                    temporaryOverlay.AddToCharacterModel(component);
                }
            }

            SpawnRendererEffects();
            if (enterEffectPrefab)
            {
                EffectManager.SpawnEffect(enterEffectPrefab, new EffectData
                {
                    origin = characterBody.corePosition,
                    scale = (characterBody ? characterBody.radius : 1f)
                }, false);
            }

            modelAnimator = GetModelAnimator();
            if (modelAnimator)
            {
                modelAnimator.enabled = false;
            }

            if (rigidbody && !rigidbody.isKinematic)
            {
                rigidbody.velocity = Vector3.zero;
                if (rigidbodyMotor)
                {
                    rigidbodyMotor.moveVector = Vector3.zero;
                }
            }
            if (characterDirection)
            {
                characterDirection.moveVector = characterDirection.forward;
            }

            if (NetworkServer.active)
            {
                characterBody.AddBuff(SS2Content.Buffs.bdPetrified);
            }
        }

        private void SpawnRendererEffects()
        {
            if (!modelEffectPrefab || !characterBody)
            {
                return;
            }
            var modelTransform = GetModelTransform();
            if (modelTransform && modelTransform.TryGetComponent(out CharacterModel model))
            {
                var rendererInfos = model.baseRendererInfos;
                if (mainRendererOnly && rendererInfos.Length > 0)
                {
                    var rendererInfo = rendererInfos[0];
                    if (!rendererInfo.ignoreOverlays)
                    {
                        AddParticlesToRenderer(rendererInfo.renderer, characterBody.coreTransform, characterBody.radius);
                    }
                }

                if (!mainRendererOnly)
                {
                    for (int i = 0; i < rendererInfos.Length; i++)
                    {
                        var rendererInfo = rendererInfos[i];
                        if (!rendererInfo.ignoreOverlays)
                        {
                            AddParticlesToRenderer(rendererInfo.renderer, characterBody.coreTransform, characterBody.radius);
                        }
                    }
                }
                
            }
        }
        private void AddParticlesToRenderer(Renderer renderer, Transform parent, float bodyRadius)
        {
            if (renderer && parent)
            {
                GameObject effectInstance = GameObject.Instantiate(modelEffectPrefab, parent);
                Vector3 scale = Vector3.one * Util.Remap(bodyRadius, bodyRadiusMin, bodyRadiusMax, particleSizeMin, particleSizeMax);
                ParticleSystem[] particleSystems = effectInstance.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem particleSystem in particleSystems)
                {
                    particleSystem.transform.localScale = scale;
                    ParticleSystem.ShapeModule shape = particleSystem.shape;
                    MeshRenderer meshRenderer = renderer as MeshRenderer;
                    if (meshRenderer)
                    {
                        shape.shapeType = ParticleSystemShapeType.MeshRenderer;
                        shape.meshRenderer = meshRenderer;
                    }
                    else
                    {
                        SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
                        if (skinnedMeshRenderer)
                        {
                            shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                            shape.skinnedMeshRenderer = skinnedMeshRenderer;
                        }
                    }
                }
                rendererEffectInstances.Add(effectInstance);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && fixedAge > freezeDuration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            for (int i = 0; i < rendererEffectInstances.Count; i++)
            {
                if (rendererEffectInstances[i])
                {
                    Destroy(rendererEffectInstances[i]);
                }
            }
            if (temporaryOverlay != null)
            {
                temporaryOverlay.Destroy();
            }
            if (modelAnimator)
            {
                modelAnimator.enabled = true;
            }

            if (exitEffectPrefab)
            {
                EffectManager.SpawnEffect(exitEffectPrefab, new EffectData
                {
                    origin = characterBody.corePosition,
                    scale = (characterBody ? characterBody.radius : 1f)
                }, false);
            }
            Util.PlaySound(exitSoundString, gameObject);

            if (NetworkServer.active)
            {
                if (characterBody.HasBuff(SS2Content.Buffs.bdPetrified))
                    characterBody.RemoveBuff(SS2Content.Buffs.bdPetrified);
            }

            base.OnExit();
        }
        
    }
}
