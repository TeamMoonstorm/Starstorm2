
using RoR2EditorKit.Common;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.PropertyDrawers
{
    public class SerializableSystemTypeTreePicker : EditorWindow
    {
        private static SerializableSystemTypeTreePicker typeTreePicker;

        private readonly SerializableSystemTypeTreeView treeView = new SerializableSystemTypeTreeView();
        private bool close;
        private SerializableSystemTypeTreeView.SystemTypeTreeInfo selectedType;
        private SerializedProperty serializableSystemTypeReference;
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
                        var data = (SerializableSystemTypeTreeView.SystemTypeTreeInfo)selectedItem?.DataContext;
                        if (selectedItem != null && data.itemType == SerializableSystemTypeTreeView.ItemType.Type)
                            SetType(selectedItem);

                        //The window can now be closed
                        close = true;
                    }
                    else if (GUILayout.Button("Cancel"))
                        close = true;
                    else if (GUILayout.Button("Reset"))
                    {
                        ResetType();
                        close = true;
                    }
                    else if (Event.current.type == EventType.Used && treeView.LastDoubleClickedItem != null)
                    {
                        //We must be in 'used' mode in order for this to work
                        SetType(treeView.LastDoubleClickedItem);
                        close = true;
                    }
                }
            }
        }

        private void SetType(TreeListItem in_item)
        {
            serializedObject.Update();

            selectedType = in_item.DataContext as SerializableSystemTypeTreeView.SystemTypeTreeInfo;
            serializableSystemTypeReference.stringValue = selectedType.fullName;
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetType()
        {
            serializedObject.Update();
            serializableSystemTypeReference.stringValue = null;
            selectedType = null;
            serializedObject.ApplyModifiedProperties();
        }


        public class PickerCreator
        {
            public SerializedProperty parentProperty;
            public SerializedProperty systemTypeReference;
            public Rect pickerPosition;
            public SerializedObject serializedObject;

            internal PickerCreator()
            {
                EditorApplication.delayCall += DelayCall;
            }

            private void DelayCall()
            {
                if (typeTreePicker != null)
                    return;

                typeTreePicker = CreateInstance<SerializableSystemTypeTreePicker>();

                //position the window below the button
                var pos = new Rect(pickerPosition.x, pickerPosition.yMax, 0, 0);

                //If the window gets out of the screen, we place it on top of the button instead
                if (pickerPosition.yMax > Screen.currentResolution.height / 2)
                    pos.y = pickerPosition.y - Screen.currentResolution.height / 2;

                //We show a drop down window which is automatically destroyed when focus is lost
                typeTreePicker.ShowAsDropDown(pos,
                    new Vector2(pickerPosition.width >= 250 ? pickerPosition.width : 250,
                        Screen.currentResolution.height / 2));

                typeTreePicker.serializableSystemTypeReference = systemTypeReference;
                typeTreePicker.serializedObject = serializedObject;

                //First lookup for the required base type attribute.
                Type typeOfObject = serializedObject.targetObject.GetType();


                var field = typeOfObject.GetFields()
                                         .Where(fieldInfo => fieldInfo.GetCustomAttributes(typeof(HG.SerializableSystemType.RequiredBaseTypeAttribute), true) != null)
                                         .Where(fieldInfo => fieldInfo.Name == parentProperty.name)
                                         .FirstOrDefault();

                Type requiredBaseType = null;
                if (field != null)
                {
                    HG.SerializableSystemType.RequiredBaseTypeAttribute attribute = (HG.SerializableSystemType.RequiredBaseTypeAttribute)field.GetCustomAttributes(typeof(HG.SerializableSystemType.RequiredBaseTypeAttribute), true).First();
                    if (attribute != null)
                    {
                        requiredBaseType = attribute.type;
                    }
                }

                //If the lookup fails, look for it using this different method.
                if (requiredBaseType == null)
                {
                    var path = parentProperty.propertyPath;
                    path = path.Substring(0, path.LastIndexOf("."));
                    var pProp = serializedObject.FindProperty(path);
                    var parentType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.GetTypes()).FirstOrDefault(t => t.Name == pProp.type);

                    var fieldInfo = parentType.GetField(parentProperty.name);
                    var requiredBaseTypeAttribute = fieldInfo.GetCustomAttributes(typeof(HG.SerializableSystemType.RequiredBaseTypeAttribute), true).FirstOrDefault();

                    if (fieldInfo != null)
                    {
                        if (requiredBaseTypeAttribute != null)
                        {
                            HG.SerializableSystemType.RequiredBaseTypeAttribute attribute = (HG.SerializableSystemType.RequiredBaseTypeAttribute)requiredBaseTypeAttribute;
                            if (attribute != null)
                            {
                                requiredBaseType = attribute.type;
                            }
                        }
                    }
                }

                typeTreePicker.treeView.AssignDefaults();
                typeTreePicker.treeView.SetRootItem("Types");
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes()
                            .Where(type => !type.IsAbstract);

                        if (requiredBaseType != null)
                            types = types.Where(type => type.IsSubclassOf(requiredBaseType));

                        types.ToList()
                             .ForEach(type => typeTreePicker.treeView.PopulateItem(type));
                    }
                    catch { }
                }
            }
        }

    }


}