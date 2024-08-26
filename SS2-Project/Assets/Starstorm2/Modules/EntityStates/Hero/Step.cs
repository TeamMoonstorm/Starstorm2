using UnityEngine;
using RoR2;
using RoR2.Navigation;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

namespace EntityStates.Hero
{
    public class Step : BaseState
    {
        public static float baseDuration;
        private float duration;
        private Animator animator;
        private HuntressTracker tracker;
        private Vector3 startPos;
        private Vector3 targetPos;
        private bool hasBlinked;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / moveSpeedStat;
            tracker = GetComponent<HuntressTracker>();
            if (tracker != null)
            {
                GetTargetPosition();
            }

            // disappear vfx, animation
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (characterMotor)
                characterMotor.velocity = Vector3.zero;

            if (!hasBlinked)
                SetPosition(Vector3.Lerp(startPos, targetPos, fixedAge / duration));

            if (fixedAge >= duration && !hasBlinked)
            {
                hasBlinked = true;
                // reappear vfx
                SetPosition(targetPos);
            }

            if (fixedAge >= duration && hasBlinked)
            {
                outer.SetNextStateToMain();
            }
        }

        private void SetPosition(Vector3 newPosition)
        {
            if (characterMotor)
            {
                characterMotor.Motor.SetPositionAndRotation(newPosition, Quaternion.identity, true);
            }
        }

        private void GetTargetPosition()
        {
            startPos = transform.position;

            HurtBox hurtBox = tracker.GetTrackingTarget();
            if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.body)
            {
                targetPos = hurtBox.healthComponent.transform.position;
                Vector3 origin = hurtBox.healthComponent.body.corePosition + (hurtBox.healthComponent.body.inputBank.GetAimRay().direction * 30f);

                System.Random random = new System.Random();
                NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                List<NodeGraph.NodeIndex> nodeList = groundNodes.FindNodesInRange(origin, 0f, 10f, HullMask.Golem);
                NodeGraph.NodeIndex randomNode = nodeList[random.Next(nodeList.Count)];
                if (randomNode != null)
                {
                    groundNodes.GetNodePosition(randomNode, out targetPos);
                    targetPos.y += 5f;
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
