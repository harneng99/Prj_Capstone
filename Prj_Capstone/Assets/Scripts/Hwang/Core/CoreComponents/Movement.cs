using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class Movement : CoreComponent
{
    [SerializeField] private Tilemap moveRangeHighlightedTilemap;
    [SerializeField] private TileBase moveRangeHighlightedTileBase;
    [SerializeField] private Vector3Int moveRangeInHexGrid;
    [SerializeField] private int maxMovementStamina;

    public Pathfinder pathfinder { get; private set; }

    public Vector3Int currentCellgridPosition { get; private set; }
    public Vector3Int currentHexgridPosition { get; private set; }
    private List<GridNode> path = new List<GridNode>();
    
    private bool isMoving;
    private Vector3 currentDestination;

    protected override void Awake()
    {
        base.Awake();

        pathfinder = GetComponent<Pathfinder>();

        entity.onPointerClick += ShowMoveableTiles;
    }

    private void Start()
    {
        entity.inputHandler.controls.Map.MouseLeftClick.performed += _ => MouseClick();

        currentCellgridPosition = pathfinder.moveableTilemap.WorldToCell(entity.GetEntityFeetPosition());
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
                    currentDestination = path.First().worldgridPosition;
                }
                else
                {
                    isMoving = false;
                    currentCellgridPosition = pathfinder.moveableTilemap.WorldToCell(entity.GetEntityFeetPosition());
                    currentHexgridPosition = pathfinder.CellgridToHexgrid(currentCellgridPosition);
                }
            }
            
            entity.transform.position = Vector3.MoveTowards(entity.transform.position, currentDestination + Vector3.up * entity.entityCollider.bounds.extents.y, entity.entityConsistentData.movementVelocity * Time.deltaTime);
        }
    }

    private void MouseClick()
    {
        if (!isMoving && entity.isSelected)
        {
            Vector2 mousePosition = entity.inputHandler.controls.Map.MousePosition.ReadValue<Vector2>();
            mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Vector3Int destinationCellgridPosition = pathfinder.moveableTilemap.WorldToCell(mousePosition);
            TileBase highlightedTile = moveRangeHighlightedTilemap.GetTile(destinationCellgridPosition);

            moveRangeHighlightedTilemap.ClearAllTiles();

            if (highlightedTile != null)
            {
                path = pathfinder.PathFinding(currentCellgridPosition, destinationCellgridPosition);
                isMoving = true;
                currentDestination = path.First().worldgridPosition;
            }
        }
    }

    public void ShowMoveableTiles()
    {
        if (!isMoving)
        {
            moveRangeHighlightedTilemap.ClearAllTiles();

            for (int x = -moveRangeInHexGrid.x; x <= moveRangeInHexGrid.x; x++)
            {
                for (int y = -moveRangeInHexGrid.y; y <= moveRangeInHexGrid.y; y++)
                {
                    for (int z = -moveRangeInHexGrid.z; z <= moveRangeInHexGrid.z; z++)
                    {
                        if (x + y + z != 0) continue;

                        Vector3Int moveableHexgridPosition = currentHexgridPosition + new Vector3Int(x, y, z);
                        Vector3Int? moveableCellgridPosition = pathfinder.HexgridToCellgrid(moveableHexgridPosition);

                        if (!moveableCellgridPosition.HasValue) continue;

                        GridNode currentGridNode = pathfinder.hexgridNodes.FirstOrDefault(node => node.cellgridPosition == moveableCellgridPosition.Value);

                        if (currentGridNode != null && !currentGridNode.isObstacle)
                        {
                            path = pathfinder.PathFinding(currentCellgridPosition, moveableCellgridPosition.Value);

                            bool pathOutOfRange = false;

                            foreach (GridNode gridNode in path)
                            {
                                if ((Mathf.Abs(gridNode.hexgridPosition.x - currentHexgridPosition.x) > moveRangeInHexGrid.x) || (Mathf.Abs(gridNode.hexgridPosition.y - currentHexgridPosition.y) > moveRangeInHexGrid.y) || (Mathf.Abs(gridNode.hexgridPosition.z - currentHexgridPosition.z) > moveRangeInHexGrid.z))
                                {
                                    pathOutOfRange = true;
                                    break;
                                }
                            }

                            if (!pathOutOfRange)
                            {
                                moveRangeHighlightedTilemap.SetTile(moveableCellgridPosition.Value, moveRangeHighlightedTileBase);
                            }
                        }
                    }
                }
            }
        }
    }
}
