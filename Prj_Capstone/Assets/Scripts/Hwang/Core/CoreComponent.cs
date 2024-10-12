using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Core components can't use IPointer event functions because it does not have its collider.
public class CoreComponent : MonoBehaviour
{
    protected Entity entity;
    protected const float epsilon = 0.001f;

    protected virtual void Awake()
    {
        entity = GetComponentInParent<Entity>();
    }
}
