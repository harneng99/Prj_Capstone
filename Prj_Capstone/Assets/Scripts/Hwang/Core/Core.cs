using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Core : MonoBehaviour
{
    private List<CoreComponent> coreComponents = new List<CoreComponent>();

    private void Awake()
    {
        coreComponents = GetComponentsInChildren<CoreComponent>().ToList();
    }

    public T GetCoreComponent<T>() where T : CoreComponent
    {
        T component = coreComponents.OfType<T>().FirstOrDefault();

        if (component == null)
        {
            Debug.LogWarning("No core component of type: " + typeof(T).Name + " found in " + transform.parent.name);
        }

        return component;
    }
}
