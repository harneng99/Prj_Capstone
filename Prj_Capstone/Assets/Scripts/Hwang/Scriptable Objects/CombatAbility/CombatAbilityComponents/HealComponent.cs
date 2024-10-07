using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealComponent : CombatAbilityComponent
{
    public float baseRestoreValue;
    public float restoreValueIncreaseByLevel;

    public override void ApplyCombatAbility(Entity target)
    {
        if (target.GetType().Equals(entity.GetType()))
        {
            target.entityStat.health.IncreaseCurrentValue(baseRestoreValue + entity.level * restoreValueIncreaseByLevel);
        }
    }
}
