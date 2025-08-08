using R2API;
using R2API.Utils;
using RoR2;
using RoR2.ExpansionManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
namespace SS2.Components
{
    public class SS2RunManager : MonoBehaviour
    {
        public static SS2RunManager instance { get; private set; }

        public Run run;


        internal static IEnumerator Init()
        {
            yield return null;
        }

        private void Awake()
        {
            run = GetComponentInParent<Run>();
        }

        private void Start()
        {
            instance = this;

            SS2Log.Debug("[SS2RunManager.cs] in Start()");

            // Add Hooks
            On.RoR2.Run.OnClientGameOver += Run_OnClientGameOver;
        }

        private void OnDestroy()
        {
            // Remove Hooks
            On.RoR2.Run.OnClientGameOver -= Run_OnClientGameOver;
        }

        private void Run_OnClientGameOver(On.RoR2.Run.orig_OnClientGameOver orig, Run self, RunReport runReport)
        {
            orig(self, runReport);

            SS2Telemetry.TrackEvent("testing", "idk", 0);

        }

        
    }
}
