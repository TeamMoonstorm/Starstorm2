using System;
using EntityStates;
using RoR2;
using UnityEngine;

namespace SS2
{
    [Serializable]
    public struct NemesisCompatInfo
    {
        /// <summary>
        /// Required. The CharacterMaster prefab for the nemesis boss.
        /// </summary>
        public GameObject masterPrefab;

        /// <summary>
        /// Optional. The item dropped on defeat. If null, no item is dropped and the boss is always eligible to spawn.
        /// </summary>
        public ItemDef droppedItem;

        /// <summary>
        /// Optional. Skill overrides applied to the boss body on spawn.
        /// </summary>
        public NemesisSpawnCard.SkillOverride[] skillOverrides;

        /// <summary>
        /// Optional. Stat modifiers applied to the boss body on spawn.
        /// </summary>
        public NemesisSpawnCard.StatModifier[] statModifiers;

        /// <summary>
        /// Optional. Override spawn state for the boss body. Leave default for no override.
        /// </summary>
        public SerializableEntityStateType spawnStateOverride;

        /// <summary>
        /// Optional. Weight for weighted selection. Values &lt;= 0 are treated as 1.
        /// </summary>
        public float selectionWeight;
    }
}
