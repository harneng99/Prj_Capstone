using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(CombatAbility))]
public class CombatAbilityEditor : Editor
{
    [SerializeField] private Texture2D gridCellTexture;
    private Texture2D emptyTexture;

    private const float maxGridCellLength = 25.0f;
    private const float gridCellInterval = 2.5f;

    private static List<Type> combatAbilityComponentTypes = new List<Type>();

    private bool showAddCombatAbilityComponentsButtons;
    private bool hasValueInDictionary;
    private bool outOfRangeKeyInCastingRangeDictionary;
    private bool outOfRangeKeyInAOEDictionary;

    private CombatAbility combatAbilityData;
    private SerializedProperty combatAbilityIcon;
    private SerializedProperty combatAbilityName;
    private SerializedProperty combatAbilityDescription;
    private SerializedProperty combatAbilityType;
    private SerializedProperty staminaCost;
    private SerializedProperty castingRange;
    private SerializedProperty AOE;
    private SerializedProperty combatAbilityComponents;

    private void OnEnable()
    {
        combatAbilityData = target as CombatAbility;

        combatAbilityIcon = serializedObject.FindProperty("<combatAbilityIcon>k__BackingField");
        combatAbilityName = serializedObject.FindProperty("<combatAbilityName>k__BackingField");
        combatAbilityType = serializedObject.FindProperty("<combatAbilityType>k__BackingField");
        staminaCost = serializedObject.FindProperty("<staminaCost>k__BackingField");
        combatAbilityDescription = serializedObject.FindProperty("<combatAbilityDescription>k__BackingField");
        castingRange = serializedObject.FindProperty("<castingRange>k__BackingField");
        AOE = serializedObject.FindProperty("<AOE>k__BackingField");
        combatAbilityComponents = serializedObject.FindProperty("<combatAbilityComponents>k__BackingField");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        float currentViewWidth = EditorGUIUtility.currentViewWidth;
        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        
        combatAbilityIcon.objectReferenceValue = EditorGUILayout.ObjectField("Combat Ability Icon", combatAbilityIcon.objectReferenceValue, typeof(Sprite), false);
        EditorGUILayout.PropertyField(combatAbilityName, new GUIContent("Combat Ability Name"));
        EditorGUILayout.PropertyField(combatAbilityType, new GUIContent("Combat Ability Type"));
        if (combatAbilityType.intValue == 0)
        {
            EditorGUILayout.PropertyField(staminaCost, new GUIContent("Stamina Cost"));
        }
        EditorGUILayout.PropertyField(combatAbilityDescription, new GUIContent("Combat Ability Description"));

        EditorGUILayout.Space(singleLineHeight);
        EditorGUILayout.PropertyField(castingRange, new GUIContent("Casting Range"));
        #region Set Casting Range Dictionary
        for (int x = -combatAbilityData.castingRange.x; x <= combatAbilityData.castingRange.x; x++)
        {
            for (int y = -combatAbilityData.castingRange.y; y <= combatAbilityData.castingRange.y; y++)
            {
                for (int z = -combatAbilityData.castingRange.z; z <= combatAbilityData.castingRange.z; z++)
                {
                    if (x + y + z != 0) continue;

                    Vector3Int currentKey = new Vector3Int(x, y, z);

                    if (!combatAbilityData.castingRangeDictionary.ContainsKey(currentKey))
                    {
                        combatAbilityData.castingRangeDictionary.Add(currentKey, true);
                    }
                }
            }
        }
        #endregion
        #region Draw Casting Range Hexgrid
        int maxGridCellCount = Mathf.Max(Mathf.Abs(combatAbilityData.castingRange.x), Mathf.Abs(combatAbilityData.castingRange.z));
        float gridCellLength = Mathf.Min(maxGridCellLength, (currentViewWidth - gridCellInterval * (maxGridCellCount + 1.0f)) / (2.0f * maxGridCellCount + 1.0f));
        float centerXPos = currentViewWidth / 2.0f - gridCellLength;

        if (gridCellLength > gridCellInterval)
        {
            GUILayout.BeginVertical();

            for (int y = combatAbilityData.castingRange.y; y >= -combatAbilityData.castingRange.y; y--)
            {
                float minXPos = float.MaxValue;

                for (int x = -combatAbilityData.castingRange.x; x <= combatAbilityData.castingRange.x; x++)
                {
                    for (int z = combatAbilityData.castingRange.z; z >= -combatAbilityData.castingRange.z; z--)
                    {
                        if (x + y + z != 0) continue;

                        float currentXPos = 0.5f * x - 0.5f * z;
                        minXPos = Mathf.Min(minXPos, currentXPos);
                    }
                }

                GUILayout.BeginHorizontal();

                GUILayout.Space(centerXPos + minXPos * gridCellLength + Mathf.FloorToInt(minXPos) * gridCellInterval);

                for (int x = -combatAbilityData.castingRange.x; x <= combatAbilityData.castingRange.x; x++)
                {
                    for (int z = combatAbilityData.castingRange.z; z >= -combatAbilityData.castingRange.z; z--)
                    {
                        if (x + y + z != 0) continue;

                        Vector3Int currentKey = new Vector3Int(x, y, z);

                        if (combatAbilityData.castingRangeDictionary[currentKey] == true)
                        {
                            if (GUILayout.Button(gridCellTexture, GUILayout.Width(gridCellLength), GUILayout.Height(gridCellLength)))
                            {
                                combatAbilityData.castingRangeDictionary[currentKey] = false;

                                EditorUtility.SetDirty(combatAbilityData);
                            }
                        }
                        else
                        {
                            if (GUILayout.Button(emptyTexture, GUILayout.Width(gridCellLength), GUILayout.Height(gridCellLength)))
                            {
                                combatAbilityData.castingRangeDictionary[currentKey] = true;

                                EditorUtility.SetDirty(combatAbilityData);
                            }
                        }
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("Warning: Not enough inspector view width. Cannot show hexgrid.", MessageType.Warning);
        }
        #endregion
        #region Draw Designer Supporting Buttons
        hasValueInDictionary = false;

        foreach (Vector3Int key in combatAbilityData.castingRangeDictionary.Keys)
        {
            if (combatAbilityData.castingRangeDictionary[key])
            {
                hasValueInDictionary = true;
            }
        }

        if (hasValueInDictionary)
        {
            if (GUILayout.Button("Clear"))
            {
                foreach (Vector3Int key in combatAbilityData.castingRangeDictionary.Keys.ToList())
                {
                    combatAbilityData.castingRangeDictionary[key] = false;
                }

                EditorUtility.SetDirty(combatAbilityData);
            }
        }
        else
        {
            if (GUILayout.Button("Fill"))
            {
                foreach (Vector3Int key in combatAbilityData.castingRangeDictionary.Keys.ToList())
                {
                    combatAbilityData.castingRangeDictionary[key] = true;
                }

                EditorUtility.SetDirty(combatAbilityData);
            }
        }

        foreach (Vector3Int key in combatAbilityData.castingRangeDictionary.Keys)
        {
            if (!((-combatAbilityData.castingRange.x <= key.x && key.x <= combatAbilityData.castingRange.x) && (-combatAbilityData.castingRange.y <= key.y && key.y <= combatAbilityData.castingRange.y) && (-combatAbilityData.castingRange.z <= key.z && key.z <= combatAbilityData.castingRange.z)))
            {
                outOfRangeKeyInCastingRangeDictionary = true;
                break;
            }
        }

        if (outOfRangeKeyInCastingRangeDictionary)
        {
            if (GUILayout.Button("Organize"))
            {
                foreach (Vector3Int key in combatAbilityData.castingRangeDictionary.Keys.ToList())
                {
                    if (!((-combatAbilityData.castingRange.x <= key.x && key.x <= combatAbilityData.castingRange.x) && (-combatAbilityData.castingRange.y <= key.y && key.y <=      combatAbilityData.castingRange.y) && (-combatAbilityData.castingRange.z <= key.z && key.z <= combatAbilityData.castingRange.z)))
                    {
                        combatAbilityData.castingRangeDictionary.Remove(key);
                    }
                }
                outOfRangeKeyInCastingRangeDictionary = false;
            }
            EditorGUILayout.HelpBox("Warning: Dictionary has out of range key value. Click the button to clean up.", MessageType.Warning);
        }
        #endregion

        EditorGUILayout.Space(singleLineHeight);
        EditorGUILayout.PropertyField(AOE, new GUIContent("Area Of Effect"));
        #region Set AOE Dictionary
        for (int x = -combatAbilityData.AOE.x; x <= combatAbilityData.AOE.x; x++)
        {
            for (int y = -combatAbilityData.AOE.y; y <= combatAbilityData.AOE.y; y++)
            {
                for (int z = -combatAbilityData.AOE.z; z <= combatAbilityData.AOE.z; z++)
                {
                    if (x + y + z != 0) continue;

                    Vector3Int currentKey = new Vector3Int(x, y, z);

                    if (!combatAbilityData.AOEDictionary.ContainsKey(currentKey))
                    {
                        combatAbilityData.AOEDictionary.Add(currentKey, true);
                    }
                }
            }
        }
        #endregion
        #region Draw AOE Hexgrid
        maxGridCellCount = Mathf.Max(Mathf.Abs(combatAbilityData.AOE.x), Mathf.Abs(combatAbilityData.AOE.z));
        gridCellLength = Mathf.Min(maxGridCellLength, (currentViewWidth - gridCellInterval * (maxGridCellCount + 1.0f)) / (2.0f * maxGridCellCount + 1.0f));

        if (gridCellLength > gridCellInterval)
        {
            GUILayout.BeginVertical();

            for (int y = combatAbilityData.AOE.y; y >= -combatAbilityData.AOE.y; y--)
            {
                float minXPos = float.MaxValue;

                for (int x = -combatAbilityData.AOE.x; x <= combatAbilityData.AOE.x; x++)
                {
                    for (int z = combatAbilityData.AOE.z; z >= -combatAbilityData.AOE.z; z--)
                    {
                        if (x + y + z != 0) continue;

                        float currentXPos = 0.5f * x - 0.5f * z;
                        minXPos = Mathf.Min(minXPos, currentXPos);
                    }
                }

                GUILayout.BeginHorizontal();

                GUILayout.Space(centerXPos + minXPos * gridCellLength + Mathf.FloorToInt(minXPos) * gridCellInterval);

                for (int x = -combatAbilityData.AOE.x; x <= combatAbilityData.AOE.x; x++)
                {
                    for (int z = combatAbilityData.AOE.z; z >= -combatAbilityData.AOE.z; z--)
                    {
                        if (x + y + z != 0) continue;

                        Vector3Int currentKey = new Vector3Int(x, y, z);

                        if (combatAbilityData.AOEDictionary[currentKey] == true)
                        {
                            if (GUILayout.Button(gridCellTexture, GUILayout.Width(gridCellLength), GUILayout.Height(gridCellLength)))
                            {
                                combatAbilityData.AOEDictionary[currentKey] = false;

                                EditorUtility.SetDirty(combatAbilityData);
                            }
                        }
                        else
                        {
                            if (GUILayout.Button(emptyTexture, GUILayout.Width(gridCellLength), GUILayout.Height(gridCellLength)))
                            {
                                combatAbilityData.AOEDictionary[currentKey] = true;

                                EditorUtility.SetDirty(combatAbilityData);
                            }
                        }
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("Warning: Not enough inspector view width. Cannot show hexgrid.", MessageType.Warning);
        }
        #endregion
        #region Draw Designer Supporting Buttons
        hasValueInDictionary = false;

        foreach (Vector3Int key in combatAbilityData.AOEDictionary.Keys)
        {
            if (combatAbilityData.AOEDictionary[key])
            {
                hasValueInDictionary = true;
            }
        }

        if (hasValueInDictionary)
        {
            if (GUILayout.Button("Clear"))
            {
                foreach (Vector3Int key in combatAbilityData.AOEDictionary.Keys.ToList())
                {
                    combatAbilityData.AOEDictionary[key] = false;
                }

                EditorUtility.SetDirty(combatAbilityData);
            }
        }
        else
        {
            if (GUILayout.Button("Fill"))
            {
                foreach (Vector3Int key in combatAbilityData.AOEDictionary.Keys.ToList())
                {
                    combatAbilityData.AOEDictionary[key] = true;
                }

                EditorUtility.SetDirty(combatAbilityData);
            }
        }

        foreach (Vector3Int key in combatAbilityData.AOEDictionary.Keys)
        {
            if (!((-combatAbilityData.AOE.x <= key.x && key.x <= combatAbilityData.AOE.x) && (-combatAbilityData.AOE.y <= key.y && key.y <= combatAbilityData.AOE.y) && (-combatAbilityData.AOE.z <= key.z && key.z <= combatAbilityData.AOE.z)))
            {
                outOfRangeKeyInAOEDictionary = true;
                break;
            }
        }

        if (outOfRangeKeyInAOEDictionary)
        {
            if (GUILayout.Button("Organize"))
            {
                foreach (Vector3Int key in combatAbilityData.AOEDictionary.Keys.ToList())
                {
                    if (!((-combatAbilityData.AOE.x <= key.x && key.x <= combatAbilityData.AOE.x) && (-combatAbilityData.AOE.y <= key.y && key.y <=      combatAbilityData.AOE.y) && (-combatAbilityData.AOE.z <= key.z && key.z <= combatAbilityData.AOE.z)))
                    {
                        combatAbilityData.AOEDictionary.Remove(key);
                    }
                }
                outOfRangeKeyInAOEDictionary = false;
            }
            EditorGUILayout.HelpBox("Warning: Dictionary has out of range key value. Click the button to clean up.", MessageType.Warning);
        }
        #endregion

        EditorGUILayout.Space(singleLineHeight);
        EditorGUILayout.PropertyField(combatAbilityComponents, new GUIContent("Combat Ability Components"));

        showAddCombatAbilityComponentsButtons = EditorGUILayout.Foldout(showAddCombatAbilityComponentsButtons, "Add Combat Ability Components");

        if (showAddCombatAbilityComponentsButtons)
        {
            foreach (Type combatAbilityComponentType in combatAbilityComponentTypes)
            {
                if (GUILayout.Button(combatAbilityComponentType.Name))
                {
                    CombatAbilityComponent combatAbilityComponent = Activator.CreateInstance(combatAbilityComponentType) as CombatAbilityComponent;

                    if (combatAbilityComponent == null)
                    {
                        Debug.LogError($"Tried to add Combat Ability Component of type \"{combatAbilityComponentType.Name}\", but failed to create instance.");
                    }
                    else
                    {
                        combatAbilityData.AddComponent(combatAbilityComponent);

                        EditorUtility.SetDirty(combatAbilityData);
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

    [DidReloadScripts]
    private static void OnRecompile()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        IEnumerable<Type> types = assemblies.SelectMany(assembly => assembly.GetTypes());
        IEnumerable<Type> filteredTypes = types.Where(type => type.IsSubclassOf(typeof(CombatAbilityComponent)) && type.IsClass && !type.ContainsGenericParameters);
        combatAbilityComponentTypes = filteredTypes.ToList();
    }
}
