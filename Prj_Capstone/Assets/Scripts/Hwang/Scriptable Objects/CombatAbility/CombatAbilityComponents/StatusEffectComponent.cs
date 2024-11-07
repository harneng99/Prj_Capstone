using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Add status effect logic
public enum StatusEffect { Horror = 1 << 0, Poison = 1 << 1, Burn = 1 << 2, Stun = 1 << 3, Paralyze = 1 << 4, Taunt = 1 << 5 }

public class StatusEffectComponent : CombatAbilityComponent
{
    [field: SerializeField, EnumFlags] public StatusEffect statusEffects { get; private set; }
    [field: SerializeField, Range(0.0f, 100.0f)] public List<float> statusEffectValues { get; private set; }

    public override void ApplyCombatAbility(Entity target)
    {
        int currentStatusEffect = 0;

        foreach (StatusEffect statusEffect in Enum.GetValues(typeof(StatusEffect)))
        {
            if (statusEffects.HasFlag(statusEffect))
            {
                target.entityStat.statusEffects[currentStatusEffect].IncreaseCurrentValue(statusEffectValues[currentStatusEffect]);
            }
            currentStatusEffect += 1;
        }
    }
}
