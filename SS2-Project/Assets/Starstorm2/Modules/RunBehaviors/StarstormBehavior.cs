
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
        Quaternion rotation = Quaternion.Euler(-90, 0, 0);

        [MSU.AsyncAssetLoad]
        internal static IEnumerator Init()
        {
            directorPrefab = SS2Assets.LoadAsset<GameObject>("StarstormDirectors", SS2Bundle.Base);
            crystalPrefab = SS2Assets.LoadAsset<GameObject>("SkinCrystalPickup", SS2Bundle.Interactables);
            SS2Content.SS2ContentPack.networkedObjectPrefabs.Add(new GameObject[] { crystalPrefab });
            ModifyUnlocks();
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
                case "moon2":
                    Moon2();
                    break;
                case "mysteryspace":
                    MysterySpace();
                    break;
                case "default":
                    break;
            }
        }

        public static void ModifyUnlocks()
        {
            SS2Assets.LoadAsset<UnlockableDef>("ss2.skin.commando.recolor1", SS2Bundle.Vanilla).cachedName = "SS2_SKIN_COMMANDO_ALTRECOLOR1";
        }

        public void GolemPlains()
        {
        }

        public void GolemPlains2()
        {
            Vector3 commandoCrystalAPosition = new Vector3(290.6743f, 67.84853f, -49.18279f);
            rotation = Quaternion.Euler(0, 5, 0);

            GameObject commandoCrystalA = Instantiate(crystalPrefab, commandoCrystalAPosition, rotation);
            SkinCrystal commandoSkinCrystalA = commandoCrystalA.GetComponent<SkinCrystal>();
            commandoSkinCrystalA.bodyString = "CommandoBody";
            commandoSkinCrystalA.skinUnlockID = 1;
            GameObjectUnlockableFilter gouf = commandoCrystalA.GetComponent<GameObjectUnlockableFilter>();
            gouf.forbiddenUnlockableDef = SS2Assets.LoadAsset<UnlockableDef>("ss2.skin.commando.recolor1", SS2Bundle.Vanilla);
            gouf.AchievementToDisable = "SS2_SKIN_COMMANDO_ALTRECOLOR1.Achievement";
            NetworkServer.Spawn(commandoCrystalA);
        }

        public void Moon2()
        {
            Vector3 commandoCrystalBPosition = new Vector3(290.6743f, 67.84853f, -49.18279f);
            rotation = Quaternion.Euler(0, 5, 0);

            GameObject commandoCrystalB = Instantiate(crystalPrefab, commandoCrystalBPosition, rotation);
            SkinCrystal commandoSkinCrystalB = commandoCrystalB.GetComponent<SkinCrystal>();
            commandoSkinCrystalB.bodyString = "CommandoBody";
            commandoSkinCrystalB.skinUnlockID = 1;
            GameObjectUnlockableFilter gouf = commandoCrystalB.GetComponent<GameObjectUnlockableFilter>();
            gouf.forbiddenUnlockableDef = SS2Assets.LoadAsset<UnlockableDef>("ss2.skin.commando.recolor2", SS2Bundle.Vanilla);
            gouf.AchievementToDisable = "SS2_SKIN_COMMANDO_ALTRECOLOR2";
            NetworkServer.Spawn(commandoCrystalB);
        }

        public void MysterySpace()
        {
            Vector3 commandoCrystalCPosition = new Vector3(290.6743f, 67.84853f, -49.18279f);
            rotation = Quaternion.Euler(0, 5, 0);

            GameObject commandoCrystalC = Instantiate(crystalPrefab, commandoCrystalCPosition, rotation);
            SkinCrystal commandoSkinCrystalC = commandoCrystalC.GetComponent<SkinCrystal>();
            commandoSkinCrystalC.bodyString = "CommandoBody";
            commandoSkinCrystalC.skinUnlockID = 1;
            GameObjectUnlockableFilter gouf = commandoCrystalC.GetComponent<GameObjectUnlockableFilter>();
            gouf.forbiddenUnlockableDef = SS2Assets.LoadAsset<UnlockableDef>("ss2.skin.commando.recolor3", SS2Bundle.Vanilla);
            gouf.AchievementToDisable = "SS2_SKIN_COMMANDO_ALTRECOLOR3";
            NetworkServer.Spawn(commandoCrystalC);
        }
    }
}
