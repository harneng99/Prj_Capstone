using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class ReadOnly : PropertyAttribute
{
    public class ReadOnlyAttribute : PropertyAttribute { }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnly))]
public class ReadOnlyAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}
#endif