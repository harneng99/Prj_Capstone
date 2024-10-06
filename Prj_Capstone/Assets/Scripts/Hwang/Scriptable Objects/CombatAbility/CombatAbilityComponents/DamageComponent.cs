using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageComponent : CombatAbilityComponent
{
    public float baseHealthDamage;
    public float healthDamageIncreaseByLevel;
    public float pauseTimeWhenHit;

    public override void ApplyCombatAbility(Entity target)
    {
        // target.SendMessage("GetDamage", baseHealthDamage);
        if (entity.GetType().Equals(typeof(PlayerCharacter)))
        {
            if (target.GetType().Equals(typeof(Enemy)))
            { 
                target.entityStat.health.DecreaseCurrentValue(baseHealthDamage + healthDamageIncreaseByLevel * entity.level);
            }
        }
        else if (entity.GetType().Equals(typeof(Enemy)))
        {
            if (target.GetType().Equals(typeof(PlayerCharacter)))
            {
                target.entityStat.health.DecreaseCurrentValue(baseHealthDamage + healthDamageIncreaseByLevel * entity.level);
            }
        }
    }
}
