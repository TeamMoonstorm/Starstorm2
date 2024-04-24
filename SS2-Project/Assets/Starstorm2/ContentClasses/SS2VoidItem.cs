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
    public abstract class SS2VoidItem : IVoidItemContentPiece
    {
        public abstract NullableRef<List<GameObject>> ItemDisplayPrefabs { get; }
        ItemDef IContentPiece<ItemDef>.Asset => ItemDef;
        public abstract ItemDef ItemDef { get; }

        public abstract List<ItemDef> GetInfectableItems();
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public abstract IEnumerator LoadContentAsync();
    }
}