using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class Movement : CoreComponent
{
    [SerializeField] private Tilemap highlightedTilemap;
    [SerializeField] private TileBase highlightedTileBase;
    [SerializeField] private Vector3Int moveRangeInHexGrid;
    [SerializeField] private int movementStamina;

    private Pathfinder pathfinder;
    private Grid gridBase;
    private Tilemap groundTilemap;

    private Vector3Int currentCellgridPosition;
    private Vector3Int currentHexgridPosition;
    private List<Vector3> path = new List<Vector3>();
    
    private bool isMoving;
    private Vector3 currentDestination;

    protected override void Awake()
    {
        base.Awake();

        pathfinder = GetComponent<Pathfinder>();
        gridBase = pathfinder.gridBase;
        groundTilemap = pathfinder.moveableTileMap;

        entity.onPointerClick += ShowMoveableTiles;
    }

    private void Start()
    {
        entity.inputHandler.controls.Map.MouseClick.performed += _ => MouseClick();

        currentCellgridPosition = groundTilemap.WorldToCell(entity.GetEntityFeetPosition());
        currentHexgridPosition = pathfinder.CellgridToHexgrid(currentCellgridPosition);
        entity.transform.position = pathfinder.HexgridToWorldgrid(currentHexgridPosition) + Vector3.up * entity.entityCollider.bounds.extents.y;
    }

    private void Update()
    {
        if (isMoving)
        {
            if (Vector3.Distance(entity.GetEntityFeetPosition(), currentDestination) < epsilon)
            {
                if (path.Count > 0)
                {
                    path.Remove(path.First());
                    currentDestination = path.First();
                }
                else
                {
                    isMoving = false;
                    currentCellgridPosition = groundTilemap.WorldToCell(entity.GetEntityFeetPosition());
                    currentHexgridPosition = pathfinder.CellgridToHexgrid(currentCellgridPosition);
                }
            }
            
            entity.transform.position = Vector3.MoveTowards(entity.transform.position, currentDestination + Vector3.up * entity.entityCollider.bounds.extents.y, entity.entityConsistentData.movementVelocity * Time.deltaTime);
        }
    }

    private void MouseClick()
    {
        if (!isMoving && Manager.Instance.gameManager.virtualCamera.Follow.Equals(entity.transform))
        {
            Vector2 mousePosition = entity.inputHandler.controls.Map.MousePosition.ReadValue<Vector2>();
            mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Vector3Int destinationCellgridPosition = groundTilemap.WorldToCell(mousePosition);
            TileBase highlightedTile = highlightedTilemap.GetTile(destinationCellgridPosition);

            highlightedTilemap.ClearAllTiles();
            if (highlightedTile != null)
            {
                path = pathfinder.PathFinding(currentCellgridPosition, destinationCellgridPosition);
                isMoving = true;
                currentDestination = path.First();
            }
        }
    }

    public void ShowMoveableTiles()
    {
        if (!isMoving)
        {
            highlightedTilemap.ClearAllTiles();

            for (int x = -moveRangeInHexGrid.x; x < moveRangeInHexGrid.x; x++)
            {
                for (int y = -moveRangeInHexGrid.y; y < moveRangeInHexGrid.y; y++)
                {
                    for (int z = -moveRangeInHexGrid.z; z < moveRangeInHexGrid.z; z++)
                    {
                        if (x + y + z != 0) continue;

                        Vector3Int moveableHexgridPosition = currentHexgridPosition + new Vector3Int(x, y, z);
                        Vector3Int? moveableCellgridPosition = pathfinder.HexgridToCellgrid(moveableHexgridPosition);

                        if (!moveableCellgridPosition.HasValue) continue;

                        HexgridNode currentHexgridNode = pathfinder.hexgridNodes.FirstOrDefault(node => node.cellgridPosition == moveableCellgridPosition);

                        if (currentHexgridNode != null && !currentHexgridNode.isObstacle)
                        {
                            highlightedTilemap.SetTile(moveableCellgridPosition.Value, highlightedTileBase);
                        }
                    }
                }
            }
        }
    }
}
