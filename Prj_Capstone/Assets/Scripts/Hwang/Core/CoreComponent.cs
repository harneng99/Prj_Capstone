using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreComponent : MonoBehaviour
{
    protected Entity entity;
    protected const float epsilon = 0.001f;

    protected virtual void Awake()
    {
        entity = GetComponentInParent<Entity>();
    }
}
