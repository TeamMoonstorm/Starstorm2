
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
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.Euler(-90, 0, 0);

        [MSU.AsyncAssetLoad]
        internal static IEnumerator Init()
        {
            directorPrefab = SS2Assets.LoadAsset<GameObject>("StarstormDirectors", SS2Bundle.Base);
            crystalPrefab = SS2Assets.LoadAsset<GameObject>("SkinCrystalPickup", SS2Bundle.Interactables);
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
                SpawnCrystals();
            }
        }

        public void SpawnCrystals()
        {
            string currStage = SceneManager.GetActiveScene().name;
            switch (currStage)
            {
                case "golemplains":
                    GolemPlains();
                    break;
                case "golemplains2":
                    GolemPlains2();
                    break;
                case "default":
                    break;
            }
        }

        public void GolemPlains()
        {
        }

        public void GolemPlains2()
        {
            position = new Vector3(9.8f, 127.5f, -251.8f);
            rotation = Quaternion.Euler(0, 5, 0);

            GameObject commandoCrystalA = Instantiate(crystalPrefab, position, rotation);
            SkinCrystal commandoSkinCrystalA = commandoCrystalA.GetComponent<SkinCrystal>();
            commandoSkinCrystalA.bodyString = "CommandoBody";
            commandoSkinCrystalA.skinUnlockID = 1;
            NetworkServer.Spawn(commandoCrystalA);
        }
    }
}
