using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PlayerCombat : Combat
{
    private PlayerCharacter mercenary;
    private List<Vector3Int> selectedCellgridPositions = new List<Vector3Int>();

    protected override void Awake()
    {
        base.Awake();

        mercenary = entity as PlayerCharacter;
    }

    protected override void Start()
    {
        base.Start();

        Manager.Instance.playerInputManager.controls.Map.MouseLeftClick.performed += _ => MouseLeftClick();
        Manager.Instance.playerInputManager.controls.Map.MouseRightClick.performed += _ => MouseRightClick();
    }

    protected override void Update()
    {
        if (Manager.Instance.gameManager.battlePhase)
        {
            if (entity.isSelected && currentSelectedCombatAbility != null)
            {
                Vector3Int nextCellgridPosition = Manager.Instance.playerInputManager.GetMousePosition(GridType.Cellgrid);

                if (currentMouseCellgridPosition != nextCellgridPosition && selectedCellgridPositions.Count < currentSelectedCombatAbility.maximumCastingAreaCount)
                {
                    currentMouseCellgridPosition = nextCellgridPosition;

                    if (!Manager.Instance.gameManager.OutOfRange(currentMouseCellgridPosition))
                    {
                        TileBase tileBase = entity.highlightedTilemap.GetTile(currentMouseCellgridPosition);

                        if (tileBase != null && tileBase.Equals(combatAbilityRangeHighlightedTileBase))
                        {
                            DrawAOE(nextCellgridPosition, currentSelectedCombatAbility);
                        }
                        else
                        {
                            DrawAOE(Vector3Int.zero, null);
                        }
                    }
                    else
                    {
                        DrawAOE(Vector3Int.zero, null);
                    }
                }

                foreach (Vector3Int cellgridPosition in selectedCellgridPositions)
                {
                    DrawAOE(cellgridPosition, currentSelectedCombatAbility, true);
                }
            }
        }
    }

    protected override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button.Equals(PointerEventData.InputButton.Left))
        {
            if (Manager.Instance.gameManager.battlePhase && Manager.Instance.gameManager.playerPhase)
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
            selectedCellgridPositions.Remove(entity.entityMovement.currentCellgridPosition);
        }
    }

    public override void MouseRightClick()
    {
        if (Manager.Instance.gameManager.battlePhase)
        {
            if (entity.isSelected && currentSelectedCombatAbility != null)
            {
                if (!Manager.Instance.playerInputManager.IsPointerOverEntity())
                {
                    if (currentSelectedCombatAbility.maximumCastingAreaCount == 1)
                    {
                        currentSelectedCombatAbility = null;
                        Manager.Instance.gameManager.isAiming = false;
                        Manager.Instance.gameManager.isAimingCopyForFunctionExecutionOrderCorrection = false;
                        DrawCastingRange(null);
                        DrawAOE(Vector3Int.zero, null);
                        selectedCellgridPositions.Clear();
                    }
                    else
                    {
                        if (selectedCellgridPositions.Contains(Manager.Instance.playerInputManager.GetMousePosition(GridType.Cellgrid)))
                        {
                            selectedCellgridPositions.Remove(Manager.Instance.playerInputManager.GetMousePosition(GridType.Cellgrid));
                        }
                        else
                        {
                            currentSelectedCombatAbility = null;
                            Manager.Instance.gameManager.isAiming = false;
                            Manager.Instance.gameManager.isAimingCopyForFunctionExecutionOrderCorrection = false;
                            DrawCastingRange(null);
                            DrawAOE(Vector3Int.zero, null);
                            selectedCellgridPositions.Clear();
                        }
                    }
                }
            }
        }
    }

    public void MouseLeftClick()
    {
        if (!Manager.Instance.playerInputManager.IsPointerOverUI())
        {
            if (Manager.Instance.gameManager.battlePhase && Manager.Instance.gameManager.playerPhase)
            {
                if (aoeTilemap.HasTile(currentMouseCellgridPosition) && currentSelectedCombatAbility != null)
                {
                    if (mercenary.entityStat.currentlyAppliedStatusEffect.HasFlag(StatusEffect.Paralyze) || mercenary.entityStat.currentlyAppliedStatusEffect.HasFlag(StatusEffect.Stun) || mercenary.entityStat.currentlyAppliedStatusEffect.HasFlag(StatusEffect.Horror))
                    {
                        Manager.Instance.uiManager.ShowWarningUI("Warning: Selected mercenary can't attack.");
                        return;
                    }

                    if (entity.entityStat.stamina.currentValue < currentSelectedCombatAbility.staminaCost)
                    {
                        Manager.Instance.uiManager.ShowWarningUI("Warning: Not enough stamina.");
                    }
                    else
                    {
                        if (selectedCellgridPositions.Count == currentSelectedCombatAbility.maximumCastingAreaCount)
                        {
                            foreach (Vector3Int cellgridPosition in selectedCellgridPositions)
                            {
                                ExecuteCombatAbility(cellgridPosition, GridType.Cellgrid, currentSelectedCombatAbility);
                            }
                            selectedCellgridPositions.Clear();

                            Manager.Instance.gameManager.isAiming = false;
                            Manager.Instance.uiManager.HideSideInformationUI();
                        }
                        else if (selectedCellgridPositions.Count < currentSelectedCombatAbility.maximumCastingAreaCount && !selectedCellgridPositions.Contains(currentMouseCellgridPosition))
                        {
                            if (CheckAbilityCondition(currentMouseCellgridPosition, GridType.Cellgrid, currentSelectedCombatAbility))
                            {
                                selectedCellgridPositions.Add(currentMouseCellgridPosition);
                                DrawAOE(currentMouseCellgridPosition, currentSelectedCombatAbility);
                            }
                            else
                            {
                                Manager.Instance.uiManager.ShowWarningUI("Warning: Unavailable Target.");
                            }
                        }
                    }
                }
            }
        }
    }
}
