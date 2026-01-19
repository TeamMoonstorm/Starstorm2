using UnityEngine;
using RoR2;
using RoR2.UI;

namespace SS2.Components
{
    public class AdvancedMapNameClusterController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Whether or not the map cluster title card appears at stage start. ONLY APPLIES AT START!")]
        public bool playOnStart = true;
        [Tooltip("Initial Delay for the map cluster title card to appear.")]
        public float initialDelay = 1f;

        public void Awake()
        {
            RoR2.CameraRigController.OnHUDUpdated += MakeChangesToHUDInstances;
        }
        public void OnEnable()
        {
            RoR2.CameraRigController.OnHUDUpdated += MakeChangesToHUDInstances;
        }

        public void OnDisable()
        {
            RoR2.CameraRigController.OnHUDUpdated -= MakeChangesToHUDInstances;
        }

        private void MakeChangesToHUDInstances()
        {
            foreach (HUD hud in HUD.instancesList)
            {
                ChildLocator childLoc = hud.transform.GetComponent<ChildLocator>();
                Transform mapNameCluster = childLoc.FindChild("MapNameCluster");

                TypewriteTextController ttc = mapNameCluster.GetComponent<TypewriteTextController>();
                ttc.playOnStart = this.playOnStart;
                ttc.initialDelay = this.initialDelay;
            }
        }

        public void PlayMapClusterAnimation()
        {
            foreach (HUD hud in HUD.instancesList)
            {
                ChildLocator childLoc = hud.transform.GetComponent<ChildLocator>();
                Transform mapNameCluster = childLoc.FindChild("MapNameCluster");

                TypewriteTextController ttc = mapNameCluster.GetComponent<TypewriteTextController>();
                ttc.StartTyping();
            }
        }

    }
}