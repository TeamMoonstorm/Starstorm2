using EntityStates;
using Moonstorm.Starstorm2.Components;
using RoR2;
using RoR2.Skills;
using System;
using System.Reflection;
using UnityEngine;

namespace Moonstorm.Starstorm2.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NemesisSpawnCard", menuName = "Starstorm2/NemesisSpawnCard")]
    public class NemesisSpawnCard : SpawnCard
    {
        public NemesisInventory nemesisInventory;
        public bool useOverrideState;
        public SerializableEntityStateType overrideSpawnState = new SerializableEntityStateType(typeof(Idle));
        public StatModifier[] statModifiers;
        public SkillOverride[] skillOverrides;
        public GameObject visualEffect;
        public string childName;
        public ItemDef itemDef;


        public enum StatModifierType
        {
            Multiplicative = 0,
            Additive,
            Override,
        }

        public override void Spawn(Vector3 position, Quaternion rotation, DirectorSpawnRequest directorSpawnRequest, ref SpawnResult result)
        {
            MasterSummon masterSummon = new MasterSummon
            {
                masterPrefab = prefab,
                position = position,
                rotation = rotation,
                summonerBodyObject = directorSpawnRequest.summonerBodyObject,
                teamIndexOverride = directorSpawnRequest.teamIndexOverride,
                ignoreTeamMemberLimit = directorSpawnRequest.ignoreTeamMemberLimit,
                useAmbientLevel = new bool?(true)
            };
            CharacterMaster characterMaster = masterSummon.Perform();
            result.spawnedInstance = ((characterMaster != null) ? characterMaster.gameObject : null);
            result.success = result.spawnedInstance;


            if (result.success)
            {
                var master = result.spawnedInstance.GetComponent<CharacterMaster>();
                var body = master.GetBody();

                nemesisInventory.GiveItems(master.inventory);
                master.inventory.GiveItem(SS2Content.Items.NemBossHelper);

                var component = body.gameObject.AddComponent<NemesisItemDrop>();
                component.itemDef = itemDef;

                ModifyCharacterBody(body);

                var skillLocator = body.skillLocator;
                foreach (var skillOverride in skillOverrides)
                    skillLocator.GetSkill(skillOverride.skillSlot).SetSkillOverride(this, skillOverride.skillDef, GenericSkill.SkillOverridePriority.Replacement);

                ChildLocator childLocator = body.modelLocator.modelTransform.GetComponent<ChildLocator>();
                var effect = GameObject.Instantiate(visualEffect, body.corePosition, Quaternion.identity, childLocator.FindChild(childName));

                master.onBodyDeath.AddListener(RemoveEffect);
                void RemoveEffect() => Destroy(effect);


                if (useOverrideState)
                {
                    EntityStateMachine stateMachine;
                    stateMachine = EntityStateMachine.FindByCustomName(body.gameObject, "Body");
                    if (!stateMachine)
                    {
                        Debug.LogError($"Error: override Entity State on Nemesis Spawn Card {this} must play on a Body State Machine. Skipping override.");
                        return;
                    }
                    EntityState state = (EntityState)Activator.CreateInstance(overrideSpawnState.stateType);
                    stateMachine.SetNextState(state);
                }
            }
        }
        private void ModifyCharacterBody(CharacterBody characterBody)
        {
            characterBody.isChampion = true;
            //★ This is objectively worse and disrespectful to whoever wrote the original code below.          
            foreach (var statModifier in statModifiers)                                                         //TO-DO: Network the below. Proper.
            {                                                                                                   //...This is in reference to the Nemesis Helper Item, by the way. Not the code directly below. This code below is fine.
                if (GetField(statModifier.fieldName, out var field))
                {
                    float value = (float)field.GetValue(characterBody);
                    /*switch (statModifier.statModifierType)
                    {
                        case StatModifierType.Additive:
                            value += statModifier.modifier;
                            break;
                        case StatModifierType.Multiplicative:
                            value *= statModifier.modifier;
                            break;
                        case StatModifierType.Override:
                            value = statModifier.modifier;
                            break;
                    }*/
                    field.SetValue(characterBody, value);
                }
                characterBody.PerformAutoCalculateLevelStats();
            }
        }

        private bool GetField(string fieldName, out FieldInfo field)
        {
            field = typeof(CharacterBody).GetField(fieldName);
            if (field == null)
            {
                Debug.LogError($"Error: {fieldName} in Nemesis Spawn Card {name} is not a valid field for a CharacterBody");
                return false;
            }
            return true;
        }

        [Serializable]
        public struct SkillOverride
        {
            public SkillSlot skillSlot;
            public SkillDef skillDef;

            public SkillOverride(SkillSlot skillSlot, SkillDef skillDef)
            {
                this.skillSlot = skillSlot;
                this.skillDef = skillDef;
            }
        }

        [Serializable]
        public struct StatModifier
        {
            public string fieldName;
            public StatModifierType statModifierType;
            public float modifier;

            public StatModifier(StatModifierType type, float modifier, string fieldName)
            {
                statModifierType = type;
                this.modifier = modifier;
                this.fieldName = fieldName;
            }
        }
    }
}
