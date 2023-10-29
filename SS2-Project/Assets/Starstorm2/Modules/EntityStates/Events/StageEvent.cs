using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using Moonstorm;
using System;
using UnityEngine.Networking;

namespace EntityStates.Events
{
    public class StageEvent : EventState
    {
        public override void FixedUpdate()
        {
            //if (Run.) to-do: end after teleporter?
            if (!HasWarned && fixedAge >= warningDur)
                StartEvent();
        }
    }
}
