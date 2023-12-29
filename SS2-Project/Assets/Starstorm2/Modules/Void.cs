using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2
{
    public class Void : NetworkBehaviour
    {
        public static GameObject shrinePrefab;

        internal static void Init()
        {
            shrinePrefab = SS2Assets.LoadAsset<GameObject>("VoidRockPickup", SS2Bundle.Interactables);
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
        }

        public static void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);

            var currStage = SceneManager.GetActiveScene().name;

            var position = new Vector3(-156f, 41f, 63f);
            var rotation = Quaternion.Euler(0, 282, 0);

            if (NetworkServer.active && currStage == "arena")
            {
                GameObject term = Instantiate(shrinePrefab, position, rotation);
                NetworkServer.Spawn(term);
                Debug.Log("TERM : " + term);
                Debug.Log("placed shrine at: " + position + "pos & " + rotation + "rot");
            }
        }
    }
}
