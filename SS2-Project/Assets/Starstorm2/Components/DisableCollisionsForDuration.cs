using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Moonstorm.Starstorm2.Components
{
    public class DisableCollisionsForDuration : MonoBehaviour
    {
        public Collider[] collidersSelf;
        public Collider[] collidersOther;
        public float timer;
        public bool disableSelfColliders;
        private void Awake()
        {
            foreach (Collider collider1 in collidersSelf)
            {
                foreach (Collider collider2 in collidersSelf)
                {
                    Physics.IgnoreCollision(collider1, collider2, true);
                }
                collider1.enabled = !disableSelfColliders;
            }
        }

        private void FixedUpdate()
        {
            this.timer -= Time.fixedDeltaTime;
            if(this.timer <= 0f)
            {
                foreach(Collider collider1 in collidersSelf)
                {
                    foreach (Collider collider2 in collidersSelf)
                    {
                        Physics.IgnoreCollision(collider1, collider2, false);
                    }
                    collider1.enabled = true;
                }
                Destroy(this);
            }
        }
    }
}
