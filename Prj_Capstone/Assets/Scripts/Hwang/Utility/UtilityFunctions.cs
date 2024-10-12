using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class UtilityFunctions
{
    public static T GetComponentInChildren<T>(this GameObject obj, bool includeInactive = false, bool includeParent = true) where T : Component
    {
        var components = obj.GetComponentsInChildren<T>(includeInactive);

        if (includeParent)
            return components.FirstOrDefault();

        return components.FirstOrDefault(childComponent => childComponent.transform != obj.transform);
    }

    public static T[] GetComponentsInChildren<T>(this GameObject obj, bool includeInactive = false, bool includeParent = true) where T : Component
    {
        var components = obj.GetComponentsInChildren<T>(includeInactive);

        if (includeParent)
            return components;

        return components.Where(childComponent => childComponent.transform != obj.transform).ToArray();
    }

    public static bool IsTilemapEmpty(Tilemap tilemap)
    {
        BoundsInt bounds = tilemap.cellBounds;

        foreach (var position in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(position))
            {
                return false;
            }
        }

        return true;
    }
}