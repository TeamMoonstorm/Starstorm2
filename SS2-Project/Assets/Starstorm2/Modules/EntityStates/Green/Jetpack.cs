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
            if (targetPosition == null)
            {
                NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                List<NodeGraph.NodeIndex> nodeList = groundNodes.FindNodesInRange(characterBody.transform.position, 0f, 32f, HullMask.Human);
                NodeGraph.NodeIndex randomNode = nodeList[random.Next(nodeList.Count)];
                if (randomNode != null)
                {
                    Vector3 position;
                    groundNodes.GetNodePosition(randomNode, out position);
                    targetPosition = position;
                }
            }
            ICanDoMath();
        }

        private void ICanDoMath()
        {
            Vector3 displacement = targetPosition - startPosition;
            Vector3 horizontalDisplacement = new Vector3(displacement.x, 0, displacement.z);
            float horizontalDistance= horizontalDisplacement.magnitude;

            float launchAngle = 35 * Mathf.Deg2Rad;

            float horizontalSpeed = Mathf.Sqrt((horizontalDistance * 9.81f) / (Mathf.Sin(2 * launchAngle))); //i forget how to access gravity PLEASE BE JUST LIKE EARTH LOL!
            float verticalSpeed = horizontalSpeed * Mathf.Tan(launchAngle);

            Vector3 launchDirection = horizontalDisplacement.normalized * horizontalSpeed;
            launchDirection.y = verticalSpeed;

            launchSpeed = launchDirection.magnitude;
            characterMotor.Motor.ForceUnground();
            characterMotor.velocity = launchDirection;

            Vector3 direction = (targetPosition - characterBody.footPosition).normalized;
            float distance = Vector3.Distance(characterBody.footPosition, targetPosition);
            float launchSpeed2 = Mathf.Sqrt(distance * 9.81f / Mathf.Sin(2 * 35 * Mathf.Deg2Rad));

            Vector3 velocity = direction * launchSpeed2;
            velocity.y = launchSpeed * Mathf.Sin(35f * Mathf.Deg2Rad);
            characterMotor.velocity = velocity;
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
