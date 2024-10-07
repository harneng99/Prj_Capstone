using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EntityConsistentData))]
public class EntityDataEditor : Editor
{
    private EntityConsistentData entityData;
    private SerializedProperty movementVelocity;
    private SerializedProperty testingDictionaryRange;
    private SerializedProperty testingDictionary;

    private void OnEnable()
    {
        entityData = target as EntityConsistentData;
        movementVelocity = serializedObject.FindProperty("<movementVelocity>k__BackingField");
        testingDictionaryRange = serializedObject.FindProperty("<testingDictionaryRange>k__BackingField");
        testingDictionary = serializedObject.FindProperty("<testingDictionary>k__BackingField");
    }

    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();
        serializedObject.UpdateIfRequiredOrScript();

        EditorGUILayout.PropertyField(movementVelocity, new GUIContent("Movement Velocity"));
        EditorGUILayout.PropertyField(testingDictionaryRange, new GUIContent("Testing Dictionary Range"));
        EditorGUILayout.PropertyField(testingDictionary, new GUIContent("Testing Dictionary"));

        for (int x = -entityData.testingDictionaryRange.x; x <= entityData.testingDictionaryRange.x; x++)
        {
            for (int y = -entityData.testingDictionaryRange.y; y <= entityData.testingDictionaryRange.y; y++)
            {
                for (int z = -entityData.testingDictionaryRange.z; z <= entityData.testingDictionaryRange.z; z++)
                {
                    if (x + y + z != 0) continue;

                    Vector3Int currentKey = new Vector3Int(x, y, z);
                    
                    if (!entityData.testingDictionary.ContainsKey(currentKey))
                    {
                        entityData.testingDictionary.Add(currentKey, true);
                        EditorUtility.SetDirty(entityData);
                    }
                }
            }
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
