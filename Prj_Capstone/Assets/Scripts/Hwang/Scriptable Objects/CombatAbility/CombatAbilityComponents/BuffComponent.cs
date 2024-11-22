using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class BuffComponent : CombatAbilityComponent
{
    [field: SerializeField] public List<BuffEntry> buffEntries {  get; private set; }

    [System.Serializable]
    public class BuffEntry
    {
        [field: SerializeField] public string selectedStat { get; private set; }
        [field: SerializeField] public int durationTurn { get; private set; }
        [field: SerializeField] public bool constantValueChange { get; private set; }
        [field: SerializeField] public float valueChange { get; private set; }
    }

    public override void ApplyCombatAbility(Entity target)
    {
        foreach (BuffEntry entry in buffEntries)
        {
            StatComponent currentStatComponent = target.entityStat.GetStatComponent(entry.selectedStat);

            if (currentStatComponent == null)
            {
                Debug.LogWarning("Cannot find stat component: " + entry.selectedStat.ToString() + " for buff component of " + target.entityName);
            }

            if (entry.constantValueChange)
            {
                currentStatComponent.ReturnToOriginalValue(entry.valueChange * entry.durationTurn, entry.durationTurn);
                currentStatComponent.IncreaseCurrentValue(entry.valueChange, durationTurn: entry.durationTurn);
            }
            else
            {
                currentStatComponent.ReturnToOriginalValue(entry.valueChange, entry.durationTurn);
                currentStatComponent.IncreaseCurrentValue(entry.valueChange);
            }
        }
    }
}

