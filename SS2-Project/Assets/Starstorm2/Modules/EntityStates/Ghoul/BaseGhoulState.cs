using System;
using System.Collections.Generic;
using UnityEngine;
using RoR2;

namespace EntityStates.Ghoul
{
    public abstract class BaseGhoulState : BaseSkillState
    {
        public GameObject attackerObject { get => attackerBody ? attackerBody.gameObject : null; }
        public CharacterBody attackerBody { get => leaderBody != null ? leaderBody : characterBody; }

        private CharacterBody leaderBody;

        public override void OnEnter()
        {
            base.OnEnter();
            if (characterBody.master && characterBody.master.minionOwnership)
            {
                CharacterMaster leaderMaster = characterBody.master.minionOwnership.ownerMaster;
                if (leaderMaster)
                {
                    leaderBody = leaderMaster.GetBody();
                }
            }
        }
    }
}
