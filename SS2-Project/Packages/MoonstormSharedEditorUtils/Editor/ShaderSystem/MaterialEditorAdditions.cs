using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils.ShaderSystem
{
    [InitializeOnLoad]
    public class MaterialEditorAdditions
    {
        static MaterialEditorAdditions()
        {
            Editor.finishedDefaultHeaderGUI += Draw;
        }

        private static void Draw(Editor obj)
        {
            if (!(obj is MaterialEditor materialEditor))
            {
                return;
            }

            var id = GUIUtility.GetControlID(new GUIContent("Pick shader asset"), FocusType.Passive);

            Shader shader = (materialEditor.target as Material).shader;
            if(shader.name.StartsWith("Stubbed"))
            {
                if(GUILayout.Button("Upgrade to Real Shader"))
                {
                    MaterialShaderManager.Upgrade((Material)materialEditor.target);
                }
            }

            if (GUILayout.Button("Pick shader asset"))
            {
                EditorGUIUtility.ShowObjectPicker<Shader>(null, false, null, id);
            }

            if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == id)
            {
                materialEditor.SetShader(EditorGUIUtility.GetObjectPickerObject() as Shader, true);
            }
        }
    }
}