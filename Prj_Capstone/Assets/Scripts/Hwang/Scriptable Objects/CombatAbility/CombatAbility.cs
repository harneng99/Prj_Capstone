using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SkillType { Active, Passive }

[CreateAssetMenu(fileName = "newCombatAbilityData", menuName = "Data/Combat Ability Data")]
public class CombatAbility : ScriptableObject
{
    [field: SerializeField] public Sprite combatAbilityIcon { get; private set; }
    [field: SerializeField] public string combatAbilityName { get; private set; } = "Default Combat Ability Name";
    [field: SerializeField] public SkillType combatAbilityType { get; private set; }
    [field: SerializeField] public int staminaCost { get; private set; }
    [field: SerializeField, TextArea] public string combatAbilityDescription { get; private set; } = "Default Combat Ability Description";
    [field: SerializeField, Tooltip("Describes the range that the entity can attack or cast current combat ability.")] public Vector3Int castingRange { get; private set; }
    public SerializedDictionary<Vector3Int, bool> castingRangeDictionary { get; private set; } = new SerializedDictionary<Vector3Int, bool>();
    [field: SerializeField, Tooltip("The area where the combat ability will be applied when casted. Currently not considering the entity's facing direction.")] public Vector3Int AOE { get; private set; }
    public SerializedDictionary<Vector3Int, bool> AOEDictionary { get; private set; } = new SerializedDictionary<Vector3Int, bool>();
    [field: SerializeReference] public List<CombatAbilityComponent> combatAbilityComponents { get; private set; }

    public void AddComponent(CombatAbilityComponent componentData)
    {
        if (combatAbilityComponents.FirstOrDefault(type => type.GetType().Equals(componentData.GetType())) == null)
        {
            combatAbilityComponents.Add(componentData);
        }
    }
}
