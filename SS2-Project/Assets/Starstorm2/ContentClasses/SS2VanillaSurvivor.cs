using EntityStates.Engi;
using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using SS2;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SS2
{
    public abstract class SS2VanillaSurvivor : IVanillaSurvivorContentPiece, IContentPackModifier
    {
        public VanillaSurvivorAssetCollection assetCollection { get; private set; }
        public SurvivorDef survivorDef { get; set; }
        public abstract SS2AssetRequest<VanillaSurvivorAssetCollection> assetRequest { get; }

        public abstract void Initialize();

        public virtual IEnumerator InitializeAsync()
        {
            ParallelCoroutine helper = new ParallelCoroutine();
            foreach(var uberSkinDef in assetCollection.FindAssets<UberSkinDef>())
            {
                helper.Add(uberSkinDef.PreBake());
                AddToModelSkinControllers(uberSkinDef.targetSkinDef);
            }

            while(!helper.IsDone())
            {
                yield return null;
            }
        }

        private void AddToModelSkinControllers(SkinDef skinDef)
        {
            var bodyPrefab = survivorDef.bodyPrefab;
            var displayPrefab = survivorDef.displayPrefab;

            var bodySkinController = bodyPrefab.GetComponentInChildren<ModelSkinController>();
            var displaySkinController = displayPrefab.GetComponentInChildren<ModelSkinController>();

            if(bodySkinController)
            {
                HG.ArrayUtils.ArrayAppend(ref bodySkinController.skins, skinDef);
            }
            if(displaySkinController)
            {
                HG.ArrayUtils.ArrayAppend(ref displaySkinController.skins, skinDef);
            }
        }

        public abstract bool IsAvailable(ContentPack contentPack);

        public IEnumerator LoadContentAsync()
        {
            var assetRequest = this.assetRequest;

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            assetCollection = assetRequest.Asset;

            var request = Addressables.LoadAssetAsync<SurvivorDef>(assetCollection.survivorDefAddress);
            while (!request.IsDone)
                yield return null;

            survivorDef = request.Result;
        }

        public abstract void ModifyContentPack(ContentPack contentPack);

        public void AddSkill(SkillFamily skillFamily, SkillDef skillDef)
        {
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }

        public static void AddEntityStateMachine(GameObject prefab, string machineName, Type mainStateType = null, Type initalStateType = null)
        {
            EntityStateMachine entityStateMachine = EntityStateMachine.FindByCustomName(prefab, machineName);
            if (entityStateMachine == null)
            {
                entityStateMachine = prefab.AddComponent<EntityStateMachine>();
            }
            else
            {
                Debug.Log($"An Entity State Machine already exists with the name {machineName}. replacing.");
            }

            entityStateMachine.customName = machineName;

            if (mainStateType == null)
            {
                mainStateType = typeof(EntityStates.Idle);
            }
            entityStateMachine.mainStateType = new EntityStates.SerializableEntityStateType(mainStateType);

            if (initalStateType == null)
            {
                initalStateType = typeof(EntityStates.Idle);
            }
            entityStateMachine.initialStateType = new EntityStates.SerializableEntityStateType(initalStateType);

            NetworkStateMachine networkMachine = prefab.GetComponent<NetworkStateMachine>();
            if (networkMachine)
            {
                networkMachine.stateMachines = networkMachine.stateMachines.Append(entityStateMachine).ToArray();
            }

            CharacterDeathBehavior deathBehavior = prefab.GetComponent<CharacterDeathBehavior>();
            if (deathBehavior)
            {
                deathBehavior.idleStateMachine = deathBehavior.idleStateMachine.Append(entityStateMachine).ToArray();
            }

            SetStateOnHurt setStateOnHurt = prefab.GetComponent<SetStateOnHurt>();
            if (setStateOnHurt)
            {
                setStateOnHurt.idleStateMachine = setStateOnHurt.idleStateMachine.Append(entityStateMachine).ToArray();
            }
        }
    }
}
