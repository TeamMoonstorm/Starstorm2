using UnityEngine;

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
