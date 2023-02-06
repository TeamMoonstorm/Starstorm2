using Moonstorm.Starstorm2;
using RoR2;
using UnityEngine.Networking;

namespace EntityStates.Pickups.Extinction
{
    class ExtinctionEnterState : ExtinctionBaseState
    {
        //TODO: Get some kind of warning prefab that spawns, would be neat, dunno.
        public Run.FixedTimeStamp readyTime { get; private set; }
        public static float baseDuration = 10f;

        public override void OnEnter()
        {
            base.OnEnter();
            if (base.isAuthority)
            {
                this.readyTime = Run.FixedTimeStamp.now + ExtinctionEnterState.baseDuration;
            }
            base.extinctionController.showExtinctionDisplay = false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && (base.ownerBody || base.ownerBody.inventory.GetItemCount(SS2Content.Items.RelicOfExtinction) < 1) && Run.FixedTimeStamp.now >= readyTime)
            {
                this.outer.SetNextState(new ExtinctionIdleState());
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            base.extinctionController.showExtinctionDisplay = true;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.readyTime);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.readyTime = reader.ReadFixedTimeStamp();
        }
    }
}
