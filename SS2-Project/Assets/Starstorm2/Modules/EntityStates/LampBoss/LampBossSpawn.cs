using RoR2;
using UnityEngine;

namespace EntityStates.LampBoss
{
    public class LampBossSpawn : GenericCharacterSpawnState
    {
        public static GameObject spawnVFX;
        public static GameObject spawnVFXblue;
        private static string childString = "BodyMesh";
        private Transform muzzle;

        private static float realDuration = 3.7f;
        private static float printTime = 3.7f;
        private static float printStartHeight = 9.5f;
        private static float printMaxHeight = -5f;
        private static float printStartBias = 6f;
        private static float printMaxBias = 2f;

        private Animator animator;
        private AimAnimator aimAnimator;


        public override void OnEnter()
        {
            base.damageStat = realDuration;
            base.OnEnter();

            // TODO: SPAWN ANIMATION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            animator = GetModelAnimator();
            animator.enabled = false;
            aimAnimator = GetAimAnimator();
            aimAnimator.enabled = false;


            bool isBlue = SkinCatalog.FindCurrentSkinDefForBodyInstance(gameObject).skinIndex == SS2.Monsters.LampBoss.HesBlue;
            var effect = isBlue ? spawnVFXblue : spawnVFX;
            Util.PlaySound("WayfarerSpawn", gameObject);
            if (effect)
            {
                GameObject effectInstance = GameObject.Instantiate(effect, transform.position, Quaternion.identity);
                var bodyMesh = FindModelChild(childString).GetComponent<SkinnedMeshRenderer>();
                var ps = effectInstance.GetComponent<ParticleSystem>();

                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                shape.skinnedMeshRenderer = bodyMesh;
            }

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

        public override void OnExit()
        {
            base.OnExit();

            animator.enabled = true;
            aimAnimator.enabled = true;
            
        }
    }
}
