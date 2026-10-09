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
        /// Optional. Randomized boss inventory. If null, only out-of-bounds teleport protection is granted.
        /// </summary>
        public NemesisInventory nemesisInventory;

        /// <summary>
        /// Optional. Local visual attached to the boss on each peer. Register the same prefab reference on all peers.
        /// </summary>
        public GameObject visualEffect;

        /// <summary>
        /// Optional. Skill overrides applied to the boss body on spawn.
        /// </summary>
        public NemesisSpawnCard.SkillOverride[] skillOverrides;

        /// <summary>
        /// Optional. Modifiers for writable float CharacterBody fields whose names start with "base".
        /// When provided, level stats are recalculated from the modified base stats.
        /// </summary>
        public NemesisSpawnCard.StatModifier[] statModifiers;

        /// <summary>
        /// Optional. Override spawn state for the boss body. Leave default for no override.
        /// </summary>
        public SerializableEntityStateType spawnStateOverride;

        /// <summary>
        /// Optional. Finite weight for weighted selection. Values &lt;= 0 are treated as 1.
        /// </summary>
        public float selectionWeight;

        /// <summary>
        /// Reads authored boss configuration without registering it or modifying the source asset.
        /// Pass the result to NemesisCatalog.AddNemesis, just like a code-authored descriptor.
        /// </summary>
        public NemesisCompatInfo(NemesisSpawnCard spawnCard)
        {
            if (!spawnCard)
                throw new ArgumentNullException(nameof(spawnCard));

            masterPrefab = spawnCard.prefab;
            droppedItem = spawnCard.itemDef;
            nemesisInventory = spawnCard.nemesisInventory;
            visualEffect = spawnCard.visualEffect;
            skillOverrides = spawnCard.skillOverrides;
            statModifiers = spawnCard.statModifiers;
            spawnStateOverride = spawnCard.useOverrideState ? spawnCard.overrideSpawnState : default;
            selectionWeight = spawnCard.selectionWeight;
        }
    }
}
