using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;


namespace Moonstorm.Starstorm2.Editor
{
    public class TheHolyScripture
    {

        [MenuItem("Tools/ENACT GOD'S WILL")]
        private static void BEASTMODE()
        {

            Debug.Log("Searching for null references in Prefabs.");

            var fucks = GetAllTransformsInPrefabs();
            //var objects = Resources.FindObjectsOfTypeAll<Object>(); //oopsie daisy
            Debug.LogWarning("FAT DUMP");
            foreach (Transform transform in fucks)
            {
                Debug.Log("ASSET PATH: " + AssetDatabase.GetAssetPath(transform));
                Debug.Log(transform.name);

            }
        }


        private static List<Transform> GetAllTransformsInPrefabs()
        {

            List<Transform> transforms = new List<Transform>();

            var guids = AssetDatabase.FindAssets("", null);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var objects = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (UnityEngine.Object obj in objects)
                {
                    Transform go = obj as Transform;
                    if (go == null)
                    {
                        continue;
                    }
                    transforms.Add(go);
                }
            }

            return transforms;
        }

        private static List<GameObject> GetAllGameObjectsInPrefabs()
        {

            List<GameObject> gameObjects = new List<GameObject>();

            var guids = AssetDatabase.FindAssets("", null);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var objects = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (UnityEngine.Object obj in objects)
                {
                    GameObject go = obj as GameObject;
                    if (go == null)
                    {
                        continue;
                    }
                    gameObjects.Add(go);
                }
            }

            return gameObjects;
        }
    }
}
