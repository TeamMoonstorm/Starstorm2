using R2API;
using R2API.Utils;
using RoR2;
using RoR2.ExpansionManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using MSU;
namespace SS2.Components
{
    public class VoidBehavior : MonoBehaviour
    {
        public static VoidBehavior instance { get; private set; }

        public static GameObject rockPrefab;
        public Run run;

        public bool condemnedRun = false;

        internal static IEnumerator Init()
        {
            SS2Content.SS2ContentPack.AddContentFromAssetCollection(SS2Assets.LoadAsset<AssetCollection>("acVoidRock", SS2Bundle.Interactables)); // this is stupid. we should be doing this automatically
            rockPrefab = SS2Assets.LoadAsset<GameObject>("VoidRockPickup", SS2Bundle.Interactables);
            yield return null;
        }

        private void Awake()
        {
            run = GetComponentInParent<Run>();
        }

        private void Start()
        {
            instance = this;

            //place rock
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
        }


        private void OnDestroy()
        {
            On.RoR2.SceneDirector.Start -= SceneDirector_Start;
        }

        public static void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);

            var currStage = SceneManager.GetActiveScene().name;

            var position = new Vector3(-156f, 41f, 63f);
            var rotation = Quaternion.Euler(0, 282, 0);

            if (NetworkServer.active && currStage == "arena")
            {
                GameObject term = Instantiate(rockPrefab, position, rotation);
                NetworkServer.Spawn(term);

#if DEBUG
                Debug.Log("TERM : " + term);
                Debug.Log("placed shrine at: " + position + "pos & " + rotation + "rot");
#endif
            }
        }
    }
}
