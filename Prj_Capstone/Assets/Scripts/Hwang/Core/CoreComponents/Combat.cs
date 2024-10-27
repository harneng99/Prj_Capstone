using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public abstract class Combat : CoreComponent
{
    [field: SerializeField] public TileBase combatAbilityRangeHighlightedTileBase { get; private set; }
    [field: SerializeField] public TileBase combatAbilityAOEHighlightedTileBase { get; private set; }
    public CombatAbility currentSelectedCombatAbility { get; set; }
    public bool isCasting { get; private set; } // 정신집중
    public bool isAttacking { get; private set; } // 공격 애니메이션 종료
    
    [field: SerializeField] public List<CombatAbility> combatAbilities { get; protected set; }
    public List<GameObject> combatAbilityButtons { get; protected set; } = new List<GameObject>();
    protected Tilemap aoeTilemap;
    protected Canvas canvas;
    protected Vector3Int currentMouseCellgridPosition;

    protected override void Awake()
    {
        base.Awake();

        canvas = GameObject.FindWithTag("MainCanvas").GetComponent<Canvas>();
        aoeTilemap = GameObject.FindWithTag("AOETilemap").GetComponent<Tilemap>();
    }

    protected virtual void Start()
    {
        combatAbilityButtons = Manager.Instance.uiManager.GenerateCombatAbilityButtons(entity);
        ToggleCombatAbilityButtons(false);

        Manager.Instance.playerInputManager.controls.Map.MouseRightClick.performed += _ => MouseRightClick();
    }

    protected virtual void Update()
    {
        if (Manager.Instance.gameManager.battlePhase)
        {
            if (entity.isSelected && currentSelectedCombatAbility != null)
            {
                Vector3Int nextCellgridPosition = Manager.Instance.playerInputManager.GetMousePosition(GridType.Cellgrid);

                if (currentMouseCellgridPosition != nextCellgridPosition)
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
            }
        }
    }

    protected abstract void MouseRightClick();

    protected override void OnPointerClick(PointerEventData eventData) { }

    public void DrawCastingRange(CombatAbility combatAbility)
    {
        entity.highlightedTilemap.ClearAllTiles();

        if (combatAbility != null)
        {
            foreach (Vector3Int rangeHexgridOffset in combatAbility.castingRangeDictionary.Keys)
            {
                if (combatAbility.castingRangeDictionary[rangeHexgridOffset] == false) continue;

                Vector3Int currentRangeHexgrid = entity.entityMovement.currentHexgridPosition + rangeHexgridOffset;
                Vector3Int currentRangeCellgrid = entity.entityMovement.pathfinder.HexgridToCellgrid(currentRangeHexgrid);

                // if (!entity.entityMovement.pathfinder.moveableTilemap.GetTile(currentRangeCellgrid)) continue;
                if (Manager.Instance.gameManager.OutOfRange(currentRangeCellgrid)) continue;

                GridNode currentGridNode = entity.entityMovement.pathfinder.gridNodes.FirstOrDefault(node => node.cellgridPosition == currentRangeCellgrid);

                if (currentGridNode != null && !currentGridNode.isObstacle)
                {
                    entity.highlightedTilemap.SetTile(currentRangeCellgrid, entity.entityCombat.combatAbilityRangeHighlightedTileBase);
                }
            }
        }
    }

    public void DrawAOE(Vector3Int cellgridCenterPosition, CombatAbility combatAbility, bool addOn = false)
    {
        if (!addOn)
        {
            aoeTilemap.ClearAllTiles();
        }

        if (combatAbility != null)
        {
            foreach (Vector3Int rangeHexgridOffset in combatAbility.AOEDictionary.Keys)
            {
                if (combatAbility.AOEDictionary[rangeHexgridOffset] == false) continue;

                // Vector3Int currentRangeHexgrid = entity.entityMovement.pathfinder.CellgridToHexgrid(currentMouseCellgridPosition) + rangeHexgridOffset;
                Vector3Int currentRangeCellgrid = cellgridCenterPosition + entity.entityMovement.pathfinder.HexgridToCellgrid(rangeHexgridOffset);

                // if (!entity.entityMovement.pathfinder.moveableTilemap.GetTile(currentRangeCellgrid)) continue;
                if (Manager.Instance.gameManager.OutOfRange(currentRangeCellgrid)) continue;

                GridNode currentGridNode = entity.entityMovement.pathfinder.gridNodes.FirstOrDefault(node => node.cellgridPosition == currentRangeCellgrid);

                if (currentGridNode != null && !currentGridNode.isObstacle)
                {
                    aoeTilemap.SetTile(currentRangeCellgrid, entity.entityCombat.combatAbilityRangeHighlightedTileBase);
                }
            }
        }
    }

    protected void ExecuteCombatAbility(Vector3Int combatAbilityCenterGridPosition, GridType gridType, CombatAbility selectedCombatAbility)
    {
        Vector3Int combatAbilityCenterHexgridPosition = gridType.Equals(GridType.Cellgrid) ? entity.entityMovement.pathfinder.CellgridToHexgrid(combatAbilityCenterGridPosition) : combatAbilityCenterGridPosition;

        foreach (Vector3Int rangeHexgridOffset in selectedCombatAbility.AOEDictionary.Keys)
        {
            if (selectedCombatAbility.AOEDictionary[rangeHexgridOffset] == false) continue;

            Vector3Int currentRangeHexgrid = combatAbilityCenterHexgridPosition + rangeHexgridOffset;

            // TODO: Change ApplyCombatAbility Function to be independent from its parameter type

            foreach (Entity entity in Manager.Instance.gameManager.entities)
            {
                if (entity.entityMovement.currentHexgridPosition.Equals(currentRangeHexgrid))
                {
                    foreach (CombatAbilityComponent combatAbilityComponent in selectedCombatAbility.combatAbilityComponents)
                    {
                        combatAbilityComponent.ApplyCombatAbility(entity);
                    }
                }
            }
        }

        entity.entityStat.stamina.DecreaseCurrentValue(selectedCombatAbility.staminaCost);
        DrawAOE(Vector3Int.zero, null);
        DrawCastingRange(null);

        if (entity.isSelected)
        {
            Manager.Instance.uiManager.SetInformationUI(entity, entity.entityDescription, entity.entityMovement.currentCellgridPosition);
        }
    }

    protected bool CheckAbilityCondition(Vector3Int combatAbilityCenterGridPosition, GridType gridType, CombatAbility selectedCombatAbility)
    {
        Vector3Int combatAbilityCenterHexgridPosition = gridType.Equals(GridType.Cellgrid) ? entity.entityMovement.pathfinder.CellgridToHexgrid(combatAbilityCenterGridPosition) : combatAbilityCenterGridPosition;

        foreach (Vector3Int rangeHexgridOffset in selectedCombatAbility.AOEDictionary.Keys)
        {
            if (selectedCombatAbility.AOEDictionary[rangeHexgridOffset] == false) continue;

            Vector3Int currentRangeHexgrid = combatAbilityCenterHexgridPosition + rangeHexgridOffset;

            foreach (Entity entity in Manager.Instance.gameManager.entities)
            {
                if (entity.isActiveAndEnabled && entity.entityMovement.currentHexgridPosition.Equals(currentRangeHexgrid))
                {
                    if (selectedCombatAbility.availableTarget.HasFlag(AvailableTarget.Enemy) && entity.GetType().Equals(typeof(Enemy)))
                    {
                        return true;
                    }

                    if (selectedCombatAbility.availableTarget.HasFlag(AvailableTarget.Self) && entity.Equals(this.entity))
                    {
                        return true;
                    }

                    if (selectedCombatAbility.availableTarget.HasFlag(AvailableTarget.Mercenary) && entity.GetType().Equals(typeof(PlayerCharacter)))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    protected void ToggleCombatAbilityButtons(bool setActive)
    {
        foreach (GameObject combatAbilityButton in combatAbilityButtons)
        {
            combatAbilityButton.SetActive(setActive);
        }
    }
}
