//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using System.Linq;
//using System.Reflection;
//using UnityEngine.SceneManagement;


//THIS THING FUCKING SUCKS
//namespace Moonstorm.Starstorm2.Editor
//{
//    public class CheckNullReferences
//    {

//        [MenuItem("Tools/Find Null References in Scriptable Objects")]
//        private static void FindNullReferencesInScriptableObjects()
//        {

//            EditorUtility.DisplayProgressBar("Null References Search", "Searching for null references in ScriptableObjets", 0.0f);
//            Debug.Log("Searching for null references in Scriptable Objects.");

//            List<ScriptableObject> scriptableObjectsWithNullReferences = new List<ScriptableObject>();

//            var guids = AssetDatabase.FindAssets("t:ScriptableObject", null);
//            foreach (var guid in guids)
//            {

//                var path = AssetDatabase.GUIDToAssetPath(guid);
//                var objects = AssetDatabase.LoadAllAssetsAtPath(path);

//                foreach (UnityEngine.Object obj in objects)
//                {

//                    ScriptableObject so = obj as ScriptableObject;

//                    if (so == null)
//                    {
//                        continue;
//                    }

//                    // We need to search in parent classes too.
//                    var type = so.GetType();
//                    bool wasAddedToTheList = false;
//                    while (type != null)
//                    {
//                        if (ObjectContainsNullVariables(type, so))
//                        {
//                            if (!wasAddedToTheList)
//                            {
//                                scriptableObjectsWithNullReferences.Add(so);
//                                wasAddedToTheList = true;
//                            }
//                        }
//                        type = type.BaseType;
//                    }
//                }
//            }

//            if (scriptableObjectsWithNullReferences.Count > 0)
//            {
//                Selection.activeObject = scriptableObjectsWithNullReferences[0];
//                EditorGUIUtility.PingObject(scriptableObjectsWithNullReferences[0]);
//            }
//            else
//            {
//                Selection.activeObject = null;
//            }

//            Debug.Log("Number of scriptable objects with null references found: " + scriptableObjectsWithNullReferences.Count);
//            Debug.Log("Scriptable objects with null references found: " + scriptableObjectsWithNullReferences);

//            EditorUtility.ClearProgressBar();
//        }

//        [MenuItem("Tools/Find Null References in Prefabs")]
//        private static void FindNullReferencesInPrefabs()
//        {

//            Debug.Log("Searching for null references in Prefabs.");

//            var gameObjects = GetAllGameObjectsInPrefabs();
//            List<GameObject> gameObjectsWithNullReferences = FindGameObjectsWithNullReferences(gameObjects);

//            if (gameObjectsWithNullReferences.Count > 0)
//            {
//                Selection.activeGameObject = gameObjectsWithNullReferences[0];
//                EditorGUIUtility.PingObject(gameObjectsWithNullReferences[0]);
//            }
//            else
//            {
//                Selection.activeGameObject = null;
//            }

//            Debug.Log("Number of Prefabs with null references found: " + gameObjectsWithNullReferences.Count);
//            Debug.Log("Prefabs with null references found: ");
//            foreach(GameObject g in gameObjectsWithNullReferences)
//            {
//                Debug.Log(g);
//            }
//        }

//        [MenuItem("Tools/Find Null References in Loaded Scenes")]
//        private static void FindAllNullReferencesInLoadedScenes()
//        {

//            Debug.Log("Searching for null references in all components in loaded scenes.");

//            var gameObjects = GetAllGameObjectsInLoadedScenes();
//            List<GameObject> gameObjectsWithNullReferences = FindGameObjectsWithNullReferences(gameObjects);

//            if (gameObjectsWithNullReferences.Count > 0)
//            {
//                Selection.activeGameObject = gameObjectsWithNullReferences[0];
//                EditorGUIUtility.PingObject(gameObjectsWithNullReferences[0]);
//            }
//            else
//            {
//                Selection.activeGameObject = null;
//            }

//            Debug.Log("Number of GameObjects with null references found: " + gameObjectsWithNullReferences.Count);
//            Debug.Log("GameObjects with null references found: " + gameObjectsWithNullReferences);
//        }

//        private static List<GameObject> GetAllGameObjectsInPrefabs()
//        {

//            List<GameObject> gameObjects = new List<GameObject>();

//            var guids = AssetDatabase.FindAssets("t:Prefab", null);
//            foreach (var guid in guids)
//            {
//                var path = AssetDatabase.GUIDToAssetPath(guid);
//                var objects = AssetDatabase.LoadAllAssetsAtPath(path);
//                foreach (UnityEngine.Object obj in objects)
//                {
//                    GameObject go = obj as GameObject;
//                    if (go == null)
//                    {
//                        continue;
//                    }
//                    gameObjects.Add(go);
//                }
//            }

//            return gameObjects;
//        }

//        private static List<GameObject> GetAllGameObjectsInLoadedScenes()
//        {

//            List<GameObject> gameObjects = new List<GameObject>();

