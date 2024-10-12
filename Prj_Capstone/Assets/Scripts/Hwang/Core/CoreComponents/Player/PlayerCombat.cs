using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : Combat
{
    protected override void Start()
    {
        base.Start();

        Manager.Instance.playerInputManager.controls.Map.MouseLeftClick.performed += _ => MouseLeftClick();
    }

    protected virtual void MouseLeftClick()
    {
        if (aoeTilemap.HasTile(currentMouseCellgridPosition))
        {
            if (entity.entityStat.stamina.currentValue < currentSelectedCombatAbility.staminaCost)
            {
                Manager.Instance.uiManager.ShowWarningUI("Warning: Not enough stamina.");
            }
            else
            {
                Manager.Instance.gameManager.isAiming = false;

                bool hasTargetInRange = ApplyCombatAbility(currentMouseCellgridPosition, GridType.Cellgrid, currentSelectedCombatAbility);

                if (!hasTargetInRange && !Manager.Instance.gameManager.isAiming)
                {
                    Manager.Instance.uiManager.ShowWarningUI("Warning: No target in range.");
                }

                /*foreach (Vector3Int rangeHexgridOffset in currentSelectedCombatAbility.AOEDictionary.Keys)
                {
                    if (currentSelectedCombatAbility.AOEDictionary[rangeHexgridOffset] == false) continue;

                    Vector3Int currentRangeCellgrid = currentMouseCellgridPosition + entity.entityMovement.pathfinder.HexgridToCellgrid(rangeHexgridOffset);

                    GridNode currentGridNode = entity.entityMovement.pathfinder.gridNodes.FirstOrDefault(node => node.cellgridPosition == currentRangeCellgrid);

                    if (currentGridNode != null && !currentGridNode.isObstacle)
                    {
                        foreach (Entity entity in Manager.Instance.gameManager.entities)
                        {
                            if (entity.isActiveAndEnabled && entity.entityMovement.currentCellgridPosition.Equals(currentRangeCellgrid))
                            {
                                hasTargetInRange = true;

                                foreach (CombatAbilityComponent combatAbilityComponent in currentSelectedCombatAbility.combatAbilityComponents)
                                {
                                    combatAbilityComponent.ApplyCombatAbility(entity);
                                }
                            }
                        }
                    }

                    if (!hasTargetInRange && !Manager.Instance.gameManager.isAiming)
                    {
                        Manager.Instance.uiManager.ShowWarningUI("Warning: No target in range.");
                    }
                }*/
            }
        }
    }
}
