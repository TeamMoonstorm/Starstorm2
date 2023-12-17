using HG;
using Moonstorm.Components;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class AffixEmpyrean : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdEmpyrean", SS2Bundle.Indev);

        public sealed class Behavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdEmpyrean;

            private void Start()
            {
                foreach (EliteDef ed in EliteCatalog.eliteDefs)
                {
                    if (ed.IsAvailable())
                        body.AddBuff(ed.eliteEquipmentDef.passiveBuffDef);
                }
            }

            private void OnDestroy()
            {
                foreach (EliteDef ed in EliteCatalog.eliteDefs)
                {
                    body.RemoveBuff(ed.eliteEquipmentDef.passiveBuffDef);
                }
            }
        }
    }
}
