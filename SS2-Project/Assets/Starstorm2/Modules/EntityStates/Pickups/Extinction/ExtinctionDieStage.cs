using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Pickups.Extinction
{
    internal class ExtinctionDieStage : ExtinctionBaseState
    {
        public Run.FixedTimeStamp readyTime { get; private set; }
        public static float baseDuration = 10f;

        protected override bool shouldFollow
        {
            get
            {
                return false;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (base.isAuthority)
            {
                this.readyTime = Run.FixedTimeStamp.now + ExtinctionDieStage.baseDuration;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.ownerBody)
            {
                this.outer.SetNextState(new ExtinctionIdleState());
            }
            if (base.isAuthority && !base.ownerBody && Run.FixedTimeStamp.now >= readyTime)
            {
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.attacker = base.gameObject;
                blastAttack.inflictor = base.gameObject;
                blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
                blastAttack.position = base.characterBody.corePosition;
                blastAttack.procCoefficient = 1f;
                blastAttack.radius = base.GetScale() * 2f;
                blastAttack.baseForce = base.GetScale();
                blastAttack.bonusForce = Vector3.up * 20f;
                blastAttack.baseDamage = base.ownerBody.baseDamage * 5;
                blastAttack.falloffModel = BlastAttack.FalloffModel.Linear;
                blastAttack.damageColorIndex = DamageColorIndex.Item;
                blastAttack.attackerFiltering = AttackerFiltering.Default;
                blastAttack.Fire();

                UnityEngine.Object.Destroy(base.gameObject);
            }
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