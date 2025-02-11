using SS2;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemCaptain.Weapon
{
    public class GiveManaReductionBuff : BaseBuffOrder
    {
        public override void OnOrderEffect()
        {
            if (isAuthority)
            {
                characterBody.AddTimedBuffAuthority(SS2Content.Buffs.bdNemCapManaReduction.buffIndex, 10f);
            }
        }
    }
}
