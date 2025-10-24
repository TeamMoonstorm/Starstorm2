using RoR2;
using UnityEngine;

namespace EntityStates.LampBoss
{
    public class LampBossSpawn : GenericCharacterSpawnState
    {
        public static GameObject spawnVFX;
        public static GameObject spawnVFXblue;
        private static string muzzleString = "Chest";
        private Transform muzzle;

        private static float realDuration = 3f;
        private static float printTime = 2f;
        private static float printStartHeight = 9.5f;
        private static float printMaxHeight = -5f;
        private static float printStartBias = 4f;
        private static float printMaxBias = 2f;

        public override void OnEnter()
        {
            base.duration = realDuration;

            base.OnEnter();
            muzzle = GetModelChildLocator().FindChild(muzzleString);
            bool isBlue = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken == "SS2_SKIN_LAMP_BLUE";
            var effect = isBlue ? spawnVFXblue : spawnVFX;
            Util.PlaySound("WayfarerSpawn", gameObject);
            EffectManager.SimpleEffect(effect, new Vector3(muzzle.position.x, muzzle.position.y + 10, muzzle.position.z), muzzle.rotation, false);

            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                var printController = modelTransform.GetComponent<PrintController>();
                printController.enabled = false;
                printController.printTime = printTime;
                printController.startingPrintHeight = printStartHeight;
                printController.maxPrintHeight = printMaxHeight;
                printController.startingPrintBias = printStartBias;
                printController.maxPrintBias = printMaxBias;
                printController.enabled = true;
            }
        }
    }
}
