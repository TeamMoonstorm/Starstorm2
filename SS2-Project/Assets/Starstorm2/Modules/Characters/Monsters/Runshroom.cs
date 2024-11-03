using RoR2;
using UnityEngine;
using System;
using SS2.Components;
using MSU;
using R2API;
using RoR2.ContentManagement;
using System.Collections;

namespace SS2.Monsters
{
    public sealed class Runshroom : SS2Monster
    {
        public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acRunshroom", SS2Bundle.Monsters);
        public override void Initialize()
        {
            if (SS2Main.ChristmasTime)
            {
                SpecialEventPickup pickup = SpecialEventPickup.AddAndSetupComponent(CharacterPrefab.GetComponent<CharacterBody>());
                pickup.contextToken = "SS2_INTERACTABLE_SANTAHAT_CONTEXT";
                SS2Main.Instance.StartCoroutine(AwaitForLoad(pickup));
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private IEnumerator AwaitForLoad(SpecialEventPickup pickup)
        {
            while(!SS2Content.loadStaticContentFinished)
            {
                yield return null;
            }

            pickup.itemDef = SS2Content.Items.SantaHat;
        }
    }
}