//            // Add root objects from all scenes.
//            for (int i = 0; i < SceneManager.sceneCount; i++)
//            {
//                var scene = SceneManager.GetSceneAt(i);
//                gameObjects.AddRange(scene.GetRootGameObjects());
//            }

//            // All scenes are empty.
//            if (gameObjects.Count == 0)
//            {
//                return gameObjects;
//            }

//            // Add all the children.
//            int idx = 0;
//            do
//            {
//                Transform goTransform = gameObjects[idx].transform;
//                int childCount = goTransform.childCount;
//                for (int i = 0; i < childCount; i++)
//                {
//                    gameObjects.Add(goTransform.GetChild(i).gameObject);
//                }
//                idx++;
//            } while (idx < gameObjects.Count);

//            return gameObjects;
//        }

//        private static List<GameObject> FindGameObjectsWithNullReferences(List<GameObject> gameObjects)
//        {

//            // Some components might have unasigned variables and it is still ok.
//            HashSet<string> ignoreComponentNames = new HashSet<string>() {
//            "TextMeshPro", "Text", "TextMeshProUGUI", "Image", "Toggle", "Touchable", "Button",
//            "EventSystem", "SteamVR_TrackedObject", "XWeaponTrail", "SteamVR_Stats", "SteamVR_Menu",
//            "SteamVR_Ears", "SteamVR_Overlay", "SteamVR_Render", "SteamVR_ControllerManager",
//            "StandaloneInputModule", "VRInputModule", "TMP_SubMeshUI" };
//            string ignoreString = "";
//            bool first = true;
//            foreach (string componentName in ignoreComponentNames)
//            {
//                if (!first)
//                {
//                    ignoreString += ", ";
//                }
//                else
//                {
//                    first = false;
//                }
//                ignoreString += componentName;
//            }

//            Debug.Log("Ignoring components: " + ignoreString);

//            List<GameObject> gameObjectsWithNullReferences = new List<GameObject>();

//            foreach (GameObject gameObject in gameObjects)
//            {

//                var components = gameObject.GetComponents(typeof(MonoBehaviour));

//                // Go throught all components.
//                foreach (Component component in components)
//                {

//                    if (component == null)
//                    {
//                        Debug.LogWarning("Null component found in GameObject: " + GetGameObjectHierarchyName(gameObject));
//                        gameObjectsWithNullReferences.Add(gameObject);
//                        continue;
//                    }

//                    System.Type type = component.GetType();

//                    // Ignore some components.
//                    if (ignoreComponentNames.Contains(type.Name))
//                    {
//                        continue;
//                    }

//                    // We need to search in parent classes too.
//                    bool wasAddedToList = false;
//                    while (type != null)
//                    {
//                        if (ObjectContainsNullVariables(type, component, gameObject))
//                        {
//                            if (!wasAddedToList)
//                            {
//                                gameObjectsWithNullReferences.Add(gameObject);
//                                wasAddedToList = true;
//                            }
//                        }
//                        type = type.BaseType;
//                    }

//                }
//            }

//            return gameObjectsWithNullReferences;
//        }

//        private static bool ObjectContainsNullVariables(System.Type type, UnityEngine.Object obj, GameObject gameObject = null)
//        {

//            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public;
//            var fields = type.GetFields(bindingFlags);

//            bool nullVariableFound = false;

//            foreach (var field in fields)
//            {

//                // Ignore variables which are not serialized.
//                if (field.IsNotSerialized)
//                {
//                    continue;
//                }

//                // Search for custom attribute SerializeField.
//                var customAttributes = field.GetCustomAttributes(false);
//                bool serielizeFieldAttributeFound = false;
//                foreach (var attribute in customAttributes)
//                {
//                    if (attribute is UnityEngine.SerializeField)
//                    {
//                        serielizeFieldAttributeFound = true;
//                        break;
//                    }
//                }

//                if (!field.IsPublic && !serielizeFieldAttributeFound)
//                {
//                    continue;
//                }

//                // Check only for unity objects.                
//                if (!typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
//                {
//                    continue;
//                }

//                // Check if the variable is a null reference.
//                UnityEngine.Object unityObject = field.GetValue(obj) as UnityEngine.Object;
//                bool isNull = unityObject == null;
//                if (isNull)
//                {
//                    string hierarchyName = "";
//                    if (gameObject)
//                    {
//                        hierarchyName = GetGameObjectHierarchyName(gameObject) + "\\";
//                    }
//                    Debug.LogWarning("Unasigned variable found: " + hierarchyName + obj.GetType().Name + "   |   " + field.Name);
//                    nullVariableFound = true;
//                }
//            }

//            return nullVariableFound;
//        }

//        private static string GetGameObjectHierarchyName(GameObject gameObject)
//        {

//            string hierarchyName = gameObject.name;
//            var go = gameObject;
//            for (; ; )
//            {
//                Transform parent = go.transform.parent;
//                if (parent == null)
//                {
//                    break;
//                }
//                go = parent.gameObject;
//                hierarchyName = go.name + "\\" + hierarchyName;
//            }

//            return hierarchyName;
//        }
//    }
//}
