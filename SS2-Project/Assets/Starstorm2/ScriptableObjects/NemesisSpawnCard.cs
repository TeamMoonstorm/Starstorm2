using Assets.Starstorm2.ScriptableObjects;
using EntityStates;
using Moonstorm.Starstorm2.Components;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Skills;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.ScriptableObjects
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
        public string childName;
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

        public class SyncBaseStats : INetMessage
        {
            NetworkInstanceId bodyNetId;
            float maxHealth;
            float regen;
            float maxShield;
            float movementSpeed;
            float acceleration;
            float jumpPower;
            float damage;
            float attackSpeed;
            float crit;
            float armor;
            float visionDistance;
            int jumpCount;

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(bodyNetId);
                writer.Write(maxHealth);
                writer.Write(regen);
                writer.Write(maxShield);
                writer.Write(movementSpeed);
                writer.Write(acceleration);
                writer.Write(jumpPower);
                writer.Write(damage);
                writer.Write(attackSpeed);
                writer.Write(crit);
                writer.Write(armor);
                writer.Write(visionDistance);
                writer.Write(jumpCount);
            }

            public void Deserialize(NetworkReader reader)
            {
                bodyNetId = reader.ReadNetworkId();
                maxHealth = reader.ReadSingle();
                regen = reader.ReadSingle();
                maxShield = reader.ReadSingle();
                movementSpeed = reader.ReadSingle();
                acceleration = reader.ReadSingle();
                jumpPower = reader.ReadSingle();
                damage = reader.ReadSingle();
                attackSpeed = reader.ReadSingle();
                crit = reader.ReadSingle();
                armor = reader.ReadSingle();
                visionDistance = reader.ReadSingle();
                jumpCount = reader.ReadInt32();
            }

            public void OnReceived()
            {
                if (NetworkServer.active)
                {
                    return;
                }
                GameObject bodyObject = Util.FindNetworkObject(bodyNetId);
                if(!bodyObject)
                {
                    SS2Log.Warning($"{typeof(SyncBaseStats).FullName}: Could not retrieve GameObject with network ID {bodyNetId}");
                }
                CharacterBody body = bodyObject.GetComponent<CharacterBody>();
                if (!body)
                {
                    SS2Log.Warning($"{typeof(SyncBaseStats).FullName}: Retrieved GameObject {bodyObject} but the GameObject does not have a CharacterBody");
                    return;
                }

                body.baseMaxHealth = maxHealth;
                body.baseRegen = regen;
                body.baseMaxShield = maxShield;
                body.baseMoveSpeed = movementSpeed;
                body.baseAcceleration = acceleration;
                body.baseJumpPower = jumpPower;
                body.baseDamage = damage;
                body.baseAttackSpeed = attackSpeed;
                body.baseCrit = crit;
                body.baseArmor = armor;
                body.baseVisionDistance = visionDistance;
                body.baseJumpCount = jumpCount;

                body.PerformAutoCalculateLevelStats();
                SS2Log.Info($"Synced base stats of {body}");
            }

            public SyncBaseStats()
            {

            }

            public SyncBaseStats(CharacterBody body)
            {
                NetworkIdentity netID = body.GetComponent<NetworkIdentity>();
                bodyNetId = netID.netId;
                maxHealth = body.baseMaxHealth;
                regen = body.baseRegen;
                maxShield = body.baseMaxShield;
                movementSpeed = body.baseMoveSpeed;
                acceleration = body.baseAcceleration;
                jumpPower = body.baseJumpPower;
                damage = body.baseDamage;
                attackSpeed = body.baseAttackSpeed;
                crit = body.baseCrit;
                armor = body.baseArmor;
                visionDistance = body.baseVisionDistance;
                jumpCount = body.baseJumpCount;
            }
        }
    }
}
