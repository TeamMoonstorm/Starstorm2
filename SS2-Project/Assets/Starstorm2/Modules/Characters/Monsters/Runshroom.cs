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

        private void ChristmasTime()
        {
            CharacterPrefab.GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial = SS2Assets.LoadAsset<Material>("matSantaHat", SS2Bundle.Items);
            CharacterPrefab.GetComponent<CharacterBody>().doNotReassignToTeamBasedCollisionLayer = true; // idk what the layers do but it breaks the itneraction if switched
            CharacterPrefab.AddComponent<SantaHatPickup>();
            CharacterPrefab.AddComponent<EntityLocator>().entity = CharacterPrefab;
            CharacterPrefab.AddComponent<Highlight>().targetRenderer = CharacterPrefab.GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().mainSkinnedMeshRenderer;
        }
    }
}
