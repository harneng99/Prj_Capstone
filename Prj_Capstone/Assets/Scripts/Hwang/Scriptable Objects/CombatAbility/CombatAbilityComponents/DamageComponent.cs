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

    public override float GetEnemyAIScore(Entity target)
    {
        Enemy enemy = entity as Enemy;

        if (enemy == null) return 0.0f;

        if (target.GetType().Equals(typeof(PlayerCharacter)))
        {
            if (target.entityStat.health.currentValue <= baseHealthDamage + healthDamageIncreaseByLevel * entity.level)
            {
                return enemy.enemyCombat.mercenaryKill;
            }
            else
            {
                return (baseHealthDamage + healthDamageIncreaseByLevel * entity.level) / target.entityStat.health.maxValue * enemy.enemyCombat.mercenaryDamage;
            }
        }

        return 0.0f;
    }
}
