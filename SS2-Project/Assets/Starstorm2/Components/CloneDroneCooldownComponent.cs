﻿using UnityEngine;
using RoR2;
using UnityEngine.AddressableAssets;
namespace SS2.Components
{
    public class CloneDroneCooldownComponent : MonoBehaviour
    {
        public Material defaultMat;

        public Material disableMat;

        private ModelLocator modelLocator;
        private GameObject model;
        private ChildLocator childLocator;
        private GameObject glowMesh;
        private MeshRenderer glowMeshRenderer;

        private CharacterModel charModel; 

        private SkillLocator skillLoc;
        private bool hasSkill = true;
        private bool chargeMat;

        private float flickerDur = 4.0f;
        private float betweenFlicker = 0.25f;
        private float flickerStopwatch = 0f;
        private Coroutine flickerCoroutine;

        private AkEvent[] droneAkEvents;

        private void Awake()
        {
            modelLocator = GetComponent<ModelLocator>();
            model = modelLocator.modelTransform.gameObject;

            charModel = model.GetComponent<CharacterModel>();

            skillLoc = GetComponent<SkillLocator>();

            var droneBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/Drone1Body.prefab").WaitForCompletion();

            droneAkEvents = droneBody.GetComponents<AkEvent>();

            foreach (AkEvent akEvent in droneAkEvents)
            {
                var akEventType = akEvent.GetType();
                var newComponent = gameObject.AddComponent(akEventType);

                var fields = akEventType.GetFields();

                foreach (var field in fields)
                {
                    var value = field.GetValue(akEvent);
                    field.SetValue(newComponent, value);
                }
            }
        }

        private void FixedUpdate()
        {
            if (skillLoc.primary.stock < 1)
            {
                if (hasSkill){
                    hasSkill = false;
                }

                if (skillLoc.primary.cooldownRemaining <= flickerDur && skillLoc.primary.cooldownRemaining > 0)
                {
                    flickerStopwatch += Time.fixedDeltaTime;
                    if (flickerStopwatch >= betweenFlicker)
                    {
                        flickerStopwatch = 0f;

                        if (charModel)
                        {
                            if(charModel.baseRendererInfos[2].defaultMaterial == disableMat || charModel.baseRendererInfos[2].defaultMaterial == defaultMat){
                                if (charModel.baseRendererInfos[2].defaultMaterial == disableMat){
                                    charModel.baseRendererInfos[2].defaultMaterial = defaultMat;

                                }else if (charModel.baseRendererInfos[2].defaultMaterial == defaultMat){
                                    charModel.baseRendererInfos[2].defaultMaterial = disableMat;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (!hasSkill)
                {
                    hasSkill = true;
                    if (charModel){
                        charModel.baseRendererInfos[2].defaultMaterial = defaultMat;
                    }
                }
            }

        }
    }
}
