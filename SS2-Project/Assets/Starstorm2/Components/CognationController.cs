using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    public class CognationController : MonoBehaviour
    {
        private Material ghostMaterial = SS2Assets.LoadAsset<Material>("matCognation", SS2Bundle.Artifacts);

        private CharacterBody originalBody;
        private CharacterMaster originalMaster;
        private DeathRewards originalRewards;

        private CharacterBody thisBody;
        private CharacterMaster thisMaster;
        private CharacterModel thisModel;
        private DeathRewards thisRewards;

        internal void SetOriginals(CharacterBody origBody, CharacterMaster origMaster, DeathRewards origRewards)
        {
            originalBody = origBody;
            originalMaster = origMaster;
            originalRewards = origRewards;
        }

        private void Start()
        {
            if (!PopulateFields())
            {
                SS2Log.Error($"Could not find one of the fields for cognation ghost.");
                Destroy(this);
                return;
            }

            ModifyStats();
            AddRewards();
            ModifyCharacterModel();
        }

        private bool PopulateFields()
        {
            thisBody = GetComponent<CharacterBody>();
            if (thisBody)
                thisMaster = thisBody.master;
            thisRewards = GetComponent<DeathRewards>();

            ModelLocator modelLocator = thisBody.GetComponent<ModelLocator>();
            if ((bool)modelLocator)
            {
                Transform modelTransform = modelLocator.modelTransform;
                if ((bool)modelTransform)
                {
                    thisModel = modelTransform.GetComponent<CharacterModel>();
                }
            }

            return (thisBody && thisMaster && thisRewards && thisModel);
        }

        private void ModifyStats()
        {
            thisBody.baseMaxHealth *= 3;
            thisBody.baseMoveSpeed *= 1.25f;
            thisBody.baseAttackSpeed *= 1.25f;
            thisBody.baseDamage *= 0.9f;
            thisBody.baseArmor -= 25;
            thisBody.PerformAutoCalculateLevelStats();
            thisBody.RecalculateStats();
        }
        private void AddRewards()
        {
            thisRewards.goldReward = (uint)(originalRewards.goldReward / 1.5f);
            thisRewards.expReward = (uint)(originalRewards.expReward / 1.5f);
        }
        private void ModifyCharacterModel()
        {
            for (int i = 0; i < thisModel.baseRendererInfos.Length; i++)
            {
                var currentMaterial = thisModel.baseRendererInfos[i].defaultMaterial;
                if (currentMaterial.shader.name.StartsWith("Hopoo Games/Deferred"))
                {
                    currentMaterial = ghostMaterial;
                    thisModel.baseRendererInfos[i].defaultMaterial = currentMaterial;
                }
            }
        }

        internal void SetModelDestroyOnUnseen()
        {
            var modelDestroyOnUnseen = thisModel.GetComponent<DestroyOnUnseen>();
            if (!modelDestroyOnUnseen)
                modelDestroyOnUnseen = thisModel.gameObject.AddComponent<DestroyOnUnseen>();

            modelDestroyOnUnseen.cull = true;
        }
    }
}
