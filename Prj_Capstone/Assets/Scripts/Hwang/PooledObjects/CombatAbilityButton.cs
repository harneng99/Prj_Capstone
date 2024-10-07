using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombatAbilityButton : PooledObject
{
    public Entity entity { get; set; }
    public CombatAbility combatAbility { get; set; }

    private GameObject combatAbilityDescriptionPrefab;

    public void ShowCombatAbilityCastRange()
    {
        entity.entityCombat.currentSelectedCombatAbility = combatAbility;
        entity.entityCombat.DrawCastingRange(combatAbility);
    }

    public void OnPointerEnter()
    {
        Vector2 mousePosition = Manager.Instance.gameManager.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        combatAbilityDescriptionPrefab = Manager.Instance.objectPoolingManager.GetGameObject("Combat Ability Description");
        RectTransform descriptionRectTransform = combatAbilityDescriptionPrefab.GetComponent<RectTransform>();
        descriptionRectTransform.SetParent(transform);
        TMP_Text combatAbilityDescriptionText = combatAbilityDescriptionPrefab.GetComponentInChildren<TMP_Text>();
        descriptionRectTransform.position = mousePosition;
        combatAbilityDescriptionText.text = combatAbility.combatAbilityDescription;
    }

    public void OnPointerExit()
    {
        combatAbilityDescriptionPrefab.GetComponent<PooledObject>().ReleaseObject();
    }
}
