using SS2;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemCaptain.Weapon
{
    public class GiveManaRegenBuff : BaseBuffOrder
    {
        public override void OnOrderEffect()
        {
            if (isAuthority)
            {
                characterBody.AddTimedBuffAuthority(SS2Content.Buffs.bdNemCapManaRegen.buffIndex, 15f);
            }
        }
    }
}
