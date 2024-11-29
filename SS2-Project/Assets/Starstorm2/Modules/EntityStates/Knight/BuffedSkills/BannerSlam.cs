using EntityStates;
using UnityEngine;
using RoR2;
using RoR2.Skills;
using UnityEngine.Networking;

namespace EntityStates.Knight
{
    public class BannerSlam : BannerSpecial
    {
        protected override void ModifyBlastAttack(BlastAttack blastAttack)
        {
            //TODO: using force in this game for gameplay is ass. Do this instead https://discord.com/channels/562704639141740588/562704639569428506/1192073657267261534
            //also I think dee's concept had him swiping everyone in before jumping, rather than on impact, then you banner them all when you land
                //think I would make a new knightmelee state and on enter set the roll statemachine to that state and apply the force to the hitresults 
            blastAttack.baseForce = -1000f;
            blastAttack.bonusForce = Vector3.up * 1000;
        }
    }

}