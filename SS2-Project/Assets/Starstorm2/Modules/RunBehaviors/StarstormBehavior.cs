
using R2API;
using RoR2;
using SS2.Monsters;
using SS2;
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
            if (ClayMonger.manager)
            {
                Instantiate(ClayMonger.manager, transform);
            }
        }

        private void Start()
        {
            instance = this;
            On.RoR2.SceneDirector.Start += SceneDirector_Start; /////// fuck OFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
            base.transform.SetParent(Run.instance.transform);
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
                SpawnObjects();
            }
        }

        public void SpawnObjects()
        {
            string currStage = SceneManager.GetActiveScene().name;
            switch (currStage)
            {
                case "frozenwall":
                    FrozenWall();
                    break;
                case "golemplains":
                    GolemPlains();
                    break;
                case "golemplains2":
                    GolemPlains2();
                    break;
                case "moon2":
                    Moon2();
                    break;
                case "mysteryspace":
                    MysterySpace();
                    break;
                case "rootjungle":
                    RootJungle();
                    break;
                case "default":
                    break;
            }
        }

        public void FrozenWall()
        {
            GameObject frozenWallInstance = Instantiate(frozenWallPrefab, Vector3.zero, Quaternion.identity);
        }

        public void GolemPlains()
        {
        }

        public void GolemPlains2()
        {
            GameObject golemPlains2Instance = Instantiate(golemPlains2Prefab, Vector3.zero, Quaternion.identity);
        }

        public void Moon2()
        {
            GameObject moon2Instance = Instantiate(moon2Prefab, Vector3.zero, Quaternion.identity);
        }

        public void MysterySpace()
        {
            GameObject mysterySpaceInstance = Instantiate(mysterySpacePrefab, Vector3.zero, Quaternion.identity);
        }
        public void RootJungle()
        {
            GameObject rootJungleInstance = Instantiate(rootJunglePrefab, Vector3.zero, Quaternion.identity);
        }
    }
}
