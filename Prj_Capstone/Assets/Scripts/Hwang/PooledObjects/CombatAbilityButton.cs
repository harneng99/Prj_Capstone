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
        GameObject combatAbilityDescriptionPopup = Manager.Instance.uiManager.combatAbilityDescriptionPopup;
        combatAbilityDescriptionPopup.SetActive(true);
        TMP_Text[] combatAbilityDescriptionText = combatAbilityDescriptionPopup.GetComponentsInChildren<TMP_Text>();
        combatAbilityDescriptionText[0].text = combatAbility.name;
        combatAbilityDescriptionText[1].text = combatAbility.combatAbilityDescription;
        // combatAbilityDescriptionPrefab = Manager.Instance.objectPoolingManager.GetGameObject("Combat Ability Description");
        // RectTransform descriptionRectTransform = combatAbilityDescriptionPopup.GetComponent<RectTransform>();
        // descriptionRectTransform.SetParent(transform);
        // descriptionRectTransform.position = mousePosition;
        // Debug.Log("Mouse Position: " + mousePosition + ", rectTransform.position: " + descriptionRectTransform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // combatAbilityDescriptionPrefab.GetComponent<PooledObject>().ReleaseObject();
        Manager.Instance.uiManager.combatAbilityDescriptionPopup.SetActive(false);
    }
}
