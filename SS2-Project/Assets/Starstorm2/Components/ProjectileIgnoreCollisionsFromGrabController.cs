using System;
using UnityEngine;
using RoR2.Projectile;
using RoR2;
using System.Collections.Generic;
namespace SS2.Components
{
    [RequireComponent(typeof(ProjectileController))]
    public class ProjectileIgnoreCollisionsFromGrabController : MonoBehaviour
    {
        private Collider myCollider;
      
        // AAAAAAAAAAAAAAAAHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        private void Start()
        {
            ProjectileController controller = GetComponent<ProjectileController>();
            GrabController grabController = controller.owner ? controller.owner.GetComponent<GrabController>() : null;
            this.myCollider = base.GetComponent<Collider>();

            if (grabController && !grabController.victimInfo.body)
            {
                return;
            }
            var victimBody = grabController.victimInfo.body;
            if (victimBody.teamComponent.teamIndex != base.GetComponent<TeamFilter>().teamIndex)
            {
                return;
            }
            if (!victimBody.hurtBoxGroup)
            {
                return;
            }

            for (int i = 0; i < victimBody.hurtBoxGroup.hurtBoxes.Length; i++)
            {
                var hurtBox = victimBody.hurtBoxGroup.hurtBoxes[i];
                if (hurtBox && hurtBox.TryGetComponent(out Collider collider))
                {
                    Physics.IgnoreCollision(myCollider, collider, true);
                }
            }
        }
    }
}
