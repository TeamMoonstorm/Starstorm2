using RoR2;
using UnityEngine;
using SS2;
namespace EntityStates.BigLamp
{
    public class DeathState : GenericCharacterDeath
    {
        public override bool shouldAutoDestroy => true;
        public static GameObject effectPrefab;
        public override void OnEnter()
        {
            base.OnEnter();

            var controller = GetComponent<LampCameraPullController>();
            if (controller)
            {
                controller.enabled = false;
            }
            var vfx = FindModelChild("VfxRoot");
            if (vfx)
            {
                vfx.gameObject.SetActive(false);
            }
            if (effectPrefab)
            {
                EffectManager.SimpleEffect(effectPrefab, transform.position, Quaternion.identity, false);
            }
            
        }
    }
}
