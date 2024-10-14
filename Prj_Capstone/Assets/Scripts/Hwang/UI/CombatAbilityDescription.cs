using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAbilityDescription : PooledObject
{
    private RectTransform combatAbilityDescriptionRectTransform;

    private void Awake()
    {
        combatAbilityDescriptionRectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        /*float pivotX = Input.mousePosition.x / Screen.width;
        float pivotY = Input.mousePosition.y / Screen.height;*/

        combatAbilityDescriptionRectTransform.position = Input.mousePosition;
        if (!UtilityFunctions.IsFullyVisibleFrom(combatAbilityDescriptionRectTransform))
        {
            combatAbilityDescriptionRectTransform.pivot = Vector2.right;
        }
        // combatAbilityDescriptionRectTransform.pivot = new Vector3(pivotX, pivotY);
    }

    private void OnDisable()
    {
        combatAbilityDescriptionRectTransform.pivot = Vector2.zero;
    }
}
