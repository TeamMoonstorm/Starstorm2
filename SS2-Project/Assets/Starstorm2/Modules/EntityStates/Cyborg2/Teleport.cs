using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
namespace EntityStates.Cyborg2
{
    public class Teleport : BaseSkillState
    {
        public override void OnEnter()
        {
            base.OnEnter();
        }






        //move camera to position
        public class CameraMover : MonoBehaviour, ICameraStateProvider
        {
            public float lerpTime;
            public Vector3 position;
            public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState)
            {
                cameraState.position = this.position;
            }

            public bool IsHudAllowed(CameraRigController cameraRigController)
            {
                return true;
            }

            public bool IsUserControlAllowed(CameraRigController cameraRigController)
            {
                return true;
            }

            public bool IsUserLookAllowed(CameraRigController cameraRigController)
            {
                return true;
            }
        }
    }
}
