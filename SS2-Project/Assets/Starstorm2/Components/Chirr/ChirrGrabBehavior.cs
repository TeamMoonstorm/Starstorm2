using EntityStates;
using RoR2;
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(GrabController))]
    public class ChirrGrabBehavior : MonoBehaviour
    {
        private GrabController grabController;
        private TeamComponent teamComponent;

        public float buffLingerDuration = 5f;
        private void Awake()
        {
            this.grabController = base.GetComponent<GrabController>();
            this.teamComponent = base.GetComponent<TeamComponent>();
            this.grabController.onVictimGrabbed += AddBuff;
            this.grabController.onVictimReleased += RemoveBuff;
        }

        private void RemoveBuff(VehicleSeat.PassengerInfo info)
        {
            if(info.body.HasBuff(SS2Content.Buffs.BuffChirrGrabFriend))
                info.body.RemoveBuff(SS2Content.Buffs.BuffChirrGrabFriend);
            if (info.body && info.body.teamComponent.teamIndex == this.teamComponent.teamIndex)
            {                
                info.body.AddTimedBuff(SS2Content.Buffs.BuffChirrGrabFriend, buffLingerDuration);
            }
        }

        private void AddBuff(VehicleSeat.PassengerInfo info)
        {
            if (info.body && info.body.teamComponent.teamIndex == this.teamComponent.teamIndex) info.body.AddBuff(SS2Content.Buffs.BuffChirrGrabFriend);
        }
    }
}
