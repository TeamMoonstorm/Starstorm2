using MSU;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2.ContentManagement;

namespace SS2
{
    public abstract class SS2Equipment : IEquipmentContentPiece
    {
        public abstract NullableRef<GameObject> ItemDisplayPrefab { get; }
        EquipmentDef IContentPiece<EquipmentDef>.Asset => EquipmentDef;
        public abstract EquipmentDef EquipmentDef { get; }

        public abstract bool Execute(EquipmentSlot slot);
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public abstract IEnumerator LoadContentAsync();
        public abstract void OnEquipmentLost(CharacterBody body);
        public abstract void OnEquipmentObtained(CharacterBody body);
    }
}