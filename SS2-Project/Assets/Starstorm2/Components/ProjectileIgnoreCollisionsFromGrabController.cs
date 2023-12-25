using System;
using UnityEngine;
using RoR2.Projectile;
using RoR2;
using System.Linq;
namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(ProjectileController))]
    public class ProjectileIgnoreCollisionsFromGrabController : MonoBehaviour
    {
        [NonSerialized]
        public Collider[] collidersToIgnore;
        private Collider myCollider;
      
        // AAAAAAAAAAAAAAAAHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        private void Start()
        {
            GrabController grabController = base.GetComponent<ProjectileController>().owner.GetComponent<GrabController>();
            this.myCollider = base.GetComponent<Collider>();

            if (grabController && !grabController.victimInfo.body)
            {
                //SS2Log.Warning("ProjectileIgnoreCollisionsFromGrabController.Start: GrabController had no body");
                return;
            }
            if (grabController.victimInfo.body.teamComponent.teamIndex != base.GetComponent<TeamFilter>().teamIndex) return;
            // O_O
            this.collidersToIgnore = HG.ArrayUtils.Join(grabController.victimColliders, grabController.victimInfo.body.hurtBoxGroup.hurtBoxes.Select(h => h.GetComponent<Collider>()).ToArray());
            if (grabController)
            {
                foreach (Collider collider in this.collidersToIgnore)
                {
                    if(collider) // not null checking these leads to a lot of issues idk.
                        Physics.IgnoreCollision(myCollider, collider, true);
                }
            }
            
            
        }
    }
}
