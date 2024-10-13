using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombatAbilityButton : PooledObject, IPointerEnterHandler, IPointerExitHandler
{
    public Entity entity { get; set; }
    public CombatAbility combatAbility { get; set; }

    private GameObject combatAbilityDescriptionPrefab;

    public void ToggleCombatAbilityCastRange()
    {
        if (Manager.Instance.gameManager.isAiming)
        {
            Manager.Instance.gameManager.isAiming = false;
            Manager.Instance.gameManager.isAimingCopyForFunctionExecutionOrderCorrection = false;
            entity.entityCombat.currentSelectedCombatAbility = null;
            entity.highlightedTilemap.ClearAllTiles();
        }
        else
        {
            Manager.Instance.gameManager.isAiming = true;
            Manager.Instance.gameManager.isAimingCopyForFunctionExecutionOrderCorrection = true;
            entity.entityCombat.currentSelectedCombatAbility = combatAbility;
            entity.entityCombat.DrawCastingRange(combatAbility);
        }
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector2 mousePosition = Manager.Instance.playerInputManager.GetMousePosition();
        combatAbilityDescriptionPrefab = Manager.Instance.objectPoolingManager.GetGameObject("Combat Ability Description");
        RectTransform descriptionRectTransform = combatAbilityDescriptionPrefab.GetComponent<RectTransform>();
        descriptionRectTransform.SetParent(transform);
        TMP_Text combatAbilityDescriptionText = combatAbilityDescriptionPrefab.GetComponentInChildren<TMP_Text>();
        descriptionRectTransform.position = mousePosition;
        Debug.Log("Mouse Position: " + mousePosition + ", rectTransform.position: " + descriptionRectTransform.position);
        combatAbilityDescriptionText.text = combatAbility.combatAbilityDescription;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        combatAbilityDescriptionPrefab.GetComponent<PooledObject>().ReleaseObject();
    }
}
