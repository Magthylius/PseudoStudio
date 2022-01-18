#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Tenshi.Editorial
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class TenshiAttributesEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label, true);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
#endif