using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Monsters
{
    public sealed class Runshroom : MonsterBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("RunshroomBody", SS2Bundle.Indev);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("RunshroomMaster", SS2Bundle.Indev);

        private MSMonsterDirectorCard defaultCard = SS2Assets.LoadAsset<MSMonsterDirectorCard>("msmdcRunshroom", SS2Bundle.Indev);

        public override void Initialize()
        {
            base.Initialize();
            MonsterDirectorCards.Add(defaultCard);
        }
    }
}
