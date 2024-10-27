using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(StatusEffectComponent))]
public class StatusEffectComponentEditor : PropertyDrawer
{
    private SerializedProperty statusEffects;
    private SerializedProperty statusEffectValues;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float newLineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        statusEffects = property.FindPropertyRelative("<statusEffects>k__BackingField");
        statusEffectValues = property.FindPropertyRelative("<statusEffectValues>k__BackingField");

        StatusEffect statusEffectFlag = (StatusEffect)statusEffects.enumValueFlag;
        
        EditorGUI.BeginProperty(position, label, property);

        Rect statusEffectRect = new Rect(position.x, position.y, position.width, singleLineHeight);
        EditorGUI.PropertyField(statusEffectRect, statusEffects, new GUIContent("Status Effect"));

        Array statusEffectArray = Enum.GetValues(typeof(StatusEffect));
        int statusEffectTotal = statusEffectArray.Length;

        if (statusEffectValues.arraySize != statusEffectTotal)
        {
            statusEffectValues.arraySize = statusEffectTotal;
        }

        int statusEffectCount = 0;
        for (int i = 0; i < statusEffectArray.Length; i++)
        {
            StatusEffect statusEffect = (StatusEffect)statusEffectArray.GetValue(i);
            if (statusEffectFlag.HasFlag(statusEffect))
            {
                statusEffectCount += 1;
                SerializedProperty statusEffectType = statusEffectValues.GetArrayElementAtIndex(i);
                Rect statusEffectValeusRect = new Rect(position.x, position.y + newLineHeight * statusEffectCount, position.width, singleLineHeight);
                EditorGUI.PropertyField(statusEffectValeusRect, statusEffectType, new GUIContent(statusEffect.ToString()));
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float newLineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        statusEffects = property.FindPropertyRelative("<statusEffects>k__BackingField");
        StatusEffect statusEffectFlag = (StatusEffect)statusEffects.enumValueFlag;

        int statusEffectCount = 1;
        Array statusEffectArray = Enum.GetValues(typeof(StatusEffect));
        for (int i = 0; i < statusEffectArray.Length; i++)
        {
            StatusEffect statusEffect = (StatusEffect)statusEffectArray.GetValue(i);
            if (statusEffectFlag.HasFlag(statusEffect))
            {
                statusEffectCount += 1;
            }
        }
        
        return newLineHeight * statusEffectCount;
    }
}
