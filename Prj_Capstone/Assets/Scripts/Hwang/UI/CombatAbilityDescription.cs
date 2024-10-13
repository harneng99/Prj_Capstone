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
        float pivotX = Manager.Instance.playerInputManager.GetMousePosition().x / Screen.width;
        float pivotY = Manager.Instance.playerInputManager.GetMousePosition().y / Screen.height;

        combatAbilityDescriptionRectTransform.pivot = new Vector3(pivotX, pivotY);
    }
}
