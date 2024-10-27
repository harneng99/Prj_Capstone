using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BuffComponent.BuffEntry))]
public class BuffComponentEditor : PropertyDrawer
{
    private SerializedProperty selectedStat;
    private SerializedProperty durationTurn;
    private SerializedProperty constantValueChange;
    private SerializedProperty valueChange;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float newLineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        selectedStat = property.FindPropertyRelative("<selectedStat>k__BackingField");
        durationTurn = property.FindPropertyRelative("<durationTurn>k__BackingField");
        constantValueChange = property.FindPropertyRelative("<constantValueChange>k__BackingField");
        valueChange = property.FindPropertyRelative("<valueChange>k__BackingField");

        EditorGUI.BeginProperty(position, label, property);

        string[] statNames = typeof(Stat)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.PropertyType.Equals(typeof(StatComponent)))
            .Select(property => property.Name)
            .ToArray();

        int selectedIndex = System.Array.IndexOf(statNames, selectedStat.stringValue);
        selectedIndex = EditorGUI.Popup(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), "Stat", selectedIndex, statNames);

        if (selectedIndex >= 0)
        {
            selectedStat.stringValue = statNames[selectedIndex];
        }

        position.y += newLineHeight;
        EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), durationTurn);

        position.y += newLineHeight;
        EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), constantValueChange);

        position.y += newLineHeight;
        EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), valueChange);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float newLineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        return newLineHeight * 4.0f;
    }
}
