using RoR2;
using System.Linq;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(CharacterModel))]
    public class ExecutionerEmissionController : MonoBehaviour
    {
        private const float MaxStocksForEmission = 10;
        private const float MaxEmission = 8f;
        private const float MinEmission = 2f;

        private CharacterModel characterModel;
        private SkillLocator skillLocator;
        private float currentEmission = MinEmission;
        private float emissionPerStock;

        static ExecutionerEmissionController()
        {
            SceneCamera.onSceneCameraPreRender += OnCameraPrerender;
        }

        private void OnEnable()
        {
            InstanceTracker.Add(this);
        }
        private void OnDisable()
        {
            InstanceTracker.Remove(this);
        }

        void Awake()
        {
            characterModel = GetComponent<CharacterModel>();
            emissionPerStock = (MaxEmission - MinEmission) / MaxStocksForEmission;
        }

        private void Start()
        {
            skillLocator = characterModel.body.skillLocator;
        }

        private static void OnCameraPrerender(SceneCamera sceneCamera)
        {
            foreach (var executionerEmissionController in InstanceTracker.GetInstancesList<ExecutionerEmissionController>())
                executionerEmissionController.UpdateExecutionerMaterials();
        }


        void UpdateExecutionerMaterials()
        {
            GenericSkill secondary = skillLocator.secondary;

            float currentStocks = 0f;
            float emissionChangeSpeed = emissionPerStock * 50f;
            //We change the emission based on what percentage of his max stocks he has, including his cooldown. This gets us a very smooth increase in his emission.
            if (secondary)
            {
                currentStocks = 1f - secondary.cooldownRemaining / (secondary.baseRechargeInterval - secondary.flatCooldownReduction);
                emissionChangeSpeed /= secondary.cooldownScale;
            }
            var desiredEmission = Util.Remap(secondary.stock + currentStocks, 0, MaxStocksForEmission, MinEmission, MaxEmission);
            currentEmission = Mathf.Lerp(currentEmission, desiredEmission, emissionChangeSpeed * Time.deltaTime);

            if (currentEmission > MaxEmission)
            {
                currentEmission = MaxEmission;
            }

            characterModel.baseRendererInfos.Select(rendererInfo => rendererInfo.renderer)
                .ToList()
                .ForEach(renderer =>
                {
                    renderer.GetPropertyBlock(characterModel.propertyStorage);
                    characterModel.propertyStorage.SetFloat("_EmPower", currentEmission);
                    renderer.SetPropertyBlock(characterModel.propertyStorage);
                });
        }
    }
}