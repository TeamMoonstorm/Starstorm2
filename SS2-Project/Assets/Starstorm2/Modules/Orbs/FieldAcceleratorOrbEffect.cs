using RoR2;
using RoR2.Orbs;
using static SS2.Items.FieldAccelerator;
namespace SS2.Orbs
{
    public class FieldAcceleratorOrbEffect : OrbEffect
    {
        private void Start()
        {
            base.Start();
            var teleInstance = TeleporterInteraction.instance;
            if (teleInstance != null)
            {
                var fatb = teleInstance.GetComponent<FieldAcceleratorTeleporterBehavior>();

                if(!targetTransform)
                    targetTransform = teleInstance.transform;
            }
        }
    }
}
