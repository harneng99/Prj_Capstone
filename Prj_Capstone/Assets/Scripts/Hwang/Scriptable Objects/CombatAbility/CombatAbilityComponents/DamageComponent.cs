using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageComponent : CombatAbilityComponent
{
    public float baseHealthDamage;
    public float healthDamageIncreaseByLevel;
    public float pauseTimeWhenHit;

    public override void ApplyCombatAbility(Collider2D target)
    {
        // target.SendMessage("GetDamage", baseHealthDamage);
    }
}
