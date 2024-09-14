using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "newCombatAbilityData", menuName = "Data/Combat Ability Data")]
public class CombatAbility : ScriptableObject
{
    [field: SerializeField] public Sprite combatAbilityIcon { get; private set; }
    [field: SerializeField] public string combatAbilityName { get; private set; } = "Default Combat Ability Name";
    [field: SerializeField, TextArea] public string combatAbilityDescription { get; private set; } = "Default Combat Ability Description";
    [field: SerializeReference] public List<CombatAbilityComponent> combatAbilityComponents { get; private set; }

    public void AddComponent(CombatAbilityComponent componentData)
    {
        if (combatAbilityComponents.FirstOrDefault(type => type.GetType().Equals(componentData.GetType())) == null)
        {
            combatAbilityComponents.Add(componentData);
        }
    }
}
