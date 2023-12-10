using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Assets.Starstorm2.Components
{
    public class EnableOnTimer : MonoBehaviour
    {
        public Behaviour[] components;
        public bool enable = true;
        public float timer;

        private void FixedUpdate()
        {
            this.timer -= Time.fixedDeltaTime;
            if(this.timer <= 0f)
            {
                foreach(Behaviour component in components)
                {
                    component.enabled = enable;
                }
                Destroy(this);
            }
        }
    }
}
