using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Monsters
{
    [DisabledContent]
    public sealed class Agarthan : MonsterBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("AgarthanBody", SS2Bundle.Indev);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("AgarthanMaster", SS2Bundle.Indev);
        public override MSMonsterDirectorCard MonsterDirectorCard { get; } = null; //SS2Assets.LoadAsset<MSMonsterDirectorCard>("msmdcAgarthan", SS2Bundle.Indev);
    }
}
