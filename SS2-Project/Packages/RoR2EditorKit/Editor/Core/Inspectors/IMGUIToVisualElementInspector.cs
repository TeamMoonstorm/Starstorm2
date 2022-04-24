using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using RoR2;
using RoR2.Skills;
using RoR2EditorKit.Utilities;

namespace RoR2EditorKit.Core.Inspectors
{
    /// <summary>
    /// Inherit from this inspector to make an Editor that looks exactly like the default inspector, but uses UIElements.
    /// <para>Perfect for later on creating a property drawer for a specific property in said inspector, so that you dont have to rewrite the original inspector's functionality.</para>
    /// </summary>
    public abstract class IMGUIToVisualElementInspector : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            root.name = GetType().Name + "_RootElement";

            var children = serializedObject.GetIterator().GetVisibleChildren();
            foreach(var child in children)
            {
                root.Add(new PropertyField(child));
            }
            return root;
        }
    }
}