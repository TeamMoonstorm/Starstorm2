using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;

namespace EntityStates.NemHuntress
{
    public class Blink : BaseState
    {
        private Transform modelTransform;
        public static GameObject blinkprefab;
        private CharacterModel characterModel;
        private HurtBoxGroup hurtboxGroup;

        private float stopwatch;
        private Vector3 blinkVector = Vector3.zero;

        [SerializeField]
        public float duration = 0.3f;

        [SerializeField]
        public float speedCoefficient = 25f;

        [SerializeField]
        public string beginSoundString;

        [SerializeField]
        public string endSoundString;

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(beginSoundString, gameObject);
            modelTransform = GetModelTransform();
            if (modelTransform)
            {
                characterModel = modelTransform.GetComponent<CharacterModel>();
                hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
            }
            if (characterModel)
            {
                characterModel.invisibilityCount++;
            }
            if (hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = hurtboxGroup;
                int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
                hurtboxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }
            blinkVector = GetBlinkVector();
            CreateBlinkEffect(Util.GetCorePosition(gameObject));
        }

        protected virtual Vector3 GetBlinkVector()
        {
            return inputBank.aimDirection;
        }

        private void CreateBlinkEffect(Vector3 origin)
        {
            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(blinkVector);
            effectData.origin = origin;
            EffectManager.SpawnEffect(blinkprefab, effectData, false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = Vector3.zero;
                characterMotor.rootMotion += blinkVector * (moveSpeedStat * speedCoefficient * Time.fixedDeltaTime);
            }
            if (stopwatch >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            if (outer.destroying)
            {
                Util.PlaySound(endSoundString, gameObject);
                CreateBlinkEffect(Util.GetCorePosition(gameObject));
                modelTransform = GetModelBaseTransform();

                if (modelTransform)
                {
                    TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay.duration = 0.6f;
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashBright");
                    temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
                    TemporaryOverlay temporaryOverlay2 = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay2.duration = 0.7f;
                    temporaryOverlay2.animateShaderAlpha = true;
                    temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay2.destroyComponentOnEnd = true;
                    temporaryOverlay2.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashExpanded");
                    temporaryOverlay2.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
                }    
            }

            if (characterModel)
            {
                characterModel.invisibilityCount--;
            }

            if (hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = hurtboxGroup;
                int hurtBoxesDeactivatorCounter = hurtboxGroup.hurtBoxesDeactivatorCounter - 1;
                hurtboxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }

            if (characterMotor)
            {
                characterMotor.disableAirControlUntilCollision = false;
            }

            base.OnExit();
        }
    }
}
