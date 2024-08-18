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
        private Animator animator;
        private HuntressTracker tracker;
        private Vector3 targetPos;

        public override void OnEnter()
        {
            base.OnEnter();
            tracker = GetComponent<HuntressTracker>();
            if (tracker != null)
            {
                GetTargetPosition();
            }


        }

        private void GetTargetPosition()
        {
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
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
