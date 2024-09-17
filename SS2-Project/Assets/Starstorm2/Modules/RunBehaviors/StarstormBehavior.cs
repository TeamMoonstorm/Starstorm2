
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class StarstormBehavior : MonoBehaviour
    {
        public static StarstormBehavior instance { get; private set; }
        private EtherealBehavior ethInstance;
        public Run run;

        public static GameObject directorPrefab;
        public GameObject directorInstance;

        internal static IEnumerator Init()
        {
            directorPrefab = PrefabAPI.InstantiateClone(SS2Assets.LoadAsset<GameObject>("StarstormDirectors", SS2Bundle.Base), "StarstormDirector", true);
            directorPrefab.RegisterNetworkPrefab();

            yield return null;
        }

        private void Awake()
        {
            run = GetComponentInParent<Run>();
        }

        private void Start()
        {
            instance = this;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
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
            }
        }
    }
}
