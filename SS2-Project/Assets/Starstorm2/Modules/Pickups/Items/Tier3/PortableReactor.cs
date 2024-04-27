using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS2.Items
{
    public sealed class PortableReactor : SS2Item
    {
        private const string token = "SS2_ITEM_PORTABLEREACTOR_DESC";

        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Duration of invulnerability from Portable Reactor. (1 = 1 second)")]
        [FormatToken(token, 0)]
        public static float invulnTime = 80f;
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Stacking duration of invulnerability. (1 = 1 second)")]
        [FormatToken(token, 1)]
        public static float stackingInvuln = 40f;

        //★ ty nebby
        //To-Do: This thing spits out a few errors every time a stage is started. Doesn't really NEED fixed, but probably should be.
        //N: This tbh should be moved to a hook on stage start, to avoid having a resurrected player becoming invincible.
        public override void Initialize()
        {
            CharacterBody.onBodyStartGlobal += ImFuckingInvincible;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "PortableReactor" - Items
             */
            yield break;
        }

        private void ImFuckingInvincible(CharacterBody obj)
        {
            if(obj.TryGetItemCount(_itemDef, out var count))
            {
                if (obj.isPlayerControlled)
                {
                    obj.AddTimedBuff(SS2Content.Buffs.BuffReactor, invulnTime + ((count - 1) * stackingInvuln));
                }
                else
                {
                    obj.AddTimedBuff(SS2Content.Buffs.BuffReactor, invulnTime / 4 + ((count - 1) * stackingInvuln));
                }
            }
        }
    }
}