using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class PreventAdd : PropertyAttribute
{
    public class ConstantLengthAttribute : PropertyAttribute { }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(PreventAdd))]
public class ConstantLengthAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.isArray && property.propertyType.Equals(SerializedPropertyType.Generic))
        {
            int originalSize = property.arraySize;

            EditorGUI.PropertyField(position, property, label, true);

            if (property.arraySize > originalSize)
            {
                property.arraySize = originalSize;
            }
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
#endif
