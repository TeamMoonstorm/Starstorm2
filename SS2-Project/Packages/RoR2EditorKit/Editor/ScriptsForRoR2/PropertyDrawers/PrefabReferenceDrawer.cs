using RoR2;
using RoR2EditorKit.Core.PropertyDrawers;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

/// <summary>
/// This is a script given by Ghor, All rights reserved to Hopoo Games.
/// <para>If youre a hopoo employee, and the team have decided this is not ok, please contact Nebby at nebby1999@gmail.com</para>
/// </summary>
namespace RoR2EditorKit.RoR2Related.PropertyDrawers
{

    [CustomPropertyDrawer(typeof(PrefabReferenceAttribute))]
    public class PrefabReferencePropertyDrawer : IMGUIPropertyDrawer
    {
        private static GameObject ConvertToPrefab(GameObject sceneObject)
        {
            PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(sceneObject);
            if (prefabStage == null)
            {
                return null;
            }

            string relativePath = RoR2.Util.BuildPrefabTransformPath(sceneObject.transform.root, sceneObject.transform, false);
            string assetPath = prefabStage.prefabAssetPath;
            if (assetPath != null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                GameObject prefabChild = prefab?.transform.Find(relativePath)?.gameObject;
                return prefabChild;
            }
            return null;
        }

        private GameObject memoizedSceneObject;
        private GameObject memoizedPrefabObject;

        private GameObject GetDraggedPrefabObject()
        {
            Object[] objectReferences = DragAndDrop.objectReferences;
            if (objectReferences.Length == 1)
            {
                if (objectReferences[0] is GameObject gameObject)
                {
                    if (!ReferenceEquals(memoizedSceneObject, gameObject))
                    {
                        memoizedSceneObject = gameObject;
                        memoizedPrefabObject = ConvertToPrefab(memoizedSceneObject);
                    }
                    return memoizedPrefabObject;
                }
            }
            return null;
        }

        protected override void DrawCustomDrawer()
        {
            EditorGUI.PropertyField(rect, property, label, true);
            if (rect.Contains(Event.current.mousePosition))
            {
                switch (Event.current.type)
                {
                    case EventType.DragUpdated:
                        {
                            GameObject draggedObject = GetDraggedPrefabObject();
                            if (draggedObject)
                            {
                                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                            }
                        }
                        break;
                    case EventType.DragPerform:
                        {
                            GameObject draggedObject = GetDraggedPrefabObject();
                            if (draggedObject)
                            {
                                DragAndDrop.AcceptDrag();
                                property.objectReferenceValue = draggedObject;
                            }
                        }
                        break;
                }
            }
        }
    }
}