using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Core.PropertyDrawers
{
    public abstract class VisualElementPropertyDrawer : PropertyDrawer
    {
        protected VisualElement RootVisualElement
        {
            get
            {
                if (_rootVisualElement == null)
                    _rootVisualElement = new VisualElement();
                return _rootVisualElement;
            }
        }

        private VisualElement _rootVisualElement;

        protected SerializedProperty serializedProperty;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _ = RootVisualElement;
            serializedProperty = property;
            DrawPropertyGUI();
            return RootVisualElement;
        }

        protected abstract void DrawPropertyGUI();
    }
}
