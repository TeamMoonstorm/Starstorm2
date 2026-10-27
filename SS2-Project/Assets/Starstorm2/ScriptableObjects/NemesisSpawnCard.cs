using EntityStates;
using SS2.Components;
using RoR2;
using RoR2.Skills;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2
{
    [CreateAssetMenu(fileName = "NemesisSpawnCard", menuName = "Starstorm2/NemesisSpawnCard")]
    public class NemesisSpawnCard : CharacterSpawnCard
    {

        public NemesisInventory nemesisInventory;
        public bool useOverrideState;
        public SerializableEntityStateType overrideSpawnState = new SerializableEntityStateType(typeof(Idle));
        public StatModifier[] statModifiers;
        public SkillOverride[] skillOverrides;
        public GameObject visualEffect;
        public ItemDef itemDef;
        public float selectionWeight = 1f;


        public override void Spawn(Vector3 position, Quaternion rotation, DirectorSpawnRequest directorSpawnRequest, ref SpawnResult result)
        {
            if (!NetworkServer.active)
            {
                SS2Log.Error($"NemesisSpawnCard.Spawn: {name} can only be spawned by the server.");
                result.success = false;
                return;
            }

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
                var master = characterMaster;
                var body = master.GetBody();
                if (!body)
                {
                    SS2Log.Error($"NemesisSpawnCard.Spawn: {prefab.name} did not spawn a CharacterBody.");
                    NetworkServer.Destroy(master.gameObject);
                    result.spawnedInstance = null;
                    result.success = false;
                    return;
                }

                if (nemesisInventory)
                {
                    nemesisInventory.GiveItems(master.inventory);
                }
                else
                {
                    master.inventory.GiveItem(RoR2Content.Items.TeleportWhenOob);
                }

                if (itemDef)
                {
                    var component = body.gameObject.AddComponent<NemesisItemDrop>();
                    component.itemDef = itemDef;
                }

                ApplyBodyConfiguration(body);

                // CharacterBody.Start links the master after MasterSummon.Perform returns.
                master.onBodyStart += OnBodyStart;
                void OnBodyStart(CharacterBody startedBody)
                {
                    master.onBodyStart -= OnBodyStart;
                    if (FriendManager.instance)
                        FriendManager.instance.RpcSetupNemBoss(startedBody.gameObject, prefab.name);
                    else
                        SS2Log.Error($"NemesisSpawnCard.Spawn: Missing FriendManager; cannot synchronize {prefab.name} boss setup.");
                }

                if (useOverrideState)
                {
                    EntityStateMachine stateMachine;
                    stateMachine = EntityStateMachine.FindByCustomName(body.gameObject, "Body");
                    if (!stateMachine)
                    {
                        SS2Log.Error($"Override Entity State on Nemesis Spawn Card {this} must play on a Body State Machine. Skipping override.");
                        return;
                    }
                    EntityState state = (EntityState)Activator.CreateInstance(overrideSpawnState.stateType);
                    stateMachine.SetNextState(state);
                }
            }
        }
        internal void ApplyBodyConfiguration(CharacterBody characterBody)
        {
            characterBody.isChampion = true;
            if (!characterBody.GetComponent<NemesisResistances>())
                characterBody.gameObject.AddComponent<NemesisResistances>();

            if (characterBody.mainHurtBox && characterBody.mainHurtBox.TryGetComponent(out CapsuleCollider capsuleCollider))
            {
                capsuleCollider.height = 4f;
                capsuleCollider.radius = 4f;
            }

            if (statModifiers != null && statModifiers.Length > 0)
            {
                foreach (var statModifier in statModifiers)
                {
                    if (TryGetStatField(statModifier.fieldName, out var field))
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
                    else
                    {
                        SS2Log.Error($"NemesisSpawnCard: Invalid base-stat field \"{statModifier.fieldName}\" on {name}.");
                    }
                }
                characterBody.PerformAutoCalculateLevelStats();
            }

            if (skillOverrides != null)
            {
                foreach (var skillOverride in skillOverrides)
                {
                    GenericSkill skill = characterBody.skillLocator ? characterBody.skillLocator.GetSkill(skillOverride.skillSlot) : null;
                    if (skill && skillOverride.skillDef)
                        skill.SetSkillOverride(this, skillOverride.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                    else
                        SS2Log.Error($"NemesisSpawnCard: Missing {skillOverride.skillSlot} skill or override on {characterBody.name}.");
                }
            }
            characterBody.MarkAllStatsDirty();
        }

        internal static bool TryGetStatField(string fieldName, out FieldInfo field)
        {
            field = !string.IsNullOrEmpty(fieldName) && fieldName.StartsWith("base", StringComparison.Ordinal)
                ? typeof(CharacterBody).GetField(fieldName, BindingFlags.Public | BindingFlags.Instance)
                : null;
            return field != null && field.FieldType == typeof(float) && !field.IsInitOnly;
        }

        private void OnValidate()
        {
            if (statModifiers == null) return;
            foreach (var statModifier in statModifiers)
            {
                if (!TryGetStatField(statModifier.fieldName, out _))
                    SS2Log.Error($"{statModifier.fieldName} in Nemesis Spawn Card {name} is not a writable float base-stat field.");
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
