
using R2API;
using RoR2;
using SS2.Monsters;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace SS2.Components
{
    public class StarstormBehavior : MonoBehaviour
    {
        public static StarstormBehavior instance { get; private set; }
        private EtherealBehavior ethInstance;
        public Run run;

        public static GameObject directorPrefab;
        public GameObject directorInstance;
        public Xoroshiro128Plus voidshopRng; // for persistent chests

        public static GameObject crystalPrefab;

        public static GameObject frozenWallPrefab;
        public static GameObject golemPlains2Prefab;
        public static GameObject moon2Prefab;
        public static GameObject mysterySpacePrefab;
        public static GameObject rootJunglePrefab;

        [MSU.AsyncAssetLoad]
        internal static IEnumerator Init()
        {
            directorPrefab = SS2Assets.LoadAsset<GameObject>("StarstormDirectors", SS2Bundle.Base);

            crystalPrefab = SS2Assets.LoadAsset<GameObject>("SkinCrystalPickup", SS2Bundle.Interactables);

            frozenWallPrefab = SS2Assets.LoadAsset<GameObject>("FrozenWallObjects", SS2Bundle.Base);
            golemPlains2Prefab = SS2Assets.LoadAsset<GameObject>("GolemPlains2Objects", SS2Bundle.Base);
            moon2Prefab = SS2Assets.LoadAsset<GameObject>("Moon2Objects", SS2Bundle.Base);
            mysterySpacePrefab = SS2Assets.LoadAsset<GameObject>("MysterySpaceObjects", SS2Bundle.Base);
            rootJunglePrefab = SS2Assets.LoadAsset<GameObject>("RootJungleObjects", SS2Bundle.Base);

            SS2Content.SS2ContentPack.networkedObjectPrefabs.Add(new GameObject[] { crystalPrefab });

            yield return null;
        }

        private void Awake()
        {
            run = GetComponentInParent<Run>();
            if(ClayMonger.manager)
            {
                Instantiate(ClayMonger.manager, transform);
            }
        }

        private void Start()
        {
            instance = this;
            On.RoR2.SceneDirector.Start += SceneDirector_Start; /////// fuck OFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
            base.transform.SetParent(Run.instance.transform);
            voidshopRng = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);
        }

        private void OnDestroy()
        {
            On.RoR2.SceneDirector.Start -= SceneDirector_Start;
        }

        public void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);

            if (NetworkServer.active)
            {
                directorInstance = Instantiate(directorPrefab);
                NetworkServer.Spawn(directorInstance);
                // SpawnObjects(); <- currently only for skin crystals. WIP.
            }
        }

        public void SpawnObjects()
        {
            string currStage = SceneManager.GetActiveScene().name;
            GameObject stagePrefab = null;
            switch (currStage)
            {
                case "frozenwall":
                    stagePrefab = frozenWallPrefab;
                    break;
                case "golemplains":
                    stagePrefab = null;
                    break;
                case "golemplains2":
                    stagePrefab = golemPlains2Prefab;
                    break;
                case "moon2":
                    stagePrefab = moon2Prefab;
                    break;
                case "mysteryspace":
                    stagePrefab = mysterySpacePrefab;
                    break;
                case "rootjungle":
                    stagePrefab = rootJunglePrefab;
                    break;
                case "default":
                    stagePrefab = null;
                    break;
            }

            if (stagePrefab != null)
            {
                GameObject stageObjects = GameObject.Instantiate(stagePrefab, Vector3.zero, Quaternion.identity);
                NetworkServer.Spawn(stageObjects);
            }
        }
    }
}
