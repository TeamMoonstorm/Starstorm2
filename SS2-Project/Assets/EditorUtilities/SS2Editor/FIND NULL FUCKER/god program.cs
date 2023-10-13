using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;


namespace Moonstorm.Starstorm2.Editor
{
    public class CheckNullReferences
    {

        [MenuItem("Tools/ENACT GOD'S WILL")]
        private static void FindNullReferencesInPrefabs()
        {

            Debug.Log("Searching for null references in Prefabs.");

            var gameObjects = GetAllGameObjectsInPrefabs();

            Debug.LogWarning("FAT DUMP");
            foreach (GameObject gameObject in gameObjects)
            {
                Debug.Log("PREFAB: " + gameObject);
                Debug.Log("ASSET PATH: " + AssetDatabase.GetAssetPath(gameObject));
            }
        }


        private static List<GameObject> GetAllGameObjectsInPrefabs()
        {

            List<GameObject> gameObjects = new List<GameObject>();

            var guids = AssetDatabase.FindAssets("t:Prefab", null);
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
