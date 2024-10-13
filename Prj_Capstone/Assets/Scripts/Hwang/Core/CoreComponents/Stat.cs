using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

public class Stat : CoreComponent
{
    [field: SerializeField] public StatComponent health { get; protected set; }
    [field: SerializeField] public StatComponent stamina { get; protected set; }
    [field: SerializeField] public StatComponent attackMultiplier { get; protected set; }
    [field: SerializeField] public StatComponent damageMultiplier { get; protected set; }

    protected override void Awake()
    {
        base.Awake();

        #region Stat Initialization
        IEnumerable<PropertyInfo> statComponentProperties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(property => property.PropertyType.Equals(typeof(StatComponent)));

        foreach (PropertyInfo property in statComponentProperties)
        {
            StatComponent statComponent = property.GetValue(this) as StatComponent;
            MethodInfo initMethod = typeof(StatComponent).GetMethod("Init");
            initMethod.Invoke(statComponent, null);
        }
        #endregion
    }

    protected void Start()
    {
        health.OnCurrentValueMin += EntityDeath;
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
}
