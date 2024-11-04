using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomTileData))]
public class CustomTileDataEditor : Editor
{
    private CustomTileData customTileData;
    private SerializedProperty tileLevel;
    private SerializedProperty tileLayerType;
    private SerializedProperty moveableTileLayer;
    private SerializedProperty objectTileLayer;
    private SerializedProperty interactableTileLayer;
    private SerializedProperty entrance;

    private void OnEnable()
    {
        customTileData = target as CustomTileData;

        tileLayerType = serializedObject.FindProperty("<tileLayerType>k__BackingField");
        tileLevel = serializedObject.FindProperty("<tileLevel>k__BackingField");
        moveableTileLayer = serializedObject.FindProperty("<moveableTileLayer>k__BackingField");
        objectTileLayer = serializedObject.FindProperty("<objectTileLayer>k__BackingField");
        interactableTileLayer = serializedObject.FindProperty("<interactableTileLayer>k__BackingField");
        entrance = serializedObject.FindProperty("<entrance>k__BackingField");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        EditorGUILayout.PropertyField(tileLayerType, new GUIContent("Tile Layer Type"));

        switch (customTileData.tileLayerType)
        {
            case TileType.Moveable:
                EditorGUILayout.PropertyField(tileLevel, new GUIContent("Tile Level"));
                EditorGUILayout.PropertyField(moveableTileLayer, new GUIContent("Moveable Tile Layer"));
                break;
            case TileType.Object:
                EditorGUILayout.PropertyField(objectTileLayer, new GUIContent("Object Tile Layer"));
                break;
            case TileType.Interactable:
                EditorGUILayout.PropertyField(interactableTileLayer, new GUIContent("Interactable Tile Layer"));
                if (interactableTileLayer.enumValueIndex == (int)InteractableTileLayer.UnidirectionalTeleport)
                {
                    EditorGUILayout.PropertyField(entrance, new GUIContent("Entrance"));
                }
                break;
            default:
                break;
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
