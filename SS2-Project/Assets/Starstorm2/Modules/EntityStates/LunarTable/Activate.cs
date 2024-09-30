using RoR2;
using UnityEngine;

namespace EntityStates.LunarTable
{
    public class Activate : EntityState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            base.PlayAnimation("Activate", "Activate");
        }
    }
}
