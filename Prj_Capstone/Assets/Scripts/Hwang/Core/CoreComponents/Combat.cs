using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Combat : CoreComponent
{
    [field: SerializeField] public TileBase combatAbilityRangeHighlightedTileBase { get; private set; }
    [field: SerializeField] public TileBase combatAbilityAOEHighlightedTileBase { get; private set; }
    public CombatAbility currentSelectedCombatAbility { get; set; }
    public bool isCasting { get; private set; } // 정신집중
    public bool isAttacking { get; private set; } // 공격 애니메이션 종료
    
    [SerializeField] protected List<CombatAbility> combatAbilities;
    protected Tilemap aoeTilemap;
    protected List<GameObject> combatAbilityButtons = new List<GameObject>();
    protected Canvas canvas;
    protected Vector3Int currentMouseCellgridPosition;

    protected override void Awake()
    {
        base.Awake();

        entity.onPointerClick += () => { ToggleCombatAbilityButtons(); };
        canvas = GameObject.FindWithTag("MainCanvas").GetComponent<Canvas>();
        aoeTilemap = GameObject.FindWithTag("AOETilemap").GetComponent<Tilemap>();
    }

    protected virtual void Start()
    {
        GenerateCombatAbilityButtons();
        ToggleCombatAbilityButtons(false);

        Manager.Instance.playerInputManager.controls.Map.MouseRightClick.performed += _ => MouseRightClick();
    }

    private void Update()
    {
        if (Manager.Instance.gameManager.battlePhase)
        {
            if (entity.isSelected && currentSelectedCombatAbility != null)
            {
                Vector3 mousePosition = Manager.Instance.playerInputManager.GetMousePosition();
                Vector3Int nextCellgridPosition = entity.highlightedTilemap.WorldToCell(mousePosition);

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

    protected virtual void MouseRightClick()
    {
        currentSelectedCombatAbility = null;
        Manager.Instance.gameManager.isAiming = false;
        ToggleCombatAbilityButtons(false);
    }

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

    protected bool ApplyCombatAbility(Vector3Int combatAbilityCenterGridPosition, GridType gridType, CombatAbility selectedCombatAbility)
    {
        bool hasTargetInRange = false;

        Vector3Int combatAbilityCenterHexgridPosition = gridType.Equals(GridType.Cellgrid) ? entity.entityMovement.pathfinder.CellgridToHexgrid(combatAbilityCenterGridPosition) : combatAbilityCenterGridPosition;

        foreach (Vector3Int rangeHexgridOffset in selectedCombatAbility.AOEDictionary.Keys)
        {
            if (selectedCombatAbility.AOEDictionary[rangeHexgridOffset] == false) continue;

            Vector3Int currentRangeHexgrid = combatAbilityCenterHexgridPosition + rangeHexgridOffset;

            foreach (Entity entity in Manager.Instance.gameManager.entities)
            {
                if (!entity.isActiveAndEnabled) continue;

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
            return true;
        }
        else
        {
            return false;
        }
    }

    protected void ToggleCombatAbilityButtons(bool setActive)
    {
        foreach (GameObject combatAbilityButton in combatAbilityButtons)
        {
            combatAbilityButton.SetActive(setActive);
        }
    }

    protected void ToggleCombatAbilityButtons()
    {
        foreach (GameObject combatAbilityButton in combatAbilityButtons)
        {
            combatAbilityButton.SetActive(!combatAbilityButton.activeSelf);
        }
    }

    private void GenerateCombatAbilityButtons()
    {
        for (int i = 0; i < combatAbilities.Count; i++)
        {
            GameObject combatAbilityButtonPrefab = Manager.Instance.objectPoolingManager.GetGameObject("Combat Ability Button");
            CombatAbilityButton combatAbilityButton = combatAbilityButtonPrefab.GetComponent<CombatAbilityButton>();
            RectTransform buttonRectTransform = combatAbilityButtonPrefab.GetComponent<RectTransform>();
            combatAbilityButtonPrefab.GetComponentsInChildren<Image>().Skip(1).ToList()[0].sprite = combatAbilities[i].combatAbilityIcon;

            combatAbilityButton.entity = entity;
            foreach (CombatAbilityComponent combatAbilityComponent in combatAbilities[i].combatAbilityComponents)
            {
                combatAbilityComponent.entity = entity;
            }
            combatAbilityButton.combatAbility = combatAbilities[i];
            buttonRectTransform.SetParent(canvas.transform);
            Vector3 currentLocalPosition = Manager.Instance.uiManager.buttonGenerateRectTransform.localPosition + Vector3.right * buttonRectTransform.sizeDelta.x * 1.5f * i;
            buttonRectTransform.localPosition = currentLocalPosition;
            combatAbilityButtons.Add(combatAbilityButtonPrefab);
        }
    }
}
