using JetBrains.Annotations;
using RoR2;
using UnityEngine;
using System;
using Moonstorm.Starstorm2.Components;
namespace Moonstorm.Starstorm2.Monsters
{
    public sealed class Runshroom : MonsterBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("RunshroomBody", SS2Bundle.Monsters);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("RunshroomMaster", SS2Bundle.Monsters);

        private MSMonsterDirectorCard defaultCard = SS2Assets.LoadAsset<MSMonsterDirectorCard>("msmdcRunshroom", SS2Bundle.Monsters);

        public override void Initialize()
        {
            base.Initialize();
            MonsterDirectorCards.Add(defaultCard);          
        }

        public override void ModifyPrefab()
        {
            base.ModifyPrefab();

            DateTime today = DateTime.Today;
            if (today.Month == 12 && ((today.Day == 27) || (today.Day == 26) || (today.Day == 25) || (today.Day == 24) || (today.Day == 23)))
            {
                ChristmasTime();
            }
        }

        private void ChristmasTime()
        {
            BodyPrefab.AddComponent<SantaHatPickup>();
            BodyPrefab.AddComponent<EntityLocator>().entity = BodyPrefab;
            BodyPrefab.AddComponent<Highlight>().targetRenderer = BodyPrefab.GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().mainSkinnedMeshRenderer;
        }
    }

    
}
