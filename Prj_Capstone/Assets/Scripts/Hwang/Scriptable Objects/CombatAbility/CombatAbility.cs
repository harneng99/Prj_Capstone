using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SkillType { Active, Passive }
[System.Flags] public enum AvailableTarget { Mercenary = 1 << 0, Enemy = 1 << 1, Self = 1 << 2 }

[CreateAssetMenu(fileName = "newCombatAbilityData", menuName = "Data/Combat Ability Data")]
public class CombatAbility : ScriptableObject
{
    [field: SerializeField] public Sprite combatAbilityIcon { get; private set; }
    [field: SerializeField] public string combatAbilityName { get; private set; } = "Default Combat Ability Name";
    [field: SerializeField] public SkillType combatAbilityType { get; private set; }
    [field: SerializeField] public int staminaCost { get; private set; }
    // TODO: Add logic to newly added combat ability components
    [field: SerializeField, Tooltip("How long should the combat ability has to be casted before actually applying it.")] public int castingTurnDuration { get; private set; }
    [field: SerializeField] public AvailableTarget availableTarget { get; private set; } = AvailableTarget.Enemy;
    [field: SerializeField, Tooltip("Number of castings that the entity can make.")] public int maximumCastingAreaCount { get; private set; } = 1;
    [field: SerializeField, TextArea] public string combatAbilityDescription { get; private set; } = "Default Combat Ability Description";
    [field: SerializeField, Tooltip("Describes the range that the entity can attack or cast current combat ability.")] public Vector3Int castingRange { get; private set; }
    [field: SerializeField] public SerializedDictionary<Vector3Int, bool> castingRangeDictionary { get; private set; }
    [field: SerializeField, Tooltip("The area where the combat ability will be applied when casted. Currently not considering the entity's facing direction.")] public Vector3Int AOE { get; private set; }
    [field: SerializeField] public SerializedDictionary<Vector3Int, bool> AOEDictionary { get; private set; }
    [field: SerializeReference] public List<CombatAbilityComponent> combatAbilityComponents { get; private set; }

    public void AddComponent(CombatAbilityComponent componentData)
    {
        if (combatAbilityComponents.FirstOrDefault(type => type.GetType().Equals(componentData.GetType())) == null)
        {
            combatAbilityComponents.Add(componentData);
        }
    }
}
