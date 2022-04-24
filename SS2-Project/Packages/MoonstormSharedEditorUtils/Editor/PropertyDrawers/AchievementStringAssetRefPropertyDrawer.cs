/*using RoR2EditorKit.Core.PropertyDrawers;
using UnityEditor;

namespace Moonstorm.EditorUtils.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(MSUnlockableDef.AchievementStringAssetRef))]
    public class AchievementStringAssetRefPropertyDrawer : EditorGUILayoutPropertyDrawer
    {
        protected override void DrawPropertyDrawer(SerializedProperty property)
        {
            var stringRef = property.FindPropertyRelative("AchievementIdentifier");
            var unlockableRef = property.FindPropertyRelative("UnlockableDef");

            if (!string.IsNullOrEmpty(stringRef.stringValue))
            {
                DrawField(stringRef);
            }
            else if (unlockableRef.objectReferenceValue != null)
            {
                DrawField(unlockableRef);
            }
            else if (unlockableRef.objectReferenceValue == null && string.IsNullOrEmpty(stringRef.stringValue))
            {
                DrawField(stringRef);
                DrawField(unlockableRef);
            }
        }
    }
}
*/