//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace EntityStates.Nuke
//{
//    public abstract class BaseNukeFireState : BaseState, SS2.Survivors.Nuke.IChargedState
//    {
//        public float charge { get; set; }

//        public override void OnEnter()
//        {
//            base.OnEnter();
//            characterBody.AddSpreadBloom(charge);
//        }
//    }
//}