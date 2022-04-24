using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    //Remove foldout of array, Set element's name to ESM's custom name
    [CustomEditor(typeof(NetworkStateMachine))]
    public class NetworkStateMachineInspector : ComponentInspector<NetworkStateMachine>
    {
        SerializedProperty stateMachines;

        VisualElement inspectorData;
        IntegerField arraySize;
        VisualElement stateMachineHolder;

        Dictionary<EntityStateMachine, ObjectField> ESMToField = new Dictionary<EntityStateMachine, ObjectField>();
        protected override void OnEnable()
        {
            base.OnEnable();
            stateMachines = serializedObject.FindProperty("stateMachines");

            OnVisualTreeCopy += () =>
            {
                inspectorData = Find<VisualElement>("InspectorData");
                arraySize = Find<IntegerField>(inspectorData, "arraySize");
                stateMachineHolder = Find<VisualElement>(inspectorData, "StateMachineHolder");
            };
        }
        protected override void DrawInspectorGUI()
        {
            arraySize.value = stateMachines.arraySize;
            arraySize.RegisterValueChangedCallback(SetSize);
            SetSize();
        }

        private void SetSize(ChangeEvent<int> evt = null)
        {
            stateMachines.arraySize = evt == null ? stateMachines.arraySize : evt.newValue;
            serializedObject.ApplyModifiedProperties();

            for(int i = 0; i < stateMachines.arraySize; i++)
            {
                SerializedProperty prop = stateMachines.GetArrayElementAtIndex(i);

                if (prop == null)
                    continue;

                var esm = prop.objectReferenceValue as EntityStateMachine;

                if(!esm)
                {
                    continue;
                }

                UpdateOrCreateField(esm);
            }
        }

        private void OnESMSet(ChangeEvent<UnityEngine.Object> evt)
        {
            var obj = evt.newValue;
            if (!obj)
                return;
            
            if(obj is EntityStateMachine esm)
            {
                UpdateOrCreateField(esm);
            }
        }

        private void UpdateOrCreateField(EntityStateMachine esm)
        {
            if (ESMToField.TryGetValue(esm, out var field))
            {
                field.label = esm.customName;
                if(field.parent != stateMachineHolder)
                {
                    field.TryRemoveFromParent();
                    stateMachineHolder.Add(field);
                }
            }
            else
            {
                ObjectField objField = new ObjectField();
                objField.SetObjectType<EntityStateMachine>();
                objField.label = esm.customName;
                objField.tooltip = $"Initial State Type: \"{esm.initialStateType.typeName}\"" +
                    $"\n\nMain State Type: \"{esm.mainStateType.typeName}\"";
                objField.value = esm;
                objField.RegisterValueChangedCallback(OnESMSet);
                stateMachineHolder.Add(objField);
                ESMToField.Add(esm, objField);
            }
        }
    }
}
