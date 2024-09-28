using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class CombatAbilityComponent : ICombatAbility
{
    [SerializeField, HideInInspector] private string name = "";

    [HideInInspector] public Entity entity;

    public CombatAbilityComponent()
    {
        name = this.GetType().Name;
    }

    public abstract void ApplyCombatAbility(Collider2D target);

    public virtual void InitializeCombatAbilityData(int numberOfAttacks) { }
}