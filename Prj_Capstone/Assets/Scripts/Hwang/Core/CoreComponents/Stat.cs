using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

public class Stat : CoreComponent
{
    // TODO: Change damage logic affected by multipliers
    [field: SerializeField] public StatComponent health { get; protected set; }
    [field: SerializeField] public StatComponent stamina { get; protected set; }
    [field: SerializeField] public StatComponent attackMultiplier { get; protected set; }
    [field: SerializeField] public StatComponent defenseMultiplier { get; protected set; }
    [field: SerializeField, PreventAdd, Tooltip("Warning: Status effects system is based on enum. You should NEVER change the order of the list!!!!!!")] public List<StatComponent> statusEffects { get; protected set; }
    [field: EnumFlags] public StatusEffect currentlyAppliedStatusEffect { get; protected set; }

    private void OnValidate()
    {
        if (statusEffects == null)
        {
            statusEffects = new List<StatComponent>();
        }

        int statusEffectCount = Enum.GetValues(typeof(StatusEffect)).Length;

        if (statusEffects.Count != statusEffectCount)
        {
            if (statusEffects.Count < statusEffectCount)
            {
                for (int index = statusEffects.Count; index < statusEffectCount; index++)
                {
                    StatComponent statusEffect = new StatComponent();
                    statusEffect.name = Enum.GetName(typeof(StatusEffect), index);
                    statusEffects.Add(statusEffect);
                }
            }
            else
            {
                statusEffects.RemoveRange(statusEffectCount, statusEffects.Count - statusEffectCount);
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        #region Stat Initialization
        IEnumerable<PropertyInfo> statComponentProperties = this.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.PropertyType.Equals(typeof(StatComponent)));

        foreach (PropertyInfo property in statComponentProperties)
        {
            StatComponent statComponent = property.GetValue(this) as StatComponent;
            MethodInfo initMethod = typeof(StatComponent).GetMethod("Init");
            initMethod.Invoke(statComponent, null);
            statComponent.entity = entity;
            statComponent.name = property.Name;
        }

        foreach (StatComponent statComponent in statusEffects)
        {
            statComponent.entity = entity;
            statComponent.Init();
        }
        #endregion
    }

    protected void Start()
    {
        health.OnCurrentValueMin += EntityDeath;

        /*for (int i = 0; i < Enum.GetValues(typeof(StatusEffect)).Length; i++)
        {
            int endTurn = 0;

            switch ((StatusEffect)Enum.GetValues(typeof(StatusEffect)).GetValue(i))
            {
                case StatusEffect.Stun:
                    endTurn = Manager.Instance.gameManager.currentTurnCount + 1; break;
                case StatusEffect.Poison:
                    endTurn = Manager.Instance.gameManager.currentTurnCount + 3; break;
                case StatusEffect.Taunt:
                    endTurn = Manager.Instance.gameManager.currentTurnCount + 2; break;
                case StatusEffect.Burn:
                    endTurn = Manager.Instance.gameManager.currentTurnCount + 2; break;
                case StatusEffect.Paralyze:
                    endTurn = Manager.Instance.gameManager.currentTurnCount + 2; break;
                case StatusEffect.Horror:
                    endTurn = Manager.Instance.gameManager.currentTurnCount + 1; break;
                default:
                    Debug.LogWarning("Unknown type of StatusEffect."); break;
            }

            statusEffects[i].OnCurrentValueMax += () => { 
                currentlyAppliedStatusEffect |= (StatusEffect)Enum.GetValues(typeof(StatusEffect)).GetValue(i);

                Action turnAction = null;
                turnAction = () =>
                {
                    if (Manager.Instance.gameManager.currentTurnCount > endTurn)
                    {
                        currentlyAppliedStatusEffect  &= ~(StatusEffect)Enum.GetValues(typeof(StatusEffect)).GetValue(i);
                        Manager.Instance.gameManager.playerTurnStart -= turnAction;
                    }
                };
                Manager.Instance.gameManager.playerTurnStart += turnAction;
            };

            if ((StatusEffect)Enum.GetValues(typeof(StatusEffect)).GetValue(i) == StatusEffect.Horror)
            {
                Action spreadHorror = null;
                spreadHorror = () =>
                {
                    if (Manager.Instance.gameManager.currentTurnCount <= endTurn)
                    {
                        for (int x = -2; x <= 2; x++)
                        {
                            for (int y = -2; y <= 2; y++)
                            {
                                for (int z = -2; z <= 2; z++)
                                {
                                    if (x + y + z != 0) continue;

                                    foreach (Entity entity in Manager.Instance.gameManager.entities)
                                    {
                                        if (entity.isActiveAndEnabled && entity.entityMovement.currentHexgridPosition.Equals(this.entity.entityMovement.currentHexgridPosition + new Vector3Int(x, y, z)))
                                        {
                                            entity.entityStat.statusEffects[(int)StatusEffect.Horror].IncreaseCurrentValue(100.0f);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Manager.Instance.gameManager.playerTurnStart -= spreadHorror;
                    }
                };

                Manager.Instance.gameManager.playerTurnStart += spreadHorror;
            }
        }*/
    }


    protected void EntityDeath()
    {
        entity.animator.SetTrigger("dead");
    }

    // Call this at the end of the entity death animation
    public void SetEntityInactive()
    {
        gameObject.SetActive(false);
    }

    protected override void OnPointerClick(PointerEventData eventData) { }

    public StatComponent GetStatComponent(string statName)
    {
        PropertyInfo property = GetType().GetProperty(statName);
        
        if (property != null && property.PropertyType.Equals(typeof(StatComponent)))
        {
            return (StatComponent)property.GetValue(this);
        }

        return null;
    }
}
