using RoR2;
using RoR2.Navigation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Invaders.Green
{
    public class Jetpack : BaseSkillState
    {
        public Vector3 targetPosition;
        private float launchSpeed;
        private Vector3 startPosition;

        private static System.Random random = new System.Random();

        public override void OnEnter()
        {
            base.OnEnter();
            startPosition = characterBody.footPosition;
            NodeGraph groundNodes = SceneInfo.instance.groundNodes;
            List<NodeGraph.NodeIndex> nodeList = groundNodes.FindNodesInRange(characterBody.transform.position, 64f, 80f, HullMask.Human);
            NodeGraph.NodeIndex randomNode = nodeList[random.Next(nodeList.Count)];
            if (randomNode != null)
            {
                Vector3 position;
                groundNodes.GetNodePosition(randomNode, out position);
                targetPosition = position;
            }
            ICanDoMath();
        }

        private void ICanDoMath()
        {
            float yInitSpeed = Trajectory.CalculateInitialYSpeed(4f, targetPosition.y - startPosition.y);
            float xOffset = targetPosition.x - startPosition.x;
            float zOffset = targetPosition.z - startPosition.z;
            Vector3 launchVector = new Vector3(xOffset / 4f, yInitSpeed, zOffset / 4f);

            characterMotor.velocity = launchVector;
            characterMotor.disableAirControlUntilCollision = true;
            characterMotor.Motor.ForceUnground();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && characterMotor.isGrounded)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
