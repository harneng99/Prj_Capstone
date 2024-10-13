using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCombat : Combat
{
    private PlayerCharacter mercenary;

    protected override void Awake()
    {
        base.Awake();

        mercenary = entity as PlayerCharacter;
    }

    protected override void Start()
    {
        base.Start();

        Manager.Instance.playerInputManager.controls.Map.MouseLeftClick.performed += _ => MouseLeftClick();
    }

    protected override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button.Equals(PointerEventData.InputButton.Left))
        {
            if (Manager.Instance.gameManager.battlePhase)
            {
                if (!Manager.Instance.gameManager.isAimingCopyForFunctionExecutionOrderCorrection)
                {
                    Manager.Instance.uiManager.SetCombatAbilityButtons();
                }
            }
        }
        else if (eventData.button.Equals(PointerEventData.InputButton.Right))
        {
            // TODO: Delete the designated target of the combat ability
        }
    }

    protected override void MouseRightClick()
    {
        if (Manager.Instance.gameManager)
        {
            if (entity.isSelected && currentSelectedCombatAbility != null && currentSelectedCombatAbility.maximumCastingAreaCount == 1)
            {
                currentSelectedCombatAbility = null;
                Manager.Instance.gameManager.isAiming = false;
                Manager.Instance.gameManager.isAimingCopyForFunctionExecutionOrderCorrection = false;
                entity.highlightedTilemap.ClearAllTiles();
                aoeTilemap.ClearAllTiles();
            }
        }
    }

    private void MouseLeftClick()
    {
        if (!Manager.Instance.playerInputManager.IsPointerOverUI())
        {
            if (Manager.Instance.gameManager.battlePhase)
            {
                if (aoeTilemap.HasTile(currentMouseCellgridPosition) && currentSelectedCombatAbility != null)
                {
                    if (entity.entityStat.stamina.currentValue < currentSelectedCombatAbility.staminaCost)
                    {
                        Manager.Instance.uiManager.ShowWarningUI("Warning: Not enough stamina.");
                    }
                    else
                    {
                        bool combatAbilityExecuted = ExecuteCombatAbility(currentMouseCellgridPosition, GridType.Cellgrid, currentSelectedCombatAbility);

                        if (!combatAbilityExecuted)
                        {
                            Manager.Instance.uiManager.ShowWarningUI("Warning: No target in range.");
                        }
                        else
                        {
                            Manager.Instance.gameManager.isAiming = false;
                            Manager.Instance.uiManager.HideSideInformationUI();
                        }
                    }
                }
            }
        }
    }
}
