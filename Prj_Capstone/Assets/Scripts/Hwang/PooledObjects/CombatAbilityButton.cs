using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombatAbilityButton : PooledObject
{
    [HideInInspector] public Entity entity;
    [HideInInspector] public CombatAbility combatAbility;

    private GameObject combatAbilityDescriptionPrefab;

    public void ShowCombatAbilityCastRange()
    {
        entity.entityCombat.currentSelectedCombatAbility = combatAbility;
        entity.entityCombat.combatAbilityRangeHighlightedTilemap.ClearAllTiles();

        foreach (Vector3Int rangeHexgridOffset in combatAbility.castingRangeDictionary.Keys)
        {
            if (combatAbility.castingRangeDictionary[rangeHexgridOffset] == false) continue;

            Vector3Int currentRangeHexgrid = entity.entityMovement.currentHexgridPosition + rangeHexgridOffset;
            Vector3Int? currentRangeCellgrid = entity.entityMovement.pathfinder.HexgridToCellgrid(currentRangeHexgrid);

            if (!currentRangeCellgrid.HasValue) continue;

            GridNode currentGridNode = entity.entityMovement.pathfinder.hexgridNodes.FirstOrDefault(node => node.cellgridPosition == currentRangeCellgrid.Value);

            if (currentGridNode != null && !currentGridNode.isObstacle)
            {
                entity.entityCombat.combatAbilityRangeHighlightedTilemap.SetTile(currentRangeCellgrid.Value, entity.entityCombat.combatAbilityRangeHighlightedTileBase);
            }
        }
    }

    public void OnPointerEnter()
    {
        Vector2 mousePosition = entity.inputHandler.controls.Map.MousePosition.ReadValue<Vector2>();
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
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
