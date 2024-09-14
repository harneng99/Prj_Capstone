using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

[CustomEditor(typeof(CombatAbility))]
public class CombatAbilityEditor : Editor
{
    private static List<Type> combatAbilityComponentTypes = new List<Type>();

    private CombatAbility combatAbilityData;

    private bool showAddCombatAbilityComponentsButtons;

    private void OnEnable()
    {
        combatAbilityData = target as CombatAbility;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

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
