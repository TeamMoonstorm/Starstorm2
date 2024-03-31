using RoR2;
using System.Collections.Generic;
using UnityEngine;

using Moonstorm;
namespace SS2.Monsters
{
    [DisabledContent]
    public sealed class Agarthan : MonsterBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("AgarthanBody", SS2Bundle.Indev);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("AgarthanMaster", SS2Bundle.Indev);
        public override List<MSMonsterDirectorCard> MonsterDirectorCards => null;
    }
}
