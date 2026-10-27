using EntityStates.NemToolbot;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    [RequireComponent(typeof(ProjectileController))]
    [RequireComponent(typeof(ProjectileGrappleController))]
    public class NemToolbotGrappleController : MonoBehaviour
    {
        private void Start()
        {
            GameObject owner = GetComponent<ProjectileController>().owner;
            EntityStateMachine hookStateMachine = owner ? EntityStateMachine.FindByCustomName(owner, "Hook") : null;
            if (!hookStateMachine)
            {
                SS2Log.Error("NemToolbot grapple: Missing owner or Hook state machine.");
                if (NetworkServer.active)
                    Destroy(gameObject);
                return;
            }

            // Loader's controller only links EntityStates.Loader.FireHook itself.
            if (hookStateMachine.state is FireGrapplingHook firingState)
                firingState.SetHookReference(gameObject);
        }
    }
}
