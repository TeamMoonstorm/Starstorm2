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
    public class VoidBehavior : MonoBehaviour
    {
        public static VoidBehavior instance { get; private set; }

        public static GameObject rockPrefab;
        public Run run;

        public bool condemnedRun = false;

        internal static IEnumerator Init()
        {
            //init prefab
            rockPrefab = PrefabAPI.InstantiateClone(SS2Assets.LoadAsset<GameObject>("VoidRockPickup", SS2Bundle.Interactables), "BondPickup", true);
            rockPrefab.RegisterNetworkPrefab();

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

            if (NetworkServer.active && currStage == "arena" && Run.instance.IsExpansionEnabled(SS2Assets.LoadAsset<ExpansionDef>("SS2ExpansionDef", SS2Bundle.Main)))
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
