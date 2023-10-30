using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.NemMerc.Clone
{
    public class CloneDeath : GenericCharacterDeath
    {
        /// <summary>
        /// // IDK DO SOMETHING COOL HERE
        /// </summary>
        /// 

        public override void OnEnter()
        {
            base.OnEnter();

            DestroyModel();
        }
        public override bool shouldAutoDestroy => true;
    }
}
