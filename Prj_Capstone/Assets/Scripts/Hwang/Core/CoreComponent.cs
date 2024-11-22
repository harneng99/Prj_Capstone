using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Core components can't use IPointer event functions because it does not have its collider.
public abstract class CoreComponent : MonoBehaviour
{
    protected Entity entity;
    protected const float epsilon = 0.001f;

    protected virtual void Awake()
    {
        entity = GetComponentInParent<Entity>();

        entity.onPointerClick += OnPointerClick;
    }

    protected abstract void OnPointerClick(PointerEventData eventData);
}
