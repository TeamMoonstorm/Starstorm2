﻿using EntityStates;
using SS2.Components;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Skills;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2
{
    [CreateAssetMenu(fileName = "NemesisSpawnCard", menuName = "Starstorm2/NemesisSpawnCard")]
    public class NemesisSpawnCard : CharacterSpawnCard // add IsAvailable() here to check for unlocks or whatever
    {

        public NemesisInventory nemesisInventory;
        public bool useOverrideState;
        public SerializableEntityStateType overrideSpawnState = new SerializableEntityStateType(typeof(Idle));
        public StatModifier[] statModifiers;
        public SkillOverride[] skillOverrides;
        public GameObject visualEffect;
        public ItemDef itemDef;


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
                useAmbientLevel = new bool?(true),
            };
            CharacterMaster characterMaster = masterSummon.Perform();
            result.spawnedInstance = ((characterMaster != null) ? characterMaster.gameObject : null);
            result.success = result.spawnedInstance;

            if (result.success)
            {
                var master = result.spawnedInstance.GetComponent<CharacterMaster>();
                var body = master.GetBody();

                nemesisInventory.GiveItems(master.inventory);
                //master.inventory.GiveItem(SS2Content.Items.NemBossHelper);

                var component = body.gameObject.AddComponent<NemesisItemDrop>();
                component.itemDef = itemDef;

                ModifyCharacterBody(body);

                var skillLocator = body.skillLocator;
                foreach (var skillOverride in skillOverrides)
                    skillLocator.GetSkill(skillOverride.skillSlot).SetSkillOverride(this, skillOverride.skillDef, GenericSkill.SkillOverridePriority.Replacement);

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
            foreach (var statModifier in statModifiers)
            {
                if (GetField(statModifier.fieldName, out var field))
                {
                    float value = (float)field.GetValue(characterBody);
                    switch (statModifier.statModifierType)
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
                    }
                    field.SetValue(characterBody, value);
                }
            }
            characterBody.PerformAutoCalculateLevelStats();
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

        private void OnValidate()
        {
            foreach (var statModifier in statModifiers)
            {
                GetField(statModifier.fieldName, out _);
            }
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

        public enum StatModifierType
        {
            Multiplicative = 0,
            Additive = 1,
            Override = 2,
        }       
    }
}
