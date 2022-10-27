using Moonstorm;
using Moonstorm.Starstorm2.Components;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Beastmaster
{
    public class BeastmasterMain : GenericCharacterMain
    {

        public static AnimationCurve flutterVelocityCurve;
        public static float holdTimeToFlutter;
        public static float flutterHorizontalVelocity;
        public static float flutterVerticalVelocity;

        private float flutterStopwatch;
        private bool hasUsedFlutter;

    }
}
