using EntityStates;
using RoR2EditorKit.Common;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.PropertyDrawers
{
    public class EntityStateTreePicker : EditorWindow
    {
        private static EntityStateTreePicker stateTreePicker;

        private readonly EntityStateTreeView treeView = new EntityStateTreeView();
        private bool close;
        private EntityStateTreeView.EntityStateTreeInfo selectedState;
        private SerializedProperty serializableEntityStateReference;
        private SerializedObject serializedObject;

        public static EditorWindow LastFocusedWindow = null;

        private void Update()
        {
            if (close)
            {
                Close();

                if (LastFocusedWindow)
                {
                    EditorApplication.delayCall += LastFocusedWindow.Repaint;
                    LastFocusedWindow = null;
                }
            }
        }

        private void OnGUI()
        {
            using (new GUILayout.VerticalScope())
            {
                treeView.DisplayTreeView(TreeListControl.DisplayTypes.USE_SCROLL_VIEW);

                using (new GUILayout.HorizontalScope("box"))
                {
                    if (GUILayout.Button("Ok"))
                    {
                        //Get the selected item
                        var selectedItem = treeView.GetSelectedItem();
                        var data = (EntityStateTreeView.EntityStateTreeInfo)selectedItem?.DataContext;
                        if (selectedItem != null && data.itemType == EntityStateTreeView.ItemType.EntityState)
                            SetState(selectedItem);

                        //The window can now be closed
                        close = true;
                    }
                    else if (GUILayout.Button("Cancel"))
                        close = true;
                    else if (GUILayout.Button("Reset"))
                    {
                        ResetState();
                        close = true;
                    }
                    else if (Event.current.type == EventType.Used && treeView.LastDoubleClickedItem != null)
                    {
                        //We must be in 'used' mode in order for this to work
                        SetState(treeView.LastDoubleClickedItem);
                        close = true;
                    }
                }
            }
        }

        private void SetState(TreeListItem in_item)
        {
            serializedObject.Update();

            selectedState = in_item.DataContext as EntityStateTreeView.EntityStateTreeInfo;
            serializableEntityStateReference.stringValue = selectedState.fullName;
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetState()
        {
            serializedObject.Update();
            serializableEntityStateReference.stringValue = typeof(Idle).AssemblyQualifiedName;
            selectedState = null;
            serializedObject.ApplyModifiedProperties();
        }


        public class PickerCreator
        {
            public SerializedProperty entityStateReference;
            public Rect pickerPosition;
            public SerializedObject serializedObject;

            internal PickerCreator()
            {
                EditorApplication.delayCall += DelayCall;
            }

            private void DelayCall()
            {
                if (stateTreePicker != null)
                    return;

                stateTreePicker = CreateInstance<EntityStateTreePicker>();

                //position the window below the button
                var pos = new Rect(pickerPosition.x, pickerPosition.yMax, 0, 0);

                //If the window gets out of the screen, we place it on top of the button instead
                if (pickerPosition.yMax > Screen.currentResolution.height / 2)
                    pos.y = pickerPosition.y - Screen.currentResolution.height / 2;

                //We show a drop down window which is automatically destroyed when focus is lost
                stateTreePicker.ShowAsDropDown(pos,
                    new Vector2(pickerPosition.width >= 250 ? pickerPosition.width : 250,
                        Screen.currentResolution.height / 2));

                stateTreePicker.serializableEntityStateReference = entityStateReference;
                stateTreePicker.serializedObject = serializedObject;

                stateTreePicker.treeView.AssignDefaults();
                stateTreePicker.treeView.SetRootItem("Entity States");
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        assembly.GetTypes()
                            .Where(type => type.IsSubclassOf(typeof(EntityState)) && !type.IsAbstract)
                            .ToList()
                            .ForEach(type => stateTreePicker.treeView.PopulateItem(type));
                    }
                    catch { }
                }
            }
        }

    }
}
