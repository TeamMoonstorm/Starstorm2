using RoR2;
using UnityEngine;

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
    }
}
