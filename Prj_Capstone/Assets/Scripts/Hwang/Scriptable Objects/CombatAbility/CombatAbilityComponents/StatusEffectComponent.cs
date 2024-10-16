using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Add status effect logic
public enum StatusEffect { horror, poison, burn, stunned, paralyze, taunted }

public class StatusEffectComponent : CombatAbilityComponent
{
    [field: SerializeField] public StatusEffect statusEffect { get; private set; }
    [field: SerializeField, Range(0.0f, 100.0f)] public float effectAmount;

    public override void ApplyCombatAbility(Entity target)
    {
        throw new System.NotImplementedException();
    }
}
