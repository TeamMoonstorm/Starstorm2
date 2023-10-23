using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
namespace EntityStates.Pickups.Remuneration
{
    public class OfferCoin : BaseState
    {
        public float duration = 2f;

        public override void OnEnter()
        {
            base.OnEnter();

            base.transform.Find("RemunerationPortal").gameObject.SetActive(true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.fixedAge >= duration)
            {
                this.outer.SetNextState(new Reward());
            }
        }
    }
}
