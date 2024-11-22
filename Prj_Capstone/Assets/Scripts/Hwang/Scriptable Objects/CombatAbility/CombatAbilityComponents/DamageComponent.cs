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
        if (entity.GetType().Equals(typeof(Player)))
        {
            if (target.GetType().Equals(typeof(Enemy)))
            { 
                target.entityStat.health.DecreaseCurrentValue((baseHealthDamage + healthDamageIncreaseByLevel * entity.level) * entity.entityStat.attackMultiplier.currentValue * (1 - target.entityStat.defenseMultiplier.currentValue));
            }
        }
        else if (entity.GetType().Equals(typeof(Enemy)))
        {
            if (target.GetType().Equals(typeof(Player)))
            {
                target.entityStat.health.DecreaseCurrentValue((baseHealthDamage + healthDamageIncreaseByLevel * entity.level) * entity.entityStat.attackMultiplier.currentValue * (1 - target.entityStat.defenseMultiplier.currentValue));
            }
        }
    }

    public override float GetEnemyAIScore(Entity target)
    {
        Enemy enemy = entity as Enemy;

        if (enemy == null) return 0.0f;

        if (target.GetType().Equals(typeof(Player)))
        {
            if (target.entityStat.health.currentValue <= (baseHealthDamage + healthDamageIncreaseByLevel * entity.level) * entity.entityStat.attackMultiplier.currentValue * (1 - target.entityStat.defenseMultiplier.currentValue))
            {
                return enemy.enemyCombat.mercenaryKill;
            }
            else
            {
                return ((baseHealthDamage + healthDamageIncreaseByLevel * entity.level) * entity.entityStat.attackMultiplier.currentValue * (1 - target.entityStat.defenseMultiplier.currentValue)) / target.entityStat.health.maxValue * enemy.enemyCombat.mercenaryDamage;
            }
        }

        return 0.0f;
    }
}
