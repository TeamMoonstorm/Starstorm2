using RoR2;
using UnityEngine;

namespace EntityStates.Lamp
{
    public class LampSpawn : GenericCharacterSpawnState
    {
        public static GameObject spawnVFX;
        public static GameObject spawnVFXblue;

        public static GameObject preSpawnEffect;

        private static float realDuration = 1f;
        private static float spawnEffectTime = 0.3f;
        private static string muzzleString = "Head";
        private static string spawnSoundString = "FollowerSpawn";
        private Transform muzzle;
        private EffectData effectData;
        private bool hasSpawnedEffect;
        private CharacterModel characterModel;

        private static float printTime = 1f;
        private static float printStartHeight = -100f;
        private static float printMaxHeight = -100f;
        private static float printStartBias = 3.33f;
        private static float printMaxBias = 0.4f;
        public override void OnEnter()
        {
            base.duration = realDuration;

            base.OnEnter();
            muzzle = FindModelChild(muzzleString);
            characterModel = GetModelTransform().GetComponent<CharacterModel>();
            characterModel.invisibilityCount++;
            if (preSpawnEffect)
            {
                EffectManager.SimpleEffect(preSpawnEffect, muzzle ? muzzle.position : characterBody.corePosition, Quaternion.identity, false);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= spawnEffectTime)
            {
                if (!hasSpawnedEffect)
                {
                    Appear();
                }
            }
        }

        private void Appear()
        {
            hasSpawnedEffect = true;
            Transform modelTransform = base.GetModelTransform();

            characterModel.invisibilityCount--;
            //PlayAnimation("Body", "Spawn");
            bool isBlue = SkinCatalog.FindCurrentSkinDefForBodyInstance(gameObject).skinIndex == SS2.Monsters.Lamp.HesBlue;
            var effect = isBlue ? spawnVFXblue : spawnVFX;
            EffectManager.SimpleEffect(effect, muzzle ? muzzle.position : characterBody.corePosition, Quaternion.identity, false);
            Util.PlaySound(spawnSoundString, gameObject);

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

        public override void OnExit()
        {
            base.OnExit();

            if (!hasSpawnedEffect)
            {
                Appear();
            }

        }
    }
}
