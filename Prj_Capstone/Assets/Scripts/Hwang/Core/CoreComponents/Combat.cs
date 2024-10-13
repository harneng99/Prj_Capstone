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

    private void Update()
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
                            aoeTilemap.ClearAllTiles();
                        }
                    }
                    else
                    {
                        aoeTilemap.ClearAllTiles();
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

    public void DrawAOE(Vector3Int cellgridCenterPosition, CombatAbility combatAbility)
    {
        aoeTilemap.ClearAllTiles();

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

    protected bool ExecuteCombatAbility(Vector3Int combatAbilityCenterGridPosition, GridType gridType, CombatAbility selectedCombatAbility)
    {
        bool hasTargetInRange = false;

        Vector3Int combatAbilityCenterHexgridPosition = gridType.Equals(GridType.Cellgrid) ? entity.entityMovement.pathfinder.CellgridToHexgrid(combatAbilityCenterGridPosition) : combatAbilityCenterGridPosition;

        foreach (Vector3Int rangeHexgridOffset in selectedCombatAbility.AOEDictionary.Keys)
        {
            if (selectedCombatAbility.AOEDictionary[rangeHexgridOffset] == false) continue;

            Vector3Int currentRangeHexgrid = combatAbilityCenterHexgridPosition + rangeHexgridOffset;

            // TODO: Change ApplyCombatAbility Function to be independent from its parameter type

            foreach (Entity entity in Manager.Instance.gameManager.entities)
            {
                if (!CheckAbilityCondition(entity, selectedCombatAbility)) continue;

                if (entity.entityMovement.currentHexgridPosition.Equals(currentRangeHexgrid))
                {
                    hasTargetInRange = true;

                    foreach (CombatAbilityComponent combatAbilityComponent in selectedCombatAbility.combatAbilityComponents)
                    {
                        combatAbilityComponent.ApplyCombatAbility(entity);
                    }
                }
            }
        }
        
        if (hasTargetInRange)
        {
            entity.entityStat.stamina.DecreaseCurrentValue(selectedCombatAbility.staminaCost);
            aoeTilemap.ClearAllTiles();
            return true;
        }
        else
        {
            return false;
        }
    }

    protected bool CheckAbilityCondition(Entity target, CombatAbility selectedCombatAbility)
    {
        if (!target.isActiveAndEnabled) return false;

        /*if (selectedCombatAbility.canBeUsedToSelf && target.Equals(entity)) return false;

        if (selectedCombatAbility.canBeUsedToAlly && target.GetType().Equals(entity.GetType())) return false;

        if (selectedCombatAbility.canBeUsedToEnemy && !target.GetType().Equals(entity.GetType())) return false;*/

        return true;
    }

    protected void ToggleCombatAbilityButtons(bool setActive)
    {
        foreach (GameObject combatAbilityButton in combatAbilityButtons)
        {
            combatAbilityButton.SetActive(setActive);
        }
    }
}
