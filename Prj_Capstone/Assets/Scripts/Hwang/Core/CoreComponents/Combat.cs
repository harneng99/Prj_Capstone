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
    
    [SerializeField] private List<CombatAbility> combatAbilities;
    private Tilemap aoeTilemap;
    private List<GameObject> combatAbilityButtons;
    private Canvas canvas;
    private Vector3Int currentMouseCellgridPosition;

    protected override void Awake()
    {
        base.Awake();

        entity.onPointerClick += GenerateCombatAbilityButtons;
        canvas = GameObject.FindWithTag("MainCanvas").GetComponent<Canvas>();
        aoeTilemap = GameObject.FindWithTag("AOETilemap").GetComponent<Tilemap>();
    }

    private void Start()
    {
        Manager.Instance.playerInputManager.controls.Map.MouseLeftClick.performed += _ => MouseLeftClick();
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
                // nextCellgridPosition = new Vector3Int(nextCellgridPosition.x, nextCellgridPosition.y, 0);

                if (currentMouseCellgridPosition != nextCellgridPosition)
                {
                    currentMouseCellgridPosition = nextCellgridPosition;

                    if (!Manager.Instance.gameManager.OutOfRange(currentMouseCellgridPosition))
                    {
                        TileBase tileBase = entity.highlightedTilemap.GetTile(currentMouseCellgridPosition);

                        if (tileBase.Equals(combatAbilityRangeHighlightedTileBase))
                        {
                            DrawAOE(currentSelectedCombatAbility);
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

    protected virtual void MouseLeftClick()
    {
        if (aoeTilemap.HasTile(currentMouseCellgridPosition))
        {
            foreach (Vector3Int rangeHexgridOffset in currentSelectedCombatAbility.AOEDictionary.Keys)
            {
                if (currentSelectedCombatAbility.AOEDictionary[rangeHexgridOffset] == false) continue;

                Vector3Int currentRangeHexgrid = entity.entityMovement.pathfinder.CellgridToHexgrid(currentMouseCellgridPosition) + rangeHexgridOffset;
                Vector3Int currentRangeCellgrid = entity.entityMovement.pathfinder.HexgridToCellgrid(currentRangeHexgrid);

                // if (Manager.Instance.gameManager.OutOfRange(currentRangeCellgrid)) continue;

                GridNode currentGridNode = entity.entityMovement.pathfinder.gridNodes.FirstOrDefault(node => node.cellgridPosition == currentRangeCellgrid);

                if (currentGridNode != null && !currentGridNode.isObstacle)
                {
                    foreach (Entity entity in Manager.Instance.gameManager.entities)
                    {
                        if (entity.entityMovement.currentCellgridPosition.Equals(currentRangeCellgrid))
                        {
                            foreach (CombatAbilityComponent combatAbilityComponent in currentSelectedCombatAbility.combatAbilityComponents)
                            {
                                combatAbilityComponent.ApplyCombatAbility(entity);
                            }
                        }
                    }
                }
            }
        }
    }

    private void MouseRightClick()
    {
        currentSelectedCombatAbility = null;
        ToggleCombatAbilityButtons(false);
    }

    public void DrawCastingRange(CombatAbility combatAbility)
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

    public void DrawAOE(CombatAbility combatAbility)
    {
        foreach (Vector3Int rangeHexgridOffset in combatAbility.AOEDictionary.Keys)
        {
            if (combatAbility.AOEDictionary[rangeHexgridOffset] == false) continue;

            Vector3Int currentRangeHexgrid = entity.entityMovement.pathfinder.CellgridToHexgrid(currentMouseCellgridPosition) + rangeHexgridOffset;
            Vector3Int currentRangeCellgrid = entity.entityMovement.pathfinder.HexgridToCellgrid(currentRangeHexgrid);

            // if (!entity.entityMovement.pathfinder.moveableTilemap.GetTile(currentRangeCellgrid)) continue;
            if (Manager.Instance.gameManager.OutOfRange(currentRangeCellgrid)) continue;

            GridNode currentGridNode = entity.entityMovement.pathfinder.gridNodes.FirstOrDefault(node => node.cellgridPosition == currentRangeCellgrid);

            if (currentGridNode != null && !currentGridNode.isObstacle)
            {
                aoeTilemap.SetTile(currentRangeCellgrid, entity.entityCombat.combatAbilityRangeHighlightedTileBase);
            }
        }
    }

    protected void ToggleCombatAbilityButtons(bool setActive)
    {
        foreach (GameObject combatAbilityButton in combatAbilityButtons)
        {
            combatAbilityButton.SetActive(setActive);
        }
    }

    private void GenerateCombatAbilityButtons()
    {
        if (entity.isSelected)
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
}
